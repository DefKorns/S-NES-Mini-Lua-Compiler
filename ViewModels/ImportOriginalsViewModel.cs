using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SNESMiniLuaCompiler.Firmware;
using SNESMiniLuaCompiler.Models;

namespace SNESMiniLuaCompiler.ViewModels
{
    /// <summary>The dialog that imports the original Lua files of one console (from the console, a firmware file or a folder).</summary>
    public partial class ImportOriginalsViewModel(SystemModel system, string consoleName) : ViewModelBase
    {
        private CancellationTokenSource? _cancellation;

        public string ConsoleName { get; } = consoleName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanImport))]
        private bool _isBusy;

        [ObservableProperty]
        private string _host = ConsoleSsh.UsbHost;

        [ObservableProperty]
        private string _message = string.Empty;

        [ObservableProperty]
        private bool _isError;

        public bool CanImport => !IsBusy;

        /// <summary>The consoles the last import kept (a firmware file or a folder can hold several).</summary>
        internal IReadOnlyList<ImportedConsole> ImportedConsoles { get; private set; } = [];

        /// <summary>Raised when the originals were imported.</summary>
        public event EventHandler? Imported;

        [RelayCommand]
        private Task ImportFromConsole() => RunAsync(
            "Connecting to the console...",
            cancellationToken => OriginalsImporter.FromConsoleAsync(system, Host.Trim(), cancellationToken));

        public Task ImportFromFileAsync(string path) => RunAsync(
            "Reading the firmware file...",
            cancellationToken => OriginalsImporter.FromFirmwareFileAsync(system, path, cancellationToken));

        public Task ImportFromFolderAsync(string path) => RunAsync(
            "Reading the folder...",
            cancellationToken => OriginalsImporter.FromFolderAsync(system, path, cancellationToken));

        public void Cancel() => _cancellation?.Cancel();

        private async Task RunAsync(string working, Func<CancellationToken, Task<IReadOnlyList<ImportedConsole>>> import)
        {
            using var cancellation = new CancellationTokenSource();
            _cancellation = cancellation;
            IsBusy = true;
            IsError = false;
            Message = working;

            try
            {
                ImportedConsoles = await import(cancellation.Token);
                Message = ImportedConsoles.Count == 1
                    ? $"{ImportedConsoles[0].Result.Files} Lua files imported."
                    : $"Imported {ImportedConsoles.Count} consoles.";
                Imported?.Invoke(this, EventArgs.Empty);
            }
            catch (OperationCanceledException)
            {
                Message = "Cancelled.";
            }
            catch (Exception exception) when (exception is IOException or InvalidDataException or InvalidOperationException or ArgumentException or UnauthorizedAccessException)
            {
                IsError = true;
                Message = exception.Message;
            }
            finally
            {
                IsBusy = false;
                _cancellation = null;
            }
        }
    }
}
