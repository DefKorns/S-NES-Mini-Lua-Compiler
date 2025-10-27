using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SNESMiniLuaCompiler.Helpers;
using SNESMiniLuaCompiler.Models;
using SNESMiniLuaCompiler.ViewModels;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SNESMiniLuaCompiler.Views
{
    public partial class MainWindow : Window
    {
        private string? _selectedSystem;

        public MainWindow()
        {
            InitializeComponent();
            InitializeFirstRunCheck();
            InitializeButtonStates();
        }

        /// <summary>
        /// Checks if this is the first run and verifies Python installation.
        /// </summary>
        private async static void InitializeFirstRunCheck()
        {
            const string PythonRequirementMessage = "Make sure you have python 3.x installed!\n\nPlease download it from python.org";

            ExceptionUtils.GlobalTryCatch(async () =>
            {
                // Check if Python 3.x is installed
                if (!ProcessUtils.PythonVersion())
                {
                    //// MessageBox.ShowError(
                    ////    PythonRequirementMessage,
                    ////    "Requirement"
                    ////);
                    ////MsgBox.Show(
                    ////    PythonRequirementMessage,
                    ////    "Requirement",
                    ////    MsgBox.ButtonType.OK,
                    ////    MsgBox.Ico.Application,
                    ////    style: MsgBox.AnimateStyle.FadeInHelp
                    ////);
                    ////throw new Exception(PythonRequirementMessage);
                    //ExceptionUtils.HandleException(
                    //    new Exception("Python 3.x is not installed."),
                    //    PythonRequirementMessage,
                    //    "MainForm.InitializeFirstRunCheck",
                    //    showUser: true
                    //);
                    await MessageBox.ShowError(
                        PythonRequirementMessage,
                        "Requirement"
                    );
                    // Close the application after the user clicks OK
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.Shutdown();
                    }
                    return;
                }

                // Remove the first-run marker file if it exists
                if (FileUtils.SafeFileExists(AppUtils.FirstRun))
                {
                    File.Delete(AppUtils.FirstRun);
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
            // Use the utility method to check both directories at once
            bool enable = AppUtils.AreDecodedAndRecodedDirsPresent();

            var decryptButton = this.FindControl<Button>("btn_decrypt");
            var encryptButton = this.FindControl<Button>("btn_encrypt");

            if (decryptButton != null)
                decryptButton.IsEnabled = enable;
            if (encryptButton != null)
                encryptButton.IsEnabled = enable;
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

            if (decryptButton != null)
                decryptButton.IsEnabled = false;
            if (encryptButton != null)
                encryptButton.IsEnabled = false;
            if (string.IsNullOrEmpty(ProcessUtils.FindExePath("python.exe")))
            {
                //MsgBox.Show("Python 3.x wasn't found on your system's PATH.\nPlease install it from python.org", "Error", MsgBox.ButtonType.OK, MsgBox.Ico.Warning);
                return;
            }

            var vm = DataContext as MainWindowViewModel;
            var selectedConsole = vm?.SelectedConsole;
            message.Text = "Decrypting files, please wait...";
            switch (selectedConsole)
            {
                case SystemModel.Nes:
                    _selectedSystem = AppUtils.GetSystemPath(SystemModel.Nes);
                    break;
                case SystemModel.Famicom:
                    _selectedSystem = AppUtils.GetSystemPath(SystemModel.Famicom);
                    break;
                case SystemModel.SnesPal:
                    _selectedSystem = AppUtils.GetSystemPath(SystemModel.SnesPal);
                    break;
                default:
                    //    //MsgBox.Show("Please select a console first.", "Error", MsgBox.ButtonType.OK, MsgBox.Ico.Warning);
                    //    EnableAllButtons(true);
                    _selectedSystem = AppUtils.GetSystemPath(SystemModel.Snes);
                    return;
            }

            await Task.Run(() =>
            {
                FileUtils.DeletePath(AppUtils.DecodedPath);
                FileUtils.CopyAssets(AppUtils.ResourcesPath, AppUtils.DecodedPath);
                FileUtils.DeleteFile(FileUtils.DecodedHashFile);
                Decrypt("decoded");
            }).ConfigureAwait(false);
            //EnableAllButtons(true);
            //decode_button.FlatAppearance.BorderSize = 2;
            //recode_button.FlatAppearance.BorderSize = 2;
            //AppUtils.LoadSpinner(false, picLoader, this);
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
                message.Text = "Done!";
            });

            //Action showDialog = ShowDecryptionFinishedDialog;
            //if (InvokeRequired)
            //    Invoke(showDialog);
            //else
            //    showDialog();
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
                        ProcessUtils.RunCmd(ProcessUtils.FindExePath("pythonw.exe"), AppUtils.DecompilerScript + " --file " + file + " --output " + decFile + " --catch_asserts");
                        File.Delete(file);
                        File.Move(decFile, file);
                        FileUtils.GenerateFileHash(file);
                    },
                    $"Error decrypting file '{file}'.",
                    "MainForm.Decrypt"
                );
            }
        }

        private async void EncryptButton_Click(object? sender, RoutedEventArgs e)
        {
            // TODO: Add your decryption logic here
            await EncryptFilesAsync().ConfigureAwait(false);
        }

        private async Task EncryptFilesAsync()
        {
            //ResetButtonBackColor();
            FileUtils.DeletePath(AppUtils.RecodedPath);
            //_activeButton = false;

            //EnableAllButtons(false);
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

            //AppUtils.LoadSpinner(false, picLoader, this);

            //EnableAllButtons(true);
            //EnableControls(new Control[] { decode_button }, false);
            //recode_button.FlatAppearance.BorderSize = 2;
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
                message.Text = "Done!";
            });
        }

        private static void Encrypt(string sDir)
        {
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
                string recodedFullPath = Path.GetFullPath(Regex.Replace(decodedFullPath, "decoded", "recoded"));
                string recodedFile = Path.GetFullPath(Regex.Replace(decodedFullPath, "decoded", "recoded"));

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
                            ProcessUtils.RunLuaJit(decodedFullPath, recodedFile);
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
    }
}
