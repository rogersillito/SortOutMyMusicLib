 using System;
 using System.Collections.Generic;
 using System.Linq;
 using Machine.Fakes;
 using Machine.Specifications;
 using developwithpassion.specifications.moq;
 using developwithpassion.specifications.extensions;
 using Should;
 using SortOutMyMusicLib.Lib;
 using Arg = Moq.It;

namespace SortOutMyMusicLib.Tests
{   
    public class FileSystemHelpersSpecs
    {
        public abstract class context : Observes<FileSystemHelpers>
        {
        
        }

        [Subject(typeof(FileSystemHelpers))]
        public class when_GetContainerDirsIn_is_called : context
        {
            Establish that = () =>
            {
                Files = new List<string>
                {
                    "C:\\artist1\\album1\\testA.mp3",
                    "C:\\artist1\\album1\\testB.mp3",
                    "C:\\artist1\\album1\\Folder.jpg",
                    "C:\\artist1\\album2\\testC.mp4",
                    "C:\\artist1\\album2\\testD.mp4",
                    "C:\\artist1\\album2\\Folder.jpg",
                    "C:\\artist2\\album3\\testE.m4a",
                    "C:\\artist2\\album3\\testF.wma",
                    "C:\\artist2\\album3\\Folder.jpg",
                    "C:\\artist3\\album4\\testG.wav",
                    "C:\\artist3\\album4\\testH.m4a",
                    "C:\\artist3\\album4\\Folder.jpg",
                    "C:\\other\\nonmedia1\\budget.xls",
                    "C:\\other\\nonmedia2\\homework.doc",
                };

                DirWalker = depends.on<IDirWalker>();
                DirWalker
                    .WhenToldTo(x => x.Walk(MusicRoot, Arg.IsAny<Func<string, string>>()))
                    .Return(Files);
            };

            private Because of = () =>
                Result = concrete_sut.GetContainerDirsIn(MusicRoot);

            private It should_use_the_dir_walker_to_return_all_paths_from_the_root = () =>
                DirWalker.WasToldTo(x => x.Walk(MusicRoot, Arg.IsAny<Func<string, string>>()));

            private It should_return_1_container_dir_per_directory_containing_media_files = () =>
                Result.Count.ShouldEqual(4);

            private It should_not_return_a_container_dir_when_a_directory_contains_no_media_files = () =>
                Result.Any(x => x.Path.StartsWith("C:\\other")).ShouldBeFalse();

            private It should_return_the_expected_number_of_media_files = () =>
                Result.SelectMany(x => x.Files).Count().ShouldEqual(8);

            private It should_exclude_non_media_files = () =>
                Result.SelectMany(x => x.Files).Select(x => x.Name).Any(x => x.EndsWith(".jpg")).ShouldBeFalse();

            private static List<string> Files;
            private static IList<ContainerDir> Result;
            public static IDirWalker DirWalker;
            public static string MusicRoot = "C:\\";
        }
    }
}
