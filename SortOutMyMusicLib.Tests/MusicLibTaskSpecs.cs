﻿ using System;
 using System.Collections.Generic;
 using System.Data.Odbc;
 using System.Diagnostics;
 using Machine.Fakes;
 using Machine.Specifications;
 using developwithpassion.specifications.moq;
 using developwithpassion.specifications.extensions;
 using Moq;
 using SortOutMyMusicLib.Lib;
 using Arg = Moq.It;
 using It = Machine.Specifications.It;

namespace SortOutMyMusicLib.Tests
{   
    public class MusicLibTaskSpecs
    {
        public abstract class context : Observes<MusicLibTask>
        {
            public const string MusicRoot = "C:\\Music";

            Establish that = () =>
            {
                ITunesHelper = depends.on<IITunesLibraryHelper>();
                DirToDoList = depends.on<IDirToDoList>();

                ContainerDirsInMusicRoot.Add(new ContainerDir
                {
                    Path = "C:\\Dir1",
                    Files = new []
                    {
                        new MediaFile { Name = "Track1.m4a", Path = "C:\\Dir1\\Track1.m4a" },
                        new MediaFile { Name = "Track2.m4a", Path = "C:\\Dir1\\Track2.m4a" },
                        new MediaFile { Name = "Track3.m4a", Path = "C:\\Dir1\\Track3.m4a" }
                    }
                });

                FileSystemHelpers = depends.on<IFileSystemHelpers>();
                FileSystemHelpers
                    .WhenToldTo(x => x.GetContainerDirsIn(MusicRoot))
                    .Return(ContainerDirsInMusicRoot);

                AppConstants = new Mock<IAppConstants>();
                AppConstants
                    .SetupGet(x => x.MyMusicRoot)
                    .Returns(MusicRoot);

                depends.on(AppConstants.Object);
            };

            public static IDirToDoList DirToDoList { get; set; }
            public static Mock<IAppConstants> AppConstants { get; set; }
            public static IITunesLibraryHelper ITunesHelper;
            public static IFileSystemHelpers FileSystemHelpers;
            public static IList<ContainerDir> ContainerDirsInMusicRoot = new List<ContainerDir>();
        }

        [Subject(typeof(MusicLibTask))]
        public class when_InitialiseAndStartDirScan_is_called_and_there_are_dirs_to_scan : context
        {
            Establish that = () => 
                DirToDoList.WhenToldTo(x => x.GetNext()).Return(ContainerDirsInMusicRoot[0]);

            private Because when = () =>
                concrete_sut.InitialiseAndStartDirScan();

            private It should_load_paths_of_all_tracks_in_itunes = () =>
                ITunesHelper.WasToldTo(x => x.LoadITunesTrackLocations());

            private It should_get_the_music_lib_root = () =>
                AppConstants.VerifyGet(x => x.MyMusicRoot, Times.Exactly(2));

            private It should_get_container_dirs_to_process = () =>
                FileSystemHelpers.WasToldTo(x => x.GetContainerDirsIn(MusicRoot));

            private It container_dirs_should_be_added_to_the_todo_list = () =>
                DirToDoList.WasToldTo(x => x.Add(ContainerDirsInMusicRoot));

            private It should_get_the_next_dir_to_scan = () =>
                DirToDoList.WasToldTo(x => x.GetNext()).OnlyOnce();
        }
    }
}
