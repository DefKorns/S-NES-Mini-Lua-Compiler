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
            if (Vm is not { } vm)
                return;

            await Dispatcher.UIThread.InvokeAsync(vm.BeginDecryption);

            string selectedSystem = AppUtils.GetSystemPath(vm.SelectedConsole ?? SystemModel.Snes);

            await Task.Run(() =>
            {
                FileUtils.DeletePath(AppUtils.DecodedPath);
                FileUtils.CopyAssets(selectedSystem, AppUtils.DecodedPath);
                FileUtils.DeleteFile(FileUtils.DecodedHashFile);
            }).ConfigureAwait(false);

            int totalFiles = FileUtils.CountFilesRecursive(AppUtils.DecodedPath);
            if (totalFiles == 0)
            {
                await Dispatcher.UIThread.InvokeAsync(() => vm.EndDecryption("No files to decrypt.", 0));
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
                        Dispatcher.UIThread.Post(() => vm.DecryptionProgress = progress);
                    }
                }
                DecryptWithProgress(AppUtils.DecodedPath);
            }).ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                FileUtils.CreatePath(AppUtils.RecodedPath);
                NotificationHelper.Success("Decryption complete!", "Light");
                vm.EndDecryption("Done!", 100);
            });
        }

        private async void TrashButton_Click(object? sender, RoutedEventArgs e)
        {
            FileUtils.DeletePath(AppUtils.DecodedPath);
            FileUtils.DeletePath(AppUtils.RecodedPath);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                NotificationHelper.Information("Temporary files deleted.", "Light");
                if (Vm is { } vm)
                {
                    vm.UpdateButtonStates();
                    vm.StatusText = "Temporary files deleted.";
                }
            });
        }

        private async void EncryptButton_Click(object? sender, RoutedEventArgs e)
            => await EncryptFilesAsync().ConfigureAwait(false);

        private async Task EncryptFilesAsync()
        {
            if (Vm is not { } vm)
                return;

            FileUtils.DeletePath(AppUtils.RecodedPath);

            // read the system once before the loop
            string? selectedConsoleStr = AppUtils.LoadConfig("system");

            int totalFiles = FileUtils.CountFilesRecursive(AppUtils.DecodedPath);
            await Dispatcher.UIThread.InvokeAsync(() => vm.BeginEncryption(totalFiles > 0));

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
                        Dispatcher.UIThread.Post(() => vm.EncryptionProgress = progress);
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
                FileUtils.CreatePath(AppUtils.RecodedPath);

                if (Directory.GetDirectories(AppUtils.RecodedPath).Length == 0)
                {
                    NotificationHelper.Warning("No files were encrypted.", "Light");
                    vm.EndEncryption("No files were encrypted.");
                }
                else
                {
                    NotificationHelper.Success("Encryption complete!", "Light");
                    vm.EndEncryption("Done!");
                }
            });
        }

        [GeneratedRegex("decoded")]
        private static partial Regex DecodedPathRegex();
    }
}
