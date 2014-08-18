using System;

namespace SortOutMyMusicLib.Lib
{
    public interface IAppConstants
    {
        string ToolsDir { get; }
        string DevToolsDir { get; }
        string MyMusicRoot { get; }
        string ITunesLibFilePath { get; }
        int MinAcceptableImageDimension { get; }
        string CoverImageFilename { get; }
    }

    public class AppConstants : IAppConstants
    {
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

        public string CoverImageFilename
        {
            get { return _configReader.GetAppSetting("CoverImageFilename"); }
        }
    }
}