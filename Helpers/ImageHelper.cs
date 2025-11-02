using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SNESMiniLuaCompiler.Helpers
{
    public static class ImageHelper
    {
        public static Bitmap? LoadImage(string path)
        {
            Bitmap? image = null;
            ExceptionUtils.GlobalTryCatch(() =>
            {
                image = new Bitmap(path);
            }, "Failed to load image.", "ImageHelper.LoadImage");
            return image;
        }

        public static Bitmap LoadFromResource(Uri resourceUri)
        {
            Bitmap? image = null;
            ExceptionUtils.GlobalTryCatch(() =>
            {
                image = new Bitmap(AssetLoader.Open(resourceUri));
            }, $"Failed to load asset {resourceUri}.", "ImageHelper.LoadFromResource");
            return image!;
        }

        public static async Task<Bitmap?> LoadFromWeb(Uri url)
        {
            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsByteArrayAsync();
                return new Bitmap(new MemoryStream(data));
            }
            catch (HttpRequestException ex)
            {
                ExceptionUtils.LogException($"Failed to load asset {url}: {ex.Message}");
                return null;
            }
        }
    }
}
