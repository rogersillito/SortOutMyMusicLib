using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;
using log4net;

namespace SortOutMyMusicLib.Lib
{
    public interface IITunesLibraryHelper
    {
        void LoadITunesTrackLocations();
        bool TrackIsInLibrary(string trackPath);
    }

    public class ITunesLibraryHelper : IITunesLibraryHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ITunesLibraryHelper));
        
        private readonly IAppConstants _appConstants;
        private IList<string> _trackLocations;

        public ITunesLibraryHelper(IAppConstants appConstants)
        {
            _appConstants = appConstants;
        }

        public void LoadITunesTrackLocations()
        {
            Log.Info("Loading Track Locations in ITunes Library");


            var iTunesXml = XDocument.Load(_appConstants.ITunesLibFilePath);
            var trackLocationKeys = iTunesXml.XPathSelectElements("//plist/dict/dict/dict/key").Where(tx => tx.Value == "Location");
            _trackLocations = trackLocationKeys.Select(lk => lk.NextNode).Cast<XElement>().Select(ConvertToWindowsPath).OrderBy(t => t).ToList();
        }

        private static string ConvertToWindowsPath(XElement xe)
        {
            const string hostPrefix = "file://localhost/";
            return Path.GetFullPath(WebUtility.UrlDecode(xe.Value).Replace(hostPrefix, string.Empty));
        }

        public bool TrackIsInLibrary(string trackPath)
        {
            return _trackLocations.Contains(trackPath);
        }
    }
}