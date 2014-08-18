using System;

namespace SortOutMyMusicLib.Lib
{
    public class GoogleSearchHelper
    {
        public static string GetImgSearchUrlFor(Release release)
        {
            return string.Concat("https://www.google.co.uk/search?q=%22", 
                                    Uri.EscapeUriString(release.Artist), 
                                    "%22+%22",
                                    Uri.EscapeUriString(release.Album), 
                                    "%22&safe=off&tbm=isch&espv=2&source=lnms&sa=X");
        }
    }
}
