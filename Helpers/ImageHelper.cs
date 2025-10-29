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
        public static Bitmap LoadFromResource(Uri resourceUri)
        {
            try
            {
                ExceptionUtils.LogException($"Loading asset: {resourceUri}");
                return new Bitmap(AssetLoader.Open(resourceUri));
            }
            catch (Exception ex)
            {
                ExceptionUtils.LogException($"Failed to load asset {resourceUri}: {ex.Message}");
                throw;
            }
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
