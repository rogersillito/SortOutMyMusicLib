using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using log4net;

namespace SortOutMyMusicLib.Lib
{
    public class MusicLibTask
    {
        private const string Separator = "\n------------------------------------\n";
        private static readonly ILog Log = LogManager.GetLogger(typeof(MusicLibTask));
        private readonly IAppConstants _appConstants;
        private readonly IFileSystemHelpers _fileSystemHelpers;
        private readonly IImageHelpers _imageHelpers;
        private readonly IProcessRunner _processRunner;
        private readonly IDirToDoList _dirToDoList;
        private readonly IITunesLibraryHelper _iTunesLibraryHelper;
        private readonly ITagMetadataHelper _tagMetadataHelper;
        private readonly IDirWalker _dirWalker;

        public MusicLibTask(IAppConstants appConstants, IFileSystemHelpers fileSystemHelpers, IImageHelpers imageHelpers, IProcessRunner processRunner, IDirToDoList dirToDoList, IITunesLibraryHelper iTunesLibraryHelper, ITagMetadataHelper tagMetadataHelper, IDirWalker dirWalker)
        {
            _appConstants = appConstants;
            _fileSystemHelpers = fileSystemHelpers;
            _imageHelpers = imageHelpers;
            _processRunner = processRunner;
            _dirToDoList = dirToDoList;
            _iTunesLibraryHelper = iTunesLibraryHelper;
            _tagMetadataHelper = tagMetadataHelper;
            _dirWalker = dirWalker;
        }

        public void InitialiseAndStartDirScan()
        {
            _iTunesLibraryHelper.LoadITunesTrackLocations();
            Log.InfoFormat("Getting Media File Paths in \"{0}\"", _appConstants.MyMusicRoot);
            _dirToDoList.Add(_fileSystemHelpers.GetContainerDirsIn(_appConstants.MyMusicRoot));
            Log.Info("Data loading complete" + Separator);
            RestartDirScan();
        }

        public void RestartDirScan()
        {
            var containerDir = _dirToDoList.GetNext();
            Log.InfoFormat("Checking: \"{0}\"", containerDir);
            var dirImages = _imageHelpers.GetFolderImagePathsOfAcceptableSizeFrom(containerDir.Path);
            var issues = new IssueLog();

            // TASK: rename folder image file when only 1 valid cover image, and it's not called Folder.jpg
            RenameSingleAcceptableFolderImageWhenWrongName(dirImages, containerDir);

            // TASK: When no folder img, either extract from ID3, or open explorer/chrome to search for/set artwork at first dir that needs a cover image
            UseACoverImageAsFolderImageIfPossible(dirImages, containerDir, issues);

            // TASK: check if all tracks in this dir are in iTunes lib
            CheckTracksAreInITunesLib(containerDir, issues);

            // TASK: check required metadata is present
            CheckRequiredTagMetadataExists(containerDir, issues);

            if (issues.NeedToFindACoverImage) 
                OpenHelperAppsToFindACoverImage(containerDir.Path);
            if (issues.MetadataNeedsFixing)
                OpenHelperAppsToFixMetadata(containerDir.Path);

            if (issues.Exist())
                return;

            Log.Info("Dir is OK!" + Separator);
            RestartDirScan();
        }

        private void SaveCoverImageToTagsIfPossible(IList<string> dirImages, ContainerDir containerDir)
        {
            //TODO: surely i EITHER get an image from a file, or save an image into tags
            //TODO: no, just do this after file validation (TagMetadataHelper)
            //foreach (var file in containerDir.Files)
            //{
            //    if (!file.Images.Any(im => im.IsAcceptableSize) && dirImages)

                
            //}

            //throw new NotImplementedException();
        }

        private void CheckRequiredTagMetadataExists(ContainerDir containerDir, IssueLog issues)
        {
            issues.MetadataNeedsFixing = !_tagMetadataHelper.ValidateMetadataIn(containerDir);
        }

        private void CheckTracksAreInITunesLib(ContainerDir containerDir, IssueLog issues)
        {
            var notInLib = containerDir.Files.Select(f => f.Path).Where(fp => !_iTunesLibraryHelper.TrackIsInLibrary(fp)).ToList();
            if (notInLib.Count == 0)
                return;

            Log.WarnFormat("{0}/{1} tracks not in ITunes Library", notInLib.Count, containerDir.Files.Count);
            issues.TracksNotInITunes = true;
        }

        private void OpenHelperAppsToFixMetadata(string dirPath)
        {
            //TODO: "C:\Program Files (x86)\Mp3tag\Mp3tag.exe" dirPath
            throw new NotImplementedException();
        }

        private void UseACoverImageAsFolderImageIfPossible(IList<string> dirImages, ContainerDir dir, IssueLog issues)
        {
            if (dirImages.Count != 0) return;
            _imageHelpers.TrySaveFolderImageFromAMediaFileIn(dir);
            if (dir.HasFolderImage) return;
            Log.Warn("Folder image needed");
            issues.NeedToFindACoverImage = true;
        }

        private void OpenHelperAppsToFindACoverImage(string dirPath)
        {
            var ahkScriptName = _appConstants.DevToolsDir + @"\AhkScripts\OpenDirAndChromeForReleaseArtwork.ahk";
            var ahkPath = _appConstants.ToolsDir + @"\AutoHotKey\AutoHotKey.exe";
            var googleUrl = GoogleSearchHelper.GetImgSearchUrlFor(ReleaseFromPathFactory.GetReleaseFrom(dirPath));
            var cmdArgs = string.Concat(ahkScriptName, " \"", googleUrl, "\" \"", dirPath, "\"");
            Log.Info(cmdArgs);
            _processRunner.Exec(ahkPath, cmdArgs, _appConstants.DevToolsDir + @"\AhkScripts");
        }

        private void RenameSingleAcceptableFolderImageWhenWrongName(IList<string> dirCoverImages, ContainerDir dir)
        {
            if (dirCoverImages.Count != 1 || HasFolderImageFilename(dirCoverImages))
                return;
            var newPath = string.Concat(dir.Path, "\\", _appConstants.FolderImageFilename);
            _fileSystemHelpers.RenameIfExistingFile(newPath);
            File.Move(dirCoverImages[0], newPath);
            Log.Info(string.Concat("Renamed Folder Image: ", newPath));
            dir.FolderImagePath = newPath;
        }

        private bool HasFolderImageFilename(IList<string> dirCoverImages)
        {
            return String.Equals(Path.GetFileName(dirCoverImages[0]), _appConstants.FolderImageFilename, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}