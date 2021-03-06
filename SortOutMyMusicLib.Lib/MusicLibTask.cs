﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
        private readonly IContainerDirTasks _containerDirTasks;
        private readonly IssueLog _issueLog;

        public MusicLibTask(IAppConstants appConstants, IFileSystemHelpers fileSystemHelpers, IImageHelpers imageHelpers, IProcessRunner processRunner, IDirToDoList dirToDoList, IITunesLibraryHelper iTunesLibraryHelper, ITagMetadataHelper tagMetadataHelper, IContainerDirTasks containerDirTasks, IssueLog issueLog)
        {
            _appConstants = appConstants;
            _fileSystemHelpers = fileSystemHelpers;
            _imageHelpers = imageHelpers;
            _processRunner = processRunner;
            _dirToDoList = dirToDoList;
            _iTunesLibraryHelper = iTunesLibraryHelper;
            _tagMetadataHelper = tagMetadataHelper;
            _containerDirTasks = containerDirTasks;
            _issueLog = issueLog;
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

            _containerDirTasks.RenameSingleAcceptableFolderImageWhenWrongName(dirImages, containerDir);
            _containerDirTasks.UseACoverImageAsFolderImageIfPossible(dirImages, containerDir, _issueLog);
            _containerDirTasks.CheckTracksAreInITunesLib(containerDir, _issueLog);
            _tagMetadataHelper.ValidateMetadataIn(containerDir, _issueLog);
            return;

            //TODO: move tasks to ContainerDirTasks and test..
            if (_issueLog.NeedToFindACoverImage) 
                OpenHelperAppsToFindACoverImage(containerDir.Path);
            if (_issueLog.MetadataNeedsFixing)
                OpenHelperAppsToFixMetadata(containerDir.Path);

            if (_issueLog.Exist())
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

        private void OpenHelperAppsToFixMetadata(string dirPath)
        {
            //TODO: "C:\Program Files (x86)\Mp3tag\Mp3tag.exe" dirPath
            throw new NotImplementedException();
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
    }
}