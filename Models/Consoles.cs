using Avalonia.Media.Imaging;

namespace SNESMiniLuaCompiler.Models
{
    public class Consoles
    {
        /// <summary>
        /// Gets the image representing the console. Can only be set during initialization.
        /// </summary>
        public Bitmap? ImagePath { get; init; }

        /// <summary>
        /// Gets the display name of the console. Can only be set during initialization.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Gets the region or edition of the console. Can only be set during initialization.
        /// </summary>
        public string? Region { get; init; }

        /// <summary>
        /// Gets the system model type of the console. Can only be set during initialization.
        /// </summary>
        public SystemModel SystemModel { get; init; }

        // The default DataTemplate will show whatever ToString() will provide. So as a first idea let's change what ToString() will provide.
        public override string ToString()
        {
            return $"{Name} {ImagePath}  (Region: {Region}, System: {SystemModel})";
        }
    }
}
