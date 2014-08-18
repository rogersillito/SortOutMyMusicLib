using System.IO;
using System.Linq;

namespace SortOutMyMusicLib.Lib
{
    public class ReleaseFromPathFactory
    {
        public static Release GetReleaseFrom(string dirPath)
        {
            var pathComponents = dirPath.Split(Path.DirectorySeparatorChar).Reverse().ToList();
            return new Release { Artist = pathComponents[1], Album = pathComponents[0] };
        }
    }
}