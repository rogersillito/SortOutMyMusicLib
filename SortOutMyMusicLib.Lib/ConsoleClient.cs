using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using log4net;

namespace SortOutMyMusicLib.Lib
{
    public class ConsoleClient
    {
        private const string Separator = "\n------------------------------------\n";
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConsoleClient));
        private readonly IAppConstants _appConstants;
        private readonly IFileSystemHelpers _fileSystemHelpers;
        private readonly IImageHelpers _imageHelpers;
        private readonly IProcessRunner _processRunner;
        private readonly IDirToDoList _dirToDoList;
        private readonly IITunesLibraryHelper _iTunesLibraryHelper;
        private readonly ITagMetadataHelper _tagMetadataHelper;

        public ConsoleClient(IAppConstants appConstants, IFileSystemHelpers fileSystemHelpers, IImageHelpers imageHelpers, IProcessRunner processRunner, IDirToDoList dirToDoList, IITunesLibraryHelper iTunesLibraryHelper, ITagMetadataHelper tagMetadataHelper)
        {
            _appConstants = appConstants;
            _fileSystemHelpers = fileSystemHelpers;
            _imageHelpers = imageHelpers;
            _processRunner = processRunner;
            _dirToDoList = dirToDoList;
            _iTunesLibraryHelper = iTunesLibraryHelper;
            _tagMetadataHelper = tagMetadataHelper;
        }

        public void Execute()
        {
            _iTunesLibraryHelper.LoadITunesTrackLocations();
            Log.InfoFormat("Getting Media File Paths in \"{0}\"", _appConstants.MyMusicRoot);
            var allMediaFiles = DirWalker.Walk(_appConstants.MyMusicRoot, fp => fp).Where(x => x.IsMediaFile()).ToList();
            _dirToDoList.Add(_fileSystemHelpers.GetPathsByContainerDirFrom(allMediaFiles));
            Log.Info("Data loading complete" + Separator);
            DoNextDir();
        }

        public void DoNextDir()
        {
            var containerDir = _dirToDoList.GetNext();
            Log.InfoFormat("Checking: \"{0}\"", containerDir.Path);
            var dirImages = _imageHelpers.GetCoverImagePathsOfAcceptableSizeFrom(containerDir.Path);
            var issues = new IssueLog();

            // TASK: rename cover image file when only 1 valid cover image, and it's not called Folder.jpg
            RenameSingleAcceptableCoverImageWhenWrongName(dirImages, containerDir.Path);

            // TASK: When no cover img, either extract from ID3, or open explorer/chrome to search for/set artwork at first dir that needs a cover image
            ExtractCoverImageFromFirstFoundTagIfPossible(dirImages, containerDir, issues);

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
            DoNextDir();
        }

        private void CheckRequiredTagMetadataExists(ContainerDir containerDir, IssueLog issues)
        {
            issues.MetadataNeedsFixing = !_tagMetadataHelper.ValidateMetadataIn(containerDir);
        }

        private void CheckTracksAreInITunesLib(ContainerDir containerDir, IssueLog issues)
        {
            var notInLib = containerDir.FilePaths.Where(fp => !_iTunesLibraryHelper.TrackIsInLibrary(fp)).ToList();
            if (notInLib.Count == 0)
                return;

            Log.WarnFormat("{0}/{1} tracks not in ITunes Library", notInLib.Count, containerDir.FilePaths.Count);
            issues.TracksNotInITunes = true;
        }

        private void OpenHelperAppsToFixMetadata(string dirPath)
        {
            //TODO: "C:\Program Files (x86)\Mp3tag\Mp3tag.exe" dirPath
            throw new NotImplementedException();
        }

        private void ExtractCoverImageFromFirstFoundTagIfPossible(IList<string> dirImages, ContainerDir dir, IssueLog issues)
        {
            if (dirImages.Count != 0) return;
            if (_imageHelpers.SaveCoverImagesForFirstTagIn(dir)) return;
            Log.Warn("Cover image needed");
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

        private void RenameSingleAcceptableCoverImageWhenWrongName(IList<string> dirCoverImages, string dirPath)
        {
            if (dirCoverImages.Count != 1 || String.Equals(Path.GetFileName(dirCoverImages[0]), _appConstants.CoverImageFilename, StringComparison.CurrentCultureIgnoreCase))
                return;
            var newPath = string.Concat(dirPath, "\\", _appConstants.CoverImageFilename);
            _fileSystemHelpers.RenameIfExistingFile(newPath);
            File.Move(dirCoverImages[0], newPath);
            Log.Info(string.Concat("Renamed Cover Image: ", newPath));
        }
    }
}