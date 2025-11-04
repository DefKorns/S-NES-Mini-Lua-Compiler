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
using System.Threading;
using System.Threading.Tasks;


namespace SNESMiniLuaCompiler.Views
{
    public partial class MainWindow : Window
    {
        private string? _selectedSystem;

        public MainWindow()
        {
            InitializeComponent();
            NotificationHelper.Initialize(this);
            InitializeFirstRunCheck();
            InitializeButtonStates();

            this.DataContextChanged += (_, e) =>
            {
                if (DataContext is MainWindowViewModel vm)
                    vm.DecryptFilesAsyncDelegate = DecryptFilesAsync;
            };
        }

        private static void InitializeFirstRunCheck()
        {
            const string PythonRequirementMessage = "Make sure you have python 3.x installed!\n\nPlease download it from python.org";

            ExceptionUtils.GlobalTryCatch(() =>
            {
                if (!ProcessUtils.PythonVersion())
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        await MessageBox.ShowError(
                            PythonRequirementMessage,
                            "Requirement"
                        );
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

        private void InitializeButtonStates()
        {
            var vm = DataContext as MainWindowViewModel;
            vm?.UpdateButtonStates();
        }


        public async Task RunDecryptFilesAsyncFromViewModel()
        {
            await DecryptFilesAsync().ConfigureAwait(false);
        }

        private async Task DecryptFilesAsync()
        {
            var encryptButton = this.FindControl<Button>("btn_encrypt");
            var vm = DataContext as MainWindowViewModel;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                encryptButton?.SetValue(IsEnabledProperty, false);
                status.Text = "Decrypting files, please wait...";
                if (vm != null)
                {
                    vm.DecryptionProgress = 0;
                    vm.IsDecryptionInProgress = true;
                    vm.IsTrashButtonEnabled = false;

                    foreach (var console in vm.ConsoleList)
                    {
                        console.IsEnabled = false;
                    }
                }
            });

            if (string.IsNullOrEmpty(ProcessUtils.FindExePath("python.exe")))
                return;

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
            }).ConfigureAwait(false);

            int totalFiles = FileUtils.CountFilesRecursive("decoded");
            if (totalFiles == 0)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    status.Text = "No files to decrypt.";
                    if (vm != null)
                    {
                        vm.DecryptionProgress = 0;
                        vm.IsDecryptionInProgress = false;
                        vm.IsTrashButtonEnabled = true;

                        foreach (var console in vm.ConsoleList)
                        {
                            console.IsEnabled = true;
                        }
                    }
                    encryptButton?.SetValue(IsEnabledProperty, true);
                });
                return;
            }

            int filesDecrypted = 0;

            await Task.Run(() =>
            {
                void DecryptWithProgress(string sDir)
                {
                    foreach (var d in Directory.GetDirectories(sDir))
                    {
                        ExceptionUtils.GlobalTryCatch(
                            () => DecryptWithProgress(d),
                            $"Error decrypting directory '{d}'.",
                            "MainForm.Decrypt"
                        );
                    }

                    foreach (var file in Directory.GetFiles(sDir))
                    {
                        ExceptionUtils.GlobalTryCatch(
                            () =>
                            {
                                string decFile = file + ".dec";
                                ProcessUtils.RunDecompiler(file, decFile);
                                File.Delete(file);
                                File.Move(decFile, file);
                                FileUtils.GenerateFileHash(file);
                            },
                            $"Error decrypting file '{file}'.",
                            "MainForm.Decrypt"
                        );
                        int done = Interlocked.Increment(ref filesDecrypted);
                        double progress = (done * 100.0) / totalFiles;
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (vm != null)
                                vm.DecryptionProgress = progress;
                        });
                    }
                }
                DecryptWithProgress("decoded");
            }).ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                encryptButton?.SetValue(IsEnabledProperty, true);
                FileUtils.CreatePath(AppUtils.RecodedPath);
                NotificationHelper.Success("Decryption complete!", "Light");
                status.Text = "Done!";
                if (vm != null)
                {
                    vm.DecryptionProgress = 100;
                    vm.IsDecryptionInProgress = false;
                    vm.IsTrashButtonEnabled = true;

                    foreach (var console in vm.ConsoleList)
                    {
                        console.IsEnabled = true;
                    }
                }
            });
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
                status.Text = "Temporary files deleted.";
            });
        }

        private async void EncryptButton_Click(object? sender, RoutedEventArgs e)
        {
            await EncryptFilesAsync().ConfigureAwait(false);
        }

        private async Task EncryptFilesAsync()
        {
            FileUtils.DeletePath(AppUtils.RecodedPath);

            var encryptButton = this.FindControl<Button>("btn_encrypt");
            var vm = DataContext as MainWindowViewModel;

            if (encryptButton != null)
                encryptButton.IsEnabled = false;

            status.Text = "Encrypting files, please wait...";

            int totalFiles = FileUtils.CountFilesRecursive("decoded");
            if (vm != null)
            {
                vm.EncryptionProgress = 0;
                vm.IsEncryptionInProgress = totalFiles > 0;

                foreach (var console in vm.ConsoleList)
                {
                    console.IsEnabled = false;
                }
            }
            int filesEncrypted = 0;

            await Task.Run(() =>
            {
                void EncryptWithProgress(string sDir)
                {
                    foreach (string d in Directory.GetDirectories(sDir))
                    {
                        ExceptionUtils.GlobalTryCatch(
                            () => EncryptWithProgress(d),
                            $"Error encrypting directory '{d}'.",
                            "MainForm.Encrypt"
                        );
                    }

                    foreach (string decodedFile in Directory.GetFiles(sDir))
                    {
                        string decodedFullPath = Path.GetFullPath(decodedFile);
                        string? selectedConsoleStr = AppUtils.LoadConfig("system");
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

                        int done = Interlocked.Increment(ref filesEncrypted);
                        double progress = (totalFiles > 0) ? (done * 100.0 / totalFiles) : 100;
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (vm != null)
                                vm.EncryptionProgress = progress;
                        });
                    }
                }

                EncryptWithProgress("decoded");

                ExceptionUtils.GlobalTryCatch(
                    () => FileUtils.DeleteEmptyDirectories(AppUtils.RecodedPath),
                    $"Error deleting empty directories in '{AppUtils.RecodedPath}'.",
                    "MainForm.Encrypt"
                );
            }).ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (encryptButton != null)
                    encryptButton.IsEnabled = true;
                FileUtils.CreatePath(AppUtils.RecodedPath);

                bool noFiles = (Directory.GetDirectories(AppUtils.RecodedPath)?.Length ?? 0) == 0;
                if (noFiles)
                {
                    NotificationHelper.Warning("No files were encrypted.", "Light");
                    status.Text = "No files were encrypted.";
                }
                else
                {
                    NotificationHelper.Success("Encryption complete!", "Light");
                    status.Text = "Done!";
                }
                if (vm != null)
                {
                    vm.EncryptionProgress = 100;
                    vm.IsEncryptionInProgress = false;
                    foreach (var console in vm.ConsoleList)
                    {
                        console.IsEnabled = true;
                    }
                }
            });
        }

        [GeneratedRegex("decoded")]
        private static partial Regex DecodedPathRegex();

    }
}
