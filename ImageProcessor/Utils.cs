using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;



namespace ImageProcessor
{
    public static class Utils
    {
        public static async Task<SKBitmap> GetBitmapFromUrl(string url)
        {
            using HttpClient httpClient = new HttpClient();
            byte[] bytes = await httpClient.GetByteArrayAsync(url);
            MemoryStream memoryStream = new MemoryStream(bytes);
            SKBitmap bitmap = SKBitmap.Decode(memoryStream);
            return bitmap;
        }

        public static SKBitmap GetBitmapFromFile(string fileName)
        {
            FileStream stream = new FileStream($"Common/Assets/Textures/{ fileName }", FileMode.Open);
            return SKBitmap.Decode(stream);
        }
    }
}
