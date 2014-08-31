 using System.Data.Odbc;
 using Machine.Fakes;
 using Machine.Specifications;
 using developwithpassion.specifications.moq;
 using developwithpassion.specifications.extensions;
 using SortOutMyMusicLib.Lib;
 using Arg = Moq.It;

namespace SortOutMyMusicLib.Tests
{   
    public class MusicLibTaskSpecs
    {
        public abstract class concern : Observes<MusicLibTask>
        {
            Establish given = () =>
            {
                ITunesHelper = depends.on<IITunesLibraryHelper>();
            };

            protected static IITunesLibraryHelper ITunesHelper;
        }

        [Subject(typeof(MusicLibTask))]
        public class when_InitialiseAndStartDirScan_is_called : concern
        {
            private Because when = () =>
                concrete_sut.InitialiseAndStartDirScan();

            private It should_load_paths_of_all_tracks_in_ituens = () =>
                ITunesHelper.WasToldTo(x => x.LoadITunesTrackLocations());
        }
    }
}
