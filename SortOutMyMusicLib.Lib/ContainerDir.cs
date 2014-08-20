using System.Collections.Generic;

namespace SortOutMyMusicLib.Lib
{
    public class ContainerDir
    {
        private string _folderImagePath;
        public string Path { get; set; }
        public IList<MediaFile> Files { get; set; }
        public bool HasFolderImage { get; private set; }
        public string FolderImagePath
        {
            get { return _folderImagePath; }
            set
            {
                HasFolderImage = true;
                _folderImagePath = value;
            }
        }

        public override string ToString()
        {
            return Path;
        }
    }
}