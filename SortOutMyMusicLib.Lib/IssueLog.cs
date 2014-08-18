namespace SortOutMyMusicLib.Lib
{
    public class IssueLog
    {
        public bool NeedToFindACoverImage { get; set; }
        public bool MetadataNeedsFixing { get; set; }
        public bool TracksNotInITunes { get; set; }
        public bool Exist()
        {
            return NeedToFindACoverImage || MetadataNeedsFixing || TracksNotInITunes;
        }
    }
}