using System.Drawing;
using TagLib;

namespace SortOutMyMusicLib.Lib
{
    public class CoverImage
    {
        public Image ImageData { get; set; }
        public PictureType Type { get; set; }
        public uint CheckSum { get; set; }
        public bool IsAcceptableSize { get; set; }
        public string FileExtension { get; set; }
    }
}