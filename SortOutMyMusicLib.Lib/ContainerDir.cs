using System.Collections.Generic;

namespace SortOutMyMusicLib.Lib
{
    public class ContainerDir
    {
        public string Path { get; set; }
        public IList<string> FilePaths { get; set; }
    }
}