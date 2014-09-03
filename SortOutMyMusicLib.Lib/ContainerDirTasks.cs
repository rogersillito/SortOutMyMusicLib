using System;
using System.Collections.Generic;
using System.IO;
using log4net;

namespace SortOutMyMusicLib.Lib
{
    public interface IContainerDirTasks
    {
        void RenameSingleAcceptableFolderImageWhenWrongName(IList<string> dirCoverImages, ContainerDir dir);
    }

    public class ContainerDirTasks: IContainerDirTasks
    {
        private readonly IAppConstants _appConstants;
        private readonly IFileSystemHelpers _fileSystemHelpers;

        private static readonly ILog Log = LogManager.GetLogger(typeof (ContainerDirTasks));

        public ContainerDirTasks(IAppConstants appConstants, IFileSystemHelpers fileSystemHelpers)
        {
            _appConstants = appConstants;
            _fileSystemHelpers = fileSystemHelpers;
        }

        public void RenameSingleAcceptableFolderImageWhenWrongName(IList<string> dirCoverImages, ContainerDir dir)
        {
            if (dirCoverImages.Count != 1 || HasFolderImageFilename(dirCoverImages))
                return;
            var newPath = string.Concat(dir.Path, "\\", _appConstants.FolderImageFilename);
            _fileSystemHelpers.RenameIfThereIsAnExistingFileAt(newPath);
            _fileSystemHelpers.Rename(dirCoverImages[0], newPath);
            Log.Info(string.Concat("Renamed Folder Image: ", newPath));
            dir.FolderImagePath = newPath;
        }

        private bool HasFolderImageFilename(IList<string> dirCoverImages)
        {
            return String.Equals(Path.GetFileName(dirCoverImages[0]), _appConstants.FolderImageFilename, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}