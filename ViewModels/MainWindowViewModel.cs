using Avalonia.Controls;
using ReactiveUI;
using SNESMiniLuaCompiler.Helpers;
using SNESMiniLuaCompiler.Models;
using System;
using System.Collections.Generic;
using System.Windows.Input;


namespace SNESMiniLuaCompiler.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
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

        private bool _isDecodeButtonEnabled;
        public bool IsDecodeButtonEnabled
        {
            get => _isDecodeButtonEnabled;
            set => this.RaiseAndSetIfChanged(ref _isDecodeButtonEnabled, value);
        }

        public MainWindowViewModel()
        {
            // Initialize other properties...

            SelectConsoleCommand = new RelayCommand<SystemModel>(OnSelectConsole);
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
            },
            new Consoles
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/snes.png")),
                Name = "SNES Mini",
                Region = "(USA)",
                SystemModel = SystemModel.Snes,
            },
            new Consoles
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/snespal.png")),
                Name = "SNES Mini",
                Region = "(Europe)",
                SystemModel = SystemModel.SnesPal,
            },
            new Consoles
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/famicom.png")),
                Name = "Famicom",
                Region = "Mini",
                SystemModel = SystemModel.Famicom,
            },
            new Consoles
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/sfamicom.png")),
                Name = "Super",
                Region = "Famicom",
                SystemModel = SystemModel.SuperFamicom,
            },
            new Consoles
            {
                ImagePath = ImageHelper.LoadFromResource(new Uri($"{AssetUriPrefix}/shonen.png")),
                Name = "Famicom",
                Region = "Shonen 50th",
                SystemModel = SystemModel.Shonen,
            },
        ];

        private void OnSelectConsole(SystemModel systemModel)
        {
            SelectedConsole = systemModel;

            // ViewModels should not directly access UI controls.
            // Instead, expose a property and bind IsEnabled in the View (XAML) to this property.
            AppUtils.EnsureDirectoryExists(AppUtils.DecodedPath);
            IsRecodeButtonEnabled = FileUtils.SafeDirectoryExists(AppUtils.RecodedPath);
            IsDecodeButtonEnabled = FileUtils.SafeDirectoryExists(AppUtils.DecodedPath);
            FileUtils.DeletePath(AppUtils.ResourcesPath);
            AppUtils.ExtractSelectedResources(systemModel);

            //FileUtils.SafeDirectoryExists(DecodedPath) && FileUtils.SafeDirectoryExists(RecodedPath);
        }


    }
}
