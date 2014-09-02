 using System.Linq.Expressions;
 using System.Runtime.Remoting.Messaging;
 using Machine.Specifications;
 using developwithpassion.specifications.moq;
 using developwithpassion.specifications.extensions;
 using Should.Core.Assertions;
 using SortOutMyMusicLib.Lib;
 using Arg = Moq.It;

namespace SortOutMyMusicLib.Tests
{   
    public class ContainerDirTasksSpecs
    {
        public abstract class context : Observes<ContainerDirTasks>
        {
        
        }

        [Subject(typeof(ContainerDirTasks))]
        public class when_RenameSingleAcceptableFolderImageWhenWrongName_is_called : context
        {

                
        }
    }
}
