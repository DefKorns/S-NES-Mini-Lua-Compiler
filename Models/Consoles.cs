using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SNESMiniLuaCompiler.Models
{
    public partial class Consoles : ObservableObject
    {
        public Bitmap? ImagePath { get; set; }
        public string? Name { get; set; }
        public string? Region { get; set; }
        public SystemModel SystemModel { get; set; }

        [ObservableProperty]
        private bool _isChecked;

        [ObservableProperty]
        private bool _isEnabled;

        public override string ToString() => Name ?? base.ToString()!;
    }
}