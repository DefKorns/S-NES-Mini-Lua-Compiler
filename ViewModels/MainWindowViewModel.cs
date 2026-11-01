using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Semi.Avalonia;
using SNESMiniLuaCompiler.Helpers;
using SNESMiniLuaCompiler.Models;

namespace SNESMiniLuaCompiler.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private const string AssetUriPrefix = "avares://SNESMiniLuaCompiler/Assets";
        public static string AppVersion => AppUtils.GetAppVersion();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowSelectConsoleHint))]
        [NotifyPropertyChangedFor(nameof(SelectedConsoleName))]
        private SystemModel? _selectedConsole;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanEncode))]
        private bool _isRecodeButtonEnabled;

        [ObservableProperty]
        private bool _isTrashButtonEnabled;

        // the app starts in the dark theme (App.axaml)
        [ObservableProperty]
        private bool _isDarkTheme = true;

        [ObservableProperty]
        private string _statusText = "Ready...";

        [ObservableProperty]
        private double _decryptionProgress;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBusy))]
        [NotifyPropertyChangedFor(nameof(StatusPillText))]
        [NotifyPropertyChangedFor(nameof(ShowSelectConsoleHint))]
        [NotifyPropertyChangedFor(nameof(ShowResult))]
        [NotifyPropertyChangedFor(nameof(CanOpenFolder))]
        [NotifyPropertyChangedFor(nameof(CanEncode))]
        private bool _isDecryptionInProgress;

        [ObservableProperty]
        private double _encryptionProgress;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBusy))]
        [NotifyPropertyChangedFor(nameof(StatusPillText))]
        [NotifyPropertyChangedFor(nameof(ShowSelectConsoleHint))]
        [NotifyPropertyChangedFor(nameof(ShowResult))]
        [NotifyPropertyChangedFor(nameof(CanOpenFolder))]
        [NotifyPropertyChangedFor(nameof(CanEncode))]
        private bool _isEncryptionInProgress;

        // what the last decode/encode did, shown in the panel under the consoles
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowResult))]
        [NotifyPropertyChangedFor(nameof(CanOpenFolder))]
        private string? _resultTitle;

        [ObservableProperty]
        private string? _resultDetail;

        [ObservableProperty]
        private bool _resultIsSuccess = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanOpenFolder))]
        private string? _folderToOpen;

        public bool IsBusy => IsDecryptionInProgress || IsEncryptionInProgress;

        public string StatusPillText => IsBusy ? "Working" : "Ready";

        public string SelectedConsoleName =>
            ConsoleList.FirstOrDefault(c => c.SystemModel == SelectedConsole)?.FullName ?? "None";

        public bool ShowSelectConsoleHint => !SelectedConsole.HasValue && !IsBusy;

        public bool ShowResult => ResultTitle is not null && !IsBusy;

        public bool CanOpenFolder => ShowResult && FolderToOpen is not null;

        public bool CanEncode => IsRecodeButtonEnabled && !IsBusy;

        public Func<Task>? DecryptFilesAsyncDelegate { get; set; }

        public MainWindowViewModel()
        {
            UpdateButtonStates();
        }

        public List<Consoles> ConsoleList { get; } =
        [
            NewConsole("nes.png",      "NES Classic", "",         "NES Classic Edition",      SystemModel.Nes),
            NewConsole("snes.png",     "SNES Mini",   "(USA)",    "SNES Mini (USA)",          SystemModel.Snes),
            NewConsole("snespal.png",  "SNES Mini",   "(Europe)", "SNES Mini (Europe)",       SystemModel.SnesPal),
            NewConsole("famicom.png",  "Famicom",     "Mini",     "Famicom Mini",             SystemModel.Famicom),
            NewConsole("sfamicom.png", "Super",       "Famicom",  "Super Famicom",            SystemModel.SuperFamicom),
            NewConsole("shonen.png",   "Shonen 50th", "",         "Famicom Shonen Jump 50th", SystemModel.Shonen),
        ];

        private static Consoles NewConsole(string image, string name, string region, string fullName, SystemModel systemModel) =>
            new()
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/{image}")),
                Name = name,
                Region = region,
                FullName = fullName,
                SystemModel = systemModel,
                IsEnabled = true
            };

        [RelayCommand]
        private async Task SelectConsole(SystemModel systemModel)
        {
            SelectedConsole = systemModel;

            foreach (Consoles console in ConsoleList)
                console.IsChecked = console.SystemModel == systemModel;

            if (FileUtils.SafeDirectoryExists(AppUtils.DecodedPath) &&
                Directory.EnumerateFileSystemEntries(AppUtils.DecodedPath).Any())
            {
                MessageBoxResult result = await Views.MessageBox.ShowWarning(
                    "This action may overwrite existing files. Do you want to continue?",
                    "Warning"
                );
                if (result != MessageBoxResult.Yes)
                    return;
            }

            AppUtils.SaveConfig("system", Path.GetFileName(AppUtils.GetSystemPath(systemModel)));
            AppUtils.EnsureDirectoryExists(AppUtils.DecodedPath);
            UpdateButtonStates();
            FileUtils.DeletePath(AppUtils.ResourcesPath);
            AppUtils.ExtractSelectedResources(systemModel);

            if (DecryptFilesAsyncDelegate is not null)
                await DecryptFilesAsyncDelegate.Invoke();
        }

        public void UpdateButtonStates()
        {
            IsRecodeButtonEnabled = FileUtils.SafeFileExists(AppUtils.ConfigFile) &&
                                    FileUtils.SafeDirectoryExists(AppUtils.RecodedPath);
            IsTrashButtonEnabled = (SelectedConsole.HasValue && FileUtils.SafeDirectoryExists(AppUtils.DecodedPath)) ||
                                    FileUtils.SafeDirectoryExists(AppUtils.RecodedPath);
        }

        // The work itself runs in MainWindow; these only change what the window shows. Call them on the UI thread.
        public void BeginDecryption()
        {
            ClearResult();
            StatusText = "Decrypting files, please wait...";
            DecryptionProgress = 0;
            IsDecryptionInProgress = true;
            IsTrashButtonEnabled = false;
            SetConsolesEnabled(false);
        }

        public void EndDecryption(bool hasFiles)
        {
            IsDecryptionInProgress = false;
            DecryptionProgress = hasFiles ? 100 : 0;
            SetConsolesEnabled(true);
            UpdateButtonStates();
            IsTrashButtonEnabled = true;

            if (hasFiles)
            {
                StatusText = "Done!";
                SetResult("Decode completed!", "Files extracted. Edit them, then press Encode.", true, AppUtils.DecodedPath);
            }
            else
            {
                StatusText = "No files to decrypt.";
                SetResult("Nothing to decode", "No files were found for this console.", false, null);
            }
        }

        public void BeginEncryption(bool hasFiles)
        {
            ClearResult();
            StatusText = "Encrypting files, please wait...";
            EncryptionProgress = 0;
            IsEncryptionInProgress = hasFiles;
            SetConsolesEnabled(false);
        }

        public void EndEncryption(int encodedFiles)
        {
            EncryptionProgress = 100;
            IsEncryptionInProgress = false;
            SetConsolesEnabled(true);
            UpdateButtonStates();

            if (encodedFiles == 0)
            {
                StatusText = "No files encoded (nothing was edited).";
                SetResult("Nothing to encode", "No file was changed since it was decoded.", false, null);
            }
            else
            {
                string files = encodedFiles == 1 ? "1 file" : $"{encodedFiles} files";
                StatusText = $"Done! {files} encoded.";
                SetResult("Encode completed!", $"{files} encoded. Copy them to your theme.", true, AppUtils.RecodedPath);
            }
        }

        public void ClearResult() => SetResult(null, null, true, null);

        private void SetResult(string? title, string? detail, bool isSuccess, string? folder)
        {
            ResultTitle = title;
            ResultDetail = detail;
            ResultIsSuccess = isSuccess;
            FolderToOpen = folder;
        }

        private void SetConsolesEnabled(bool isEnabled)
        {
            foreach (Consoles console in ConsoleList)
                console.IsEnabled = isEnabled;
        }

        [RelayCommand]
        private void OpenFolder()
        {
            if (FolderToOpen is not null)
                ProcessUtils.OpenDirectory(FolderToOpen);
        }

        [RelayCommand]
        private static void FollowSystemTheme()
            => Application.Current?.RegisterFollowSystemTheme();

        [RelayCommand]
        private void ToggleTheme()
        {
            Application? app = Application.Current;
            if (app is null)
                return;
            ThemeVariant next = app.ActualThemeVariant == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
            app.RequestedThemeVariant = next;
            app.UnregisterFollowSystemTheme();
            IsDarkTheme = next == ThemeVariant.Dark;
        }

        [RelayCommand]
        private static void SelectTheme(object? obj)
        {
            Application? app = Application.Current;
            if (app is null)
                return;
            app.RequestedThemeVariant = obj as ThemeVariant;
            app.UnregisterFollowSystemTheme();
        }
    }
}
