using System.IO;
using System.Linq;

namespace SortOutMyMusicLib.Lib
{
    public static class Extensions
    {
        private static readonly string[] ImageExtensions = { ".jpg" };
        private static readonly string[] MediaExtensions = { ".mp3", ".m4a", ".mp4", ".wma", ".mp2", ".wav" };

        public static bool IsMediaFile(this string path)
        {
            var ext = Path.GetExtension(path);
            return MediaExtensions.Contains(ext.ToLower());
        }
        public static bool IsImageFile(this string path)
        {
            var ext = Path.GetExtension(path);
            return ImageExtensions.Contains(ext.ToLower());
        }
    }
}