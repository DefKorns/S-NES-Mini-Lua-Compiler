using Avalonia.Media.Imaging;
using ReactiveUI;

namespace SNESMiniLuaCompiler.Models
{
    public class Consoles : ReactiveObject
    {
        public Bitmap? ImagePath { get; set; }

        public string? Name { get; set; }

        public string? Region { get; set; }

        public SystemModel SystemModel { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public override string ToString() => Name ?? base.ToString()!;
    }
}
