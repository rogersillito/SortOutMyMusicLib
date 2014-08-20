using System.Collections.Generic;

namespace SortOutMyMusicLib.Lib
{
    public class MediaFile
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public IEnumerable<CoverImage> Images { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}