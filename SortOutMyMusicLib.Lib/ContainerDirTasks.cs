using System;
using System.Collections.Generic;
using System.IO;
using log4net;

namespace SortOutMyMusicLib.Lib
{
    public interface IContainerDirTasks
    {
        void RenameSingleAcceptableFolderImageWhenWrongName(IList<string> dirCoverImages, ContainerDir containerDir);
        void UseACoverImageAsFolderImageIfPossible(IList<string> dirCoverImages, ContainerDir containerDir, IssueLog issues);
    }

    public class ContainerDirTasks: IContainerDirTasks
    {
        private readonly IAppConstants _appConstants;
        private readonly IFileSystemHelpers _fileSystemHelpers;
        private readonly IImageHelpers _imageHelpers;
        private static readonly ILog Log = LogManager.GetLogger(typeof (ContainerDirTasks));

        public ContainerDirTasks(IAppConstants appConstants, IFileSystemHelpers fileSystemHelpers, IImageHelpers imageHelpers)
        {
            _appConstants = appConstants;
            _fileSystemHelpers = fileSystemHelpers;
            _imageHelpers = imageHelpers;
        }

        public void RenameSingleAcceptableFolderImageWhenWrongName(IList<string> dirCoverImages, ContainerDir containerDir)
        {
            if (dirCoverImages.Count != 1 || HasFolderImageFilename(dirCoverImages))
                return;
            var newPath = string.Concat(containerDir.Path, "\\", _appConstants.FolderImageFilename);
            _fileSystemHelpers.RenameIfThereIsAnExistingFileAt(newPath);
            _fileSystemHelpers.Rename(dirCoverImages[0], newPath);
            Log.Info(string.Concat("Renamed Folder Image: ", newPath));
            containerDir.FolderImagePath = newPath;
        }

        public void UseACoverImageAsFolderImageIfPossible(IList<string> dirCoverImages, ContainerDir containerDir, IssueLog issues)
        {
            if (dirCoverImages.Count != 0) return;
            _imageHelpers.TrySaveFolderImageFromAMediaFileIn(containerDir);
            if (containerDir.HasFolderImage) return;
            Log.Warn("Folder image needed");
            issues.NeedToFindACoverImage = true;
        }

        private bool HasFolderImageFilename(IList<string> dirCoverImages)
        {
            return String.Equals(Path.GetFileName(dirCoverImages[0]), _appConstants.FolderImageFilename, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}