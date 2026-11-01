using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SNESMiniLuaCompiler.Helpers;
using SNESMiniLuaCompiler.Models;
using SNESMiniLuaCompiler.ViewModels;

namespace SNESMiniLuaCompiler.Views
{
    public partial class MainWindow : Window
    {
        // propriedade cached em vez de repetir "DataContext as MainWindowViewModel"
        private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
            NotificationHelper.Initialize(this);

            this.DataContextChanged += (_, _) =>
            {
                if (Vm is { } vm)
                {
                    vm.DecryptFilesAsyncDelegate = DecryptFilesAsync;
                    vm.UpdateButtonStates();
                }
            };
        }

        private async Task DecryptFilesAsync()
        {
            Button? encryptButton = this.FindControl<Button>("btn_encrypt");
            MainWindowViewModel? vm = Vm;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (encryptButton != null)
                    encryptButton.IsEnabled = false;
                status.Text = "Decrypting files, please wait...";
                if (vm != null)
                {
                    vm.DecryptionProgress = 0;
                    vm.IsDecryptionInProgress = true;
                    vm.IsTrashButtonEnabled = false;
                    SetConsolesEnabled(vm, false);
                }
            });

            string selectedSystem = AppUtils.GetSystemPath(vm?.SelectedConsole ?? SystemModel.Snes);

            await Task.Run(() =>
            {
                FileUtils.DeletePath(AppUtils.DecodedPath);
                FileUtils.CopyAssets(selectedSystem, AppUtils.DecodedPath);
                FileUtils.DeleteFile(FileUtils.DecodedHashFile);
            }).ConfigureAwait(false);

            int totalFiles = FileUtils.CountFilesRecursive(AppUtils.DecodedPath);
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
                        SetConsolesEnabled(vm, true);
                    }
                    if (encryptButton != null)
                        encryptButton.IsEnabled = true;
                });
                return;
            }

            int filesDecrypted = 0;

            await Task.Run(() =>
            {
                void DecryptWithProgress(string sDir)
                {
                    foreach (string d in Directory.GetDirectories(sDir))
                        ExceptionUtils.GlobalTryCatch(
                            () => DecryptWithProgress(d),
                            $"Error decrypting directory '{d}'.",
                            "MainForm.Decrypt");

                    foreach (string file in Directory.GetFiles(sDir))
                    {
                        ExceptionUtils.GlobalTryCatch(() =>
                        {
                            string decFile = file + ".dec";
                            ProcessUtils.RunDecompiler(file, decFile);
                            File.Delete(file);
                            File.Move(decFile, file);
                            FileUtils.GenerateFileHash(file);
                        },
                        $"Error decrypting file '{file}'.",
                        "MainForm.Decrypt");

                        double progress = (Interlocked.Increment(ref filesDecrypted) * 100.0) / totalFiles;
                        Dispatcher.UIThread.Post(() => { if (vm != null) vm.DecryptionProgress = progress; });
                    }
                }
                DecryptWithProgress(AppUtils.DecodedPath);
            }).ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (encryptButton != null)
                    encryptButton.IsEnabled = true;
                FileUtils.CreatePath(AppUtils.RecodedPath);
                NotificationHelper.Success("Decryption complete!", "Light");
                status.Text = "Done!";
                if (vm != null)
                {
                    vm.DecryptionProgress = 100;
                    vm.IsDecryptionInProgress = false;
                    vm.IsTrashButtonEnabled = true;
                    SetConsolesEnabled(vm, true);
                }
            });
        }

        private async void TrashButton_Click(object? sender, RoutedEventArgs e)
        {
            FileUtils.DeletePath(AppUtils.DecodedPath);
            FileUtils.DeletePath(AppUtils.RecodedPath);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Vm?.UpdateButtonStates();
                NotificationHelper.Information("Temporary files deleted.", "Light");
                status.Text = "Temporary files deleted.";
            });
        }

        private async void EncryptButton_Click(object? sender, RoutedEventArgs e)
            => await EncryptFilesAsync().ConfigureAwait(false);

        private async Task EncryptFilesAsync()
        {
            FileUtils.DeletePath(AppUtils.RecodedPath);

            Button? encryptButton = this.FindControl<Button>("btn_encrypt");
            MainWindowViewModel? vm = Vm;

            // read the system once before the loop
            string? selectedConsoleStr = AppUtils.LoadConfig("system");

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (encryptButton != null)
                    encryptButton.IsEnabled = false;
                status.Text = "Encrypting files, please wait...";
            });

            int totalFiles = FileUtils.CountFilesRecursive(AppUtils.DecodedPath);
            if (vm != null)
            {
                vm.EncryptionProgress = 0;
                vm.IsEncryptionInProgress = totalFiles > 0;
                SetConsolesEnabled(vm, false);
            }

            int filesEncrypted = 0;

            await Task.Run(() =>
            {
                void EncryptWithProgress(string sDir)
                {
                    foreach (string d in Directory.GetDirectories(sDir))
                        ExceptionUtils.GlobalTryCatch(
                            () => EncryptWithProgress(d),
                            $"Error encrypting directory '{d}'.",
                            "MainForm.Encrypt");

                    foreach (string decodedFile in Directory.GetFiles(sDir))
                    {
                        string decodedFullPath = Path.GetFullPath(decodedFile);
                        string recodedFullPath = Path.GetFullPath(
                            DecodedPathRegex().Replace(decodedFullPath, $"recoded/{selectedConsoleStr}/resources"));

                        ExceptionUtils.GlobalTryCatch(() =>
                        {
                            string? recodedDir = Path.GetDirectoryName(recodedFullPath);
                            if (!string.IsNullOrEmpty(recodedDir))
                                FileUtils.CreatePath(recodedDir);

                            if (FileUtils.HasEditedFiles(FileUtils.GetSHA256HashFromFile(decodedFullPath)))
                                ProcessUtils.RunLuaJit(decodedFullPath, recodedFullPath);
                        },
                        $"Error encrypting file '{decodedFile}'.",
                        "MainForm.Encrypt");

                        double progress = totalFiles > 0
                            ? Interlocked.Increment(ref filesEncrypted) * 100.0 / totalFiles
                            : 100;
                        Dispatcher.UIThread.Post(() => { if (vm != null) vm.EncryptionProgress = progress; });
                    }
                }

                EncryptWithProgress(AppUtils.DecodedPath);
                ExceptionUtils.GlobalTryCatch(
                    () => FileUtils.DeleteEmptyDirectories(AppUtils.RecodedPath),
                    $"Error deleting empty directories in '{AppUtils.RecodedPath}'.",
                    "MainForm.Encrypt");
            }).ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (encryptButton != null)
                    encryptButton.IsEnabled = true;
                FileUtils.CreatePath(AppUtils.RecodedPath);

                bool noFiles = Directory.GetDirectories(AppUtils.RecodedPath).Length == 0;
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
                    SetConsolesEnabled(vm, true);
                }
            });
        }

        private static void SetConsolesEnabled(MainWindowViewModel? vm, bool isEnabled)
        {
            if (vm == null)
                return;

            foreach (Consoles console in vm.ConsoleList)
                console.IsEnabled = isEnabled;
        }

        [GeneratedRegex("decoded")]
        private static partial Regex DecodedPathRegex();
    }
}
