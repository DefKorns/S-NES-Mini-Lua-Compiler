using Avalonia.Media.Imaging;
using ReactiveUI;

namespace SNESMiniLuaCompiler.Models
{
    public class Consoles : ReactiveObject
    {
        /// <summary>
        /// Gets the image representing the console. Can only be set during initialization.
        /// </summary>
        public Bitmap? ImagePath { get; set; }

        /// <summary>
        /// Gets the display name of the console. Can only be set during initialization.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets the region or edition of the console. Can only be set during initialization.
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Gets the system model type of the console. Can only be set during initialization.
        /// </summary>
        public SystemModel SystemModel { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }

        public override string ToString() => Name ?? base.ToString()!;
    }
}
