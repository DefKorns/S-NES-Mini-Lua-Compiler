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
        private SystemModel? _selectedConsole;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanEncode))]
        private bool _isRecodeButtonEnabled;

        [ObservableProperty]
        private string _statusText = "Ready...";

        [ObservableProperty]
        private bool _isTrashButtonEnabled;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowSelectConsoleHint))]
        private double _decryptionProgress;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowSelectConsoleHint))]
        [NotifyPropertyChangedFor(nameof(CanEncode))]
        private bool _isDecryptionInProgress;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowSelectConsoleHint))]
        private double _encryptionProgress;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowSelectConsoleHint))]
        [NotifyPropertyChangedFor(nameof(CanEncode))]
        private bool _isEncryptionInProgress;

        public bool ShowSelectConsoleHint =>
            !SelectedConsole.HasValue && !IsDecryptionInProgress && !IsEncryptionInProgress;

        public bool CanEncode => IsRecodeButtonEnabled && !IsDecryptionInProgress && !IsEncryptionInProgress;

        public Func<Task>? DecryptFilesAsyncDelegate { get; set; }

        public MainWindowViewModel()
        {
            UpdateButtonStates();
        }

        public List<Consoles> ConsoleList { get; } =
        [
            new Consoles { ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/nes.png")),      Name = "NES Classic", Region = "Edition",     SystemModel = SystemModel.Nes,          IsEnabled = true },
            new Consoles { ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/snes.png")),     Name = "SNES Mini",   Region = "(USA)",       SystemModel = SystemModel.Snes,         IsEnabled = true },
            new Consoles { ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/snespal.png")),  Name = "SNES Mini",   Region = "(Europe)",    SystemModel = SystemModel.SnesPal,      IsEnabled = true },
            new Consoles { ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/famicom.png")),  Name = "Famicom",     Region = "Mini",        SystemModel = SystemModel.Famicom,      IsEnabled = true },
            new Consoles { ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/sfamicom.png")), Name = "Super",       Region = "Famicom",     SystemModel = SystemModel.SuperFamicom, IsEnabled = true },
            new Consoles { ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/shonen.png")),   Name = "Famicom",     Region = "Shonen 50th", SystemModel = SystemModel.Shonen,       IsEnabled = true },
        ];

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
            StatusText = "Decrypting files, please wait...";
            DecryptionProgress = 0;
            IsDecryptionInProgress = true;
            IsTrashButtonEnabled = false;
            SetConsolesEnabled(false);
        }

        public void EndDecryption(string statusText, double progress)
        {
            StatusText = statusText;
            DecryptionProgress = progress;
            IsDecryptionInProgress = false;
            SetConsolesEnabled(true);
            UpdateButtonStates();
            IsTrashButtonEnabled = true;
        }

        public void BeginEncryption(bool hasFiles)
        {
            StatusText = "Encrypting files, please wait...";
            EncryptionProgress = 0;
            IsEncryptionInProgress = hasFiles;
            SetConsolesEnabled(false);
        }

        public void EndEncryption(string statusText)
        {
            StatusText = statusText;
            EncryptionProgress = 100;
            IsEncryptionInProgress = false;
            SetConsolesEnabled(true);
        }

        private void SetConsolesEnabled(bool isEnabled)
        {
            foreach (Consoles console in ConsoleList)
                console.IsEnabled = isEnabled;
        }

        [RelayCommand]
        private static void FollowSystemTheme()
            => Application.Current?.RegisterFollowSystemTheme();

        [RelayCommand]
        private static void ToggleTheme()
        {
            Application? app = Application.Current;
            if (app is null)
                return;
            app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
            app.UnregisterFollowSystemTheme();
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
