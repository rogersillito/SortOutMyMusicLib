using System;
using System.IO;

namespace SortOutMyMusicLib.Lib
{
    public interface IAppConstants
    {
        string ToolsDir { get; }
        string DevToolsDir { get; }
        string MyMusicRoot { get; }
        string ITunesLibFilePath { get; }
        int MinAcceptableImageDimension { get; }
        string FolderImageFilename { get; }
    }

    public class AppConstants : IAppConstants
    {
        //TODO: make it only get values from source once.. currently happens on each property access
        private readonly IConfigReader _configReader;

        public AppConstants(IConfigReader configReader)
        {
            _configReader = configReader;
        }

        public string ToolsDir
        {
            get { return Environment.GetEnvironmentVariable("_ToolsDir"); }
        }
         
        public string DevToolsDir
        {
            get { return Environment.GetEnvironmentVariable("_DevTools"); }
        }

        public string MyMusicRoot
        {
            get { return _configReader.GetAppSetting("MyMusicRoot"); }
        }

        public string ITunesLibFilePath
        {
            get { return _configReader.GetAppSetting("ITunesLibFilePath"); }
        }

        public int MinAcceptableImageDimension
        {
            get { return Int32.Parse(_configReader.GetAppSetting("MinAcceptableImageDimension")); }
        }

        public string FolderImageFilename
        {
            get { return _configReader.GetAppSetting("FolderImageFilename"); }
        }

    }
}