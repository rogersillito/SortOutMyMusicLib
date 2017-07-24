using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using Machine.Fakes;
using Machine.Specifications;
using developwithpassion.specifications.moq;
using developwithpassion.specifications.extensions;
using Moq;
using Should;
using Should.Core.Assertions;
using SortOutMyMusicLib.Lib;
using Arg = Moq.It;
using It = Machine.Specifications.It;

namespace SortOutMyMusicLib.Tests
{
    public class ContainerDirTasks_RenameSingleAcceptableFolderImageWhenWrongName_Specs
    {
        public abstract class context : Observes<ContainerDirTasks>
        {
            Establish that = () =>
            {
                DirCoverImages = new List<string>();
                ContainerDir = new ContainerDir();

                AppConstants = new Mock<IAppConstants>();
                AppConstants
                    .SetupGet(x => x.FolderImageFilename)
                    .Returns("Folder.jpg");
                depends.on(AppConstants.Object);

                FileSystemHelpers = depends.on<IFileSystemHelpers>();
            };

            private Because of = () =>
                concrete_sut.RenameSingleAcceptableFolderImageWhenWrongName(DirCoverImages, ContainerDir);

            protected static List<string> DirCoverImages;
            protected static ContainerDir ContainerDir;
            protected static Mock<IAppConstants> AppConstants;
            protected static IFileSystemHelpers FileSystemHelpers;
        }

        [Behaviors]
        public class ACallThatDoesNotRenameAFolderImage
        {
            private It should_not_set_a_folder_image_path_on_the_ContainerDir = () =>
                ContainerDir.FolderImagePath.ShouldBeNull();

            private It should_not_rename_existing_files_at_new_path = () =>
                FileSystemHelpers.WasNotToldTo(x => x.RenameIfThereIsAnExistingFileAt(Arg.IsAny<string>()));

            private It should_not_rename_a_folder_image = () =>
                FileSystemHelpers.WasNotToldTo(x => x.Rename(Arg.IsAny<string>(), Arg.IsAny<string>(), false));

            protected static List<string> DirCoverImages;
            protected static ContainerDir ContainerDir;
            protected static IFileSystemHelpers FileSystemHelpers;
        }

        [Subject(typeof(ContainerDirTasks))]
        public class when_there_are_no_cover_images : context
        {
            protected Behaves_like<ACallThatDoesNotRenameAFolderImage> A_call_that_does_not_rename_a_folder_image;

            private It should_not_get_the_expected_folder_image_filename = () =>
                AppConstants.VerifyGet(x => x.FolderImageFilename, Times.Never());
        }

        [Subject(typeof(ContainerDirTasks))]
        public class when_there_are_multiple_cover_images : context
        {
            private Establish that = () =>
            {
                DirCoverImages.Add("C:\\Folder.jpg");
                DirCoverImages.Add("C:\\Cover.jpg");
            };

            protected Behaves_like<ACallThatDoesNotRenameAFolderImage> A_call_that_does_not_rename_a_folder_image;

            private It should_not_get_the_expected_folder_image_filename = () =>
                AppConstants.VerifyGet(x => x.FolderImageFilename, Times.Never());
        }

        [Subject(typeof(ContainerDirTasks))]
        public class when_there_is_a_single_folder_image_that_already_has_the_correct_name : context
        {
            private Establish that = () =>
                DirCoverImages.Add("C:\\Folder.jpg");

            protected Behaves_like<ACallThatDoesNotRenameAFolderImage> A_call_that_does_not_rename_a_folder_image;

            private It should_get_the_expected_folder_image_filename_once_only = () =>
                    AppConstants.VerifyGet(x => x.FolderImageFilename, Times.Once());
        }

        [Subject(typeof(ContainerDirTasks))]
        public class when_there_is_a_single_folder_image_with_the_wrong_name : context
        {
            private const string ExpectedFolderImagePath = "C:\\DirRoot\\Folder.jpg";

            private Establish that = () =>
            {
                ContainerDir.Path = "C:\\DirRoot";
                DirCoverImages.Add("C:\\DirRoot\\Rupert.jpg");
            };

            private It should_get_the_expected_folder_image_filename_twice = () =>
                    AppConstants.VerifyGet(x => x.FolderImageFilename, Times.Exactly(2));

            private It should_ensure_an_existing_files_at_new_path_gets_renamed = () =>
                FileSystemHelpers.WasToldTo(x => x.RenameIfThereIsAnExistingFileAt(ExpectedFolderImagePath));

            private It should_rename_a_folder_image = () =>
                FileSystemHelpers.WasToldTo(x => x.Rename(DirCoverImages[0], ExpectedFolderImagePath, false));

            private It should_set_a_folder_image_path_on_the_ContainerDir = () =>
                ContainerDir.FolderImagePath.ShouldEqual(ExpectedFolderImagePath);
        }
    }
}
