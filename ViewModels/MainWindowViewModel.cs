using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Semi.Avalonia;
using SNESMiniLuaCompiler.Helpers;
using SNESMiniLuaCompiler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace SNESMiniLuaCompiler.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private const string AssetUriPrefix = "avares://SNESMiniLuaCompiler/Assets";
        public static string AppVersion => AppUtils.GetAppVersion();
        public ICommand SelectConsoleCommand { get; }

        private SystemModel? _selectedConsole;
        public SystemModel? SelectedConsole
        {
            get => _selectedConsole;
            set => this.RaiseAndSetIfChanged(ref _selectedConsole, value);
        }

        private bool _isRecodeButtonEnabled;
        public bool IsRecodeButtonEnabled
        {
            get => _isRecodeButtonEnabled;
            set => this.RaiseAndSetIfChanged(ref _isRecodeButtonEnabled, value);
        }

        //private bool _isDecodeButtonEnabled;
        //public bool IsDecodeButtonEnabled
        //{
        //    get => _isDecodeButtonEnabled;
        //    set => this.RaiseAndSetIfChanged(ref _isDecodeButtonEnabled, value);
        //}

        private bool _isTrashButtonEnabled;
        public bool IsTrashButtonEnabled
        {
            get => _isTrashButtonEnabled;
            set => this.RaiseAndSetIfChanged(ref _isTrashButtonEnabled, value);
        }

        private double _decryptionProgress;
        public double DecryptionProgress
        {
            get => _decryptionProgress;
            set => this.RaiseAndSetIfChanged(ref _decryptionProgress, value);
        }

        private bool _isDecryptionInProgress;
        public bool IsDecryptionInProgress
        {
            get => _isDecryptionInProgress;
            set => this.RaiseAndSetIfChanged(ref _isDecryptionInProgress, value);
        }

        private double _encryptionProgress;
        public double EncryptionProgress
        {
            get => _encryptionProgress;
            set => this.RaiseAndSetIfChanged(ref _encryptionProgress, value);
        }

        private bool _isEncryptionInProgress;
        public bool IsEncryptionInProgress
        {
            get => _isEncryptionInProgress;
            set => this.RaiseAndSetIfChanged(ref _isEncryptionInProgress, value);
        }

        public Func<Task>? DecryptFilesAsyncDelegate { get; set; }

        public MainWindowViewModel()
        {
            SelectConsoleCommand = new RelayCommand<SystemModel>(OnSelectConsole);
            UpdateButtonStates();
        }

        /// <summary>
        /// List of available consoles, including their image, name, region, and system model.
        /// </summary>
        public List<Consoles> ConsoleList { get; } = [
            new Consoles
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/nes.png")),
                Name = "NES Classic",
                Region = "Edition",
                SystemModel = SystemModel.Nes,
                IsChecked = false
            },
            new Consoles
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/snes.png")),
                Name = "SNES Mini",
                Region = "(USA)",
                SystemModel = SystemModel.Snes,
                IsChecked = false
            },
            new Consoles
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/snespal.png")),
                Name = "SNES Mini",
                Region = "(Europe)",
                SystemModel = SystemModel.SnesPal,
                IsChecked = false
            },
            new Consoles
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/famicom.png")),
                Name = "Famicom",
                Region = "Mini",
                SystemModel = SystemModel.Famicom,
                IsChecked = false
            },
            new Consoles
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/sfamicom.png")),
                Name = "Super",
                Region = "Famicom",
                SystemModel = SystemModel.SuperFamicom,
                IsChecked = false
            },
            new Consoles
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/shonen.png")),
                Name = "Famicom",
                Region = "Shonen 50th",
                SystemModel = SystemModel.Shonen,
                IsChecked = false
            },
        ];

        private async void OnSelectConsole(SystemModel systemModel)
        {
            SelectedConsole = systemModel;

            foreach (var console in ConsoleList)
            {
                console.IsChecked = console.SystemModel == systemModel;
            }

            if (FileUtils.SafeDirectoryExists(AppUtils.DecodedPath) && Directory.EnumerateFileSystemEntries(AppUtils.DecodedPath).Any())
            {
                var result = await Views.MessageBox.ShowWarning(
                    "This action may overwrite existing files. Do you want to continue?",
                    "Warning"
                );

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            AppUtils.SaveConfig("system", Path.GetFileName(AppUtils.GetSystemPath(systemModel)));
            AppUtils.EnsureDirectoryExists(AppUtils.DecodedPath);
            UpdateButtonStates();
            FileUtils.DeletePath(AppUtils.ResourcesPath);
            AppUtils.ExtractSelectedResources(systemModel);

            // Call the delegate to trigger decryption in the View
            if (DecryptFilesAsyncDelegate is not null)
                await DecryptFilesAsyncDelegate.Invoke();
        }

        public void UpdateButtonStates()
        {
            bool hasConsole = SelectedConsole.HasValue;
            //IsDecodeButtonEnabled = hasConsole && FileUtils.SafeFileExists(AppUtils.ConfigFile) && FileUtils.SafeDirectoryExists(AppUtils.DecodedPath);
            IsRecodeButtonEnabled = FileUtils.SafeFileExists(AppUtils.ConfigFile) && FileUtils.SafeDirectoryExists(AppUtils.RecodedPath);
            IsTrashButtonEnabled = hasConsole && FileUtils.SafeDirectoryExists(AppUtils.DecodedPath) || FileUtils.SafeDirectoryExists(AppUtils.RecodedPath);
        }


        [RelayCommand]
        private static void FollowSystemTheme()
            => Application.Current?.RegisterFollowSystemTheme();

        [RelayCommand]
        private static void ToggleTheme()
        {
            var app = Application.Current;
            if (app is null) return;
            var theme = app.ActualThemeVariant;
            app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
            app.UnregisterFollowSystemTheme();
        }

        [RelayCommand]
        private static void SelectTheme(object? obj)
        {
            var app = Application.Current;
            if (app is null) return;
            app.RequestedThemeVariant = obj as ThemeVariant;
            app.UnregisterFollowSystemTheme();
        }
    }
}
