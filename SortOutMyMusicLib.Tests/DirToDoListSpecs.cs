 using System.Collections.Generic;
 using System.Linq;
 using log4net;
 using Machine.Fakes;
 using Machine.Specifications;
 using developwithpassion.specifications.moq;
 using developwithpassion.specifications.extensions;
 using Should;
 using SortOutMyMusicLib.Lib;
 using Arg = Moq.It;

namespace SortOutMyMusicLib.Tests
{   
    public class DirToDoListSpecs
    {
        public abstract class context : Observes<DirToDoList>
        {
            Establish that = () =>
            {
                ImageHelpers = depends.on<IImageHelpers>();
                ContainerDirs = new List<ContainerDir>
                {
                    new ContainerDir(),
                    new ContainerDir(),
                    new ContainerDir()
                };
            };

            private Because of = () =>
                concrete_sut.Add(ContainerDirs.ConvertAll(cd => cd));
            
            public static ContainerDir GetNextResult;        
            public static List<ContainerDir> ContainerDirs;
            public static IImageHelpers ImageHelpers;
        }

        [Subject(typeof(DirToDoList))]
        public class when_GetNext_is_called_but_there_are_no_ContainerDirs_to_process: context
        {
            private Because of = () =>
            {
                concrete_sut.Add(new List<ContainerDir>());
                GetNextResult = concrete_sut.GetNext();
            };

            private It should_return_null = () =>
                GetNextResult.ShouldBeNull();

            private It should_not_set_images = () =>
                ImageHelpers.WasNotToldTo(x => x.SetImagesFor(Arg.IsAny<ContainerDir>()));
        }

        [Subject(typeof(DirToDoList))]
        public class when_GetNext_is_first_called : context
        {
            private Because of = () => { GetNextResult = concrete_sut.GetNext(); };

            private It should_return_the_first_ContainerDir = () =>
                GetNextResult.ShouldEqual(ContainerDirs[0]);

            private It should_set_images_on_the_dir_before_returning_it = () =>
                ImageHelpers.WasToldTo(x => x.SetImagesFor(ContainerDirs[0]));
        }
    }
}
