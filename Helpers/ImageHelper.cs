using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SNESMiniLuaCompiler.Helpers
{
    public static class ImageHelper
    {
        public static Bitmap LoadFromResource(Uri resourceUri)
        {
            Bitmap? image = null;
            ExceptionUtils.GlobalTryCatch(() =>
            {
                image = new Bitmap(AssetLoader.Open(resourceUri));
            }, $"Failed to load asset {resourceUri}.", "ImageHelper.LoadFromResource");
            return image!;
        }
    }
}
