using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SNESMiniLuaCompiler.Helpers;
using SNESMiniLuaCompiler.Models;
using SNESMiniLuaCompiler.ViewModels;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace SNESMiniLuaCompiler.Views
{
    public partial class MainWindow : Window
    {
        private string? _selectedSystem;
        //private WindowNotificationManager? _manager;

        public MainWindow()
        {
            InitializeComponent();
            NotificationHelper.Initialize(this);
            InitializeFirstRunCheck();
            InitializeButtonStates();
        }

        /// <summary>
        /// Checks if this is the first run and verifies Python installation.
        /// </summary>
        private static void InitializeFirstRunCheck()
        {
            const string PythonRequirementMessage = "Make sure you have python 3.x installed!\n\nPlease download it from python.org";

            ExceptionUtils.GlobalTryCatch(() =>
            {
                // Check if Python 3.x is installed
                if (!ProcessUtils.PythonVersion())
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        await MessageBox.ShowError(
                            PythonRequirementMessage,
                            "Requirement"
                        );
                        // Close the application after the user clicks OK
                        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                        {
                            ExceptionUtils.LogException("Python 3.x is not installed. Application will shut down.");
                            desktop.Shutdown();
                        }
                    });
                    return;
                }
            },
            "An error occurred during the first run check.",
            "MainForm.InitializeFirstRunCheck");
        }

        /// <summary>
        /// Initializes the state of buttons based on directory existence and active button status.
        /// </summary>
        private void InitializeButtonStates()
        {
            var vm = DataContext as MainWindowViewModel;
            vm?.UpdateButtonStates();
        }

        private async void DecryptButton_Click(object? sender, RoutedEventArgs e)
        {
            // TODO: Add your decryption logic here
            await DecryptFilesAsync().ConfigureAwait(false);
        }

        private async Task DecryptFilesAsync()
        {
            var decryptButton = this.FindControl<Button>("btn_decrypt");
            var encryptButton = this.FindControl<Button>("btn_encrypt");

            // Disable buttons and update UI on the UI thread
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                decryptButton?.SetValue(IsEnabledProperty, false);
                encryptButton?.SetValue(IsEnabledProperty, false);
                message.Text = "Decrypting files, please wait...";
            });

            if (string.IsNullOrEmpty(ProcessUtils.FindExePath("python.exe")))
                return;

            var vm = DataContext as MainWindowViewModel;
            var selectedConsole = vm?.SelectedConsole;

            _selectedSystem = selectedConsole switch
            {
                SystemModel.Nes => AppUtils.GetSystemPath(SystemModel.Nes),
                SystemModel.Famicom => AppUtils.GetSystemPath(SystemModel.Famicom),
                SystemModel.SnesPal => AppUtils.GetSystemPath(SystemModel.SnesPal),
                SystemModel.SuperFamicom => AppUtils.GetSystemPath(SystemModel.SuperFamicom),
                SystemModel.Shonen => AppUtils.GetSystemPath(SystemModel.Shonen),
                _ => AppUtils.GetSystemPath(SystemModel.Snes)
            };

            await Task.Run(() =>
            {
                FileUtils.DeletePath(AppUtils.DecodedPath);
                FileUtils.CopyAssets(_selectedSystem, AppUtils.DecodedPath);
                FileUtils.DeleteFile(FileUtils.DecodedHashFile);
                Decrypt("decoded");
            }).ConfigureAwait(false);

            // Re-enable buttons and update UI on the UI thread
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                decryptButton?.SetValue(IsEnabledProperty, true);
                encryptButton?.SetValue(IsEnabledProperty, true);
                FileUtils.CreatePath(AppUtils.RecodedPath);
                NotificationHelper.Success("Decryption complete!", "Light");
                message.Text = "Done!";
            });
        }

        private static void Decrypt(string sDir)
        {
            foreach (string d in Directory.GetDirectories(sDir))
            {
                ExceptionUtils.GlobalTryCatch(
                    () => Decrypt(d),
                    $"Error decrypting directory '{d}'.",
                    "MainForm.Decrypt"
                );
            }

            foreach (string file in Directory.GetFiles(sDir))
            {
                string decFile = file + ".dec";
                ExceptionUtils.GlobalTryCatch(
                    () =>
                    {
                        //ProcessUtils.RunCmd(AppUtils.DecompilerScript + " --file " + file + " --output " + decFile + " --catch_asserts");
                        ProcessUtils.RunDecompiler(file, decFile);
                        File.Delete(file);
                        File.Move(decFile, file);
                        FileUtils.GenerateFileHash(file);
                    },
                    $"Error decrypting file '{file}'.",
                    "MainForm.Decrypt"
                );
            }
        }

        private async void TrashButton_Click(object? sender, RoutedEventArgs e)
        {

            FileUtils.DeletePath(AppUtils.DecodedPath);
            FileUtils.DeletePath(AppUtils.RecodedPath);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.UpdateButtonStates();
                NotificationHelper.Information("Temporary files deleted.", "Light");
                message.Text = "Temporary files deleted.";
            });
        }

        private async void EncryptButton_Click(object? sender, RoutedEventArgs e)
        {
            await EncryptFilesAsync().ConfigureAwait(false);
        }

        private async Task EncryptFilesAsync()
        {
            //ResetButtonBackColor();
            FileUtils.DeletePath(AppUtils.RecodedPath);

            var decryptButton = this.FindControl<Button>("btn_decrypt");
            var encryptButton = this.FindControl<Button>("btn_encrypt");

            if (decryptButton != null)
                decryptButton.IsEnabled = false;
            if (encryptButton != null)
                encryptButton.IsEnabled = false;

            //AppUtils.LoadSpinner(true, picLoader, this);
            var vm = DataContext as MainWindowViewModel;
            var selectedConsole = vm?.SelectedConsole;
            message.Text = "Encrypting files, please wait...";

            // Run Encrypt on a background thread to make the method truly async
            await Task.Run(() => Encrypt("decoded")).ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (decryptButton != null)
                    decryptButton.IsEnabled = true;
                if (encryptButton != null)
                    encryptButton.IsEnabled = true;
                FileUtils.CreatePath(AppUtils.RecodedPath);
            });

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                bool noFiles = (Directory.GetDirectories(AppUtils.RecodedPath)?.Length ?? 0) == 0;
                if (noFiles)
                {
                    NotificationHelper.Warning("No files were encrypted.", "Light");
                    message.Text = "No files were encrypted.";
                }
                else
                {
                    NotificationHelper.Success("Encryption complete!", "Light");
                    message.Text = "Done!";
                }
            });
        }

        private static void Encrypt(string sDir)
        {
            string? selectedConsoleStr = AppUtils.LoadConfig("system");

            foreach (string d in Directory.GetDirectories(sDir))
            {
                ExceptionUtils.GlobalTryCatch(
                    () => Encrypt(d),
                    $"Error encrypting directory '{d}'.",
                    "MainForm.Encrypt"
                );
            }

            foreach (string decodedFile in Directory.GetFiles(sDir))
            {
                string decodedFullPath = Path.GetFullPath(decodedFile);
                string recodedFullPath = Path.GetFullPath(DecodedPathRegex().Replace(decodedFullPath, $"recoded/{selectedConsoleStr}/resources"));

                ExceptionUtils.GlobalTryCatch(
                    () =>
                    {
                        var recodedDir = Path.GetDirectoryName(recodedFullPath);
                        if (!string.IsNullOrEmpty(recodedDir))
                        {
                            FileUtils.CreatePath(recodedDir);
                        }

                        if (FileUtils.HasEditedFiles(FileUtils.GetSHA256HashFromFile(decodedFullPath)))
                        {
                            ProcessUtils.RunLuaJit(decodedFullPath, recodedFullPath);
                        }
                    },
                    $"Error encrypting file '{decodedFile}'.",
                    "MainForm.Encrypt"
                );
            }

            ExceptionUtils.GlobalTryCatch(
                () => FileUtils.DeleteEmptyDirectories(AppUtils.RecodedPath),
                $"Error deleting empty directories in '{AppUtils.RecodedPath}'.",
                "MainForm.Encrypt"
            );
        }

        [GeneratedRegex("decoded")]
        private static partial Regex DecodedPathRegex();

    }
}
