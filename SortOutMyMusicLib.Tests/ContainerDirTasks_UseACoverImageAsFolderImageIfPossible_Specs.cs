 using System.Collections.Generic;
 using Machine.Fakes;
 using Machine.Specifications;
 using developwithpassion.specifications.moq;
 using developwithpassion.specifications.extensions;
 using Should;
 using SortOutMyMusicLib.Lib;
 using Arg = Moq.It;

namespace SortOutMyMusicLib.Tests
{   
    public class ContainerDirTasks_UseACoverImageAsFolderImageIfPossible_Specs
    {
        public abstract class context : Observes<ContainerDirTasks>
        {
            Establish that = () =>
            {
                DirCoverImages = new List<string>();
                ContainerDir = new ContainerDir();
                IssueLog = new IssueLog();
                ImageHelpers = depends.on<IImageHelpers>();
                ImageHelpers.WhenToldTo(x => x.TrySaveFolderImageFromAMediaFileIn(ContainerDir));
            };

            private Because of = () =>
                concrete_sut.UseACoverImageAsFolderImageIfPossible(DirCoverImages, ContainerDir, IssueLog);

            public static List<string> DirCoverImages;
            public static ContainerDir ContainerDir;
            public static IssueLog IssueLog;
            public static IImageHelpers ImageHelpers;
        }

        [Subject(typeof(ContainerDirTasks))]
        public class when_there_are_already_dir_cover_images: context
        {
            Establish that = () => DirCoverImages.Add("C:\\Folder.jpg");

            private It should_not_try_to_save_a_folder_image_from_a_media_file = () =>
                ImageHelpers.WasNotToldTo(x => x.TrySaveFolderImageFromAMediaFileIn(ContainerDir));

            private It should_not_record_an_issue_flag = () =>
                IssueLog.NeedToFindACoverImage.ShouldBeFalse();
        }

        [Subject(typeof(ContainerDirTasks))]
        public class when_there_are_no_existing_dir_cover_images_and_a_folder_image_can_be_extracted_from_a_media_file: context
        {
            private Establish that = () =>
                ContainerDir.FolderImagePath = "C:\\Folder.jpg";

            private It should_try_to_save_a_folder_image_from_a_media_file = () =>
                ImageHelpers.WasToldTo(x => x.TrySaveFolderImageFromAMediaFileIn(ContainerDir));

            private It should_not_record_an_issue_flag = () =>
                IssueLog.NeedToFindACoverImage.ShouldBeFalse();
        }

        [Subject(typeof(ContainerDirTasks))]
        public class when_there_are_no_existing_dir_cover_images_and_a_folder_image_cannot_be_extracted_from_a_media_file: context
        {
            private It should_try_to_save_a_folder_image_from_a_media_file = () =>
                ImageHelpers.WasToldTo(x => x.TrySaveFolderImageFromAMediaFileIn(ContainerDir));

            private It should_record_an_issue_flag = () =>
                IssueLog.NeedToFindACoverImage.ShouldBeTrue();
        }
    }
}
