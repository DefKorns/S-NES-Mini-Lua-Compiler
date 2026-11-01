using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SNESMiniLuaCompiler.ViewModels;

namespace SNESMiniLuaCompiler.Views
{
    public partial class ImportOriginalsDialog : Window
    {
        public ImportOriginalsDialog()
        {
            InitializeComponent();
        }

        public ImportOriginalsDialog(ImportOriginalsViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
            viewModel.Imported += (_, _) => Close(true);
        }

        private ImportOriginalsViewModel? Vm => DataContext as ImportOriginalsViewModel;

        private async void ChooseFile_Click(object? sender, RoutedEventArgs e)
        {
            IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choose the firmware file",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Firmware (.hsqs, .tar, .tar.gz)") { Patterns = ["*.hsqs", "*.squashfs", "*.tar", "*.gz", "*.tgz"] },
                    FilePickerFileTypes.All,
                ],
            });

            if (Vm is { } vm && files.Count == 1 && files[0].TryGetLocalPath() is { } path)
                await vm.ImportFromFileAsync(path);
        }

        private async void ChooseFolder_Click(object? sender, RoutedEventArgs e)
        {
            IReadOnlyList<IStorageFolder> folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Choose the console's UI folder",
                AllowMultiple = false,
            });

            if (Vm is { } vm && folders.Count == 1 && folders[0].TryGetLocalPath() is { } path)
                await vm.ImportFromFolderAsync(path);
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Vm?.Cancel();
            Close(false);
        }
    }
}
