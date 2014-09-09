 using System;
 using System.Linq;
 using System.Linq.Expressions;
 using Machine.Fakes;
 using Machine.Specifications;
 using developwithpassion.specifications.moq;
 using developwithpassion.specifications.extensions;
 using Should;
 using SortOutMyMusicLib.Lib;
 using Arg = Moq.It;

namespace SortOutMyMusicLib.Tests
{   
    public class ContainerDirTasks_CheckTracksAreInITunesLib_Specs
    {
        public abstract class context : Observes<ContainerDirTasks>
        {
            Establish that = () =>
            {
                ContainerDir = new ContainerDir();
                IssueLog = new IssueLog();
                ITunesLibraryHelper = depends.on<IITunesLibraryHelper>();
                FilePaths = new[]
                {
                    "C:\\File1.mp3", 
                    "C:\\File2.mp3", 
                    "C:\\File3.mp3"
                };
                ContainerDir.Files = FilePaths.Select(x => new MediaFile {Path = x}).ToList();
            };

            private Because of = () =>
                concrete_sut.CheckTracksAreInITunesLib(ContainerDir, IssueLog);

            public static ContainerDir ContainerDir;
            public static IssueLog IssueLog;
            public static IITunesLibraryHelper ITunesLibraryHelper;
            public static string[] FilePaths;
        }

        [Subject(typeof(ContainerDirTasks))]
        public class when_one_or_more_tracks_in_the_container_dir_are_not_in_the_itunes_lib : context
        {
            Establish that = () => ITunesLibraryHelper
                .WhenToldTo(x => x.TrackIsInLibrary(Arg.Is<string>(t => FilePaths.Contains(t))))
                .Return(false);

            private It should_log_the_issue = () =>
                IssueLog.TracksNotInITunes.ShouldBeTrue();

            private It should_check_each_file_in_the_container_dir_is_in_itunes = () =>
                ITunesLibraryHelper.WasToldTo(x => x.TrackIsInLibrary(Arg.Is<string>(t => FilePaths.Contains(t)))).Times(3);
        }

        [Subject(typeof(ContainerDirTasks))]
        public class when_all_tracks_in_the_container_dir_are_found_in_the_itunes_lib : context
        {
            Establish that = () => ITunesLibraryHelper
                .WhenToldTo(x => x.TrackIsInLibrary(Arg.Is<string>(t => FilePaths.Contains(t))))
                .Return(true);

            private It should_not_log_the_issue = () =>
                IssueLog.TracksNotInITunes.ShouldBeFalse();

            private It should_check_each_file_in_the_container_dir_is_in_itunes = () =>
                ITunesLibraryHelper.WasToldTo(x => x.TrackIsInLibrary(Arg.Is<string>(t => FilePaths.Contains(t)))).Times(3);
        }
    }
}
