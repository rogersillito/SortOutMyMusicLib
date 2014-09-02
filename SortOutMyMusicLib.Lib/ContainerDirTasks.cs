using System.Collections.Generic;

namespace SortOutMyMusicLib.Lib
{
    public interface IContainerDirTasks
    {
        void RenameSingleAcceptableFolderImageWhenWrongName(IList<string> dirCoverImages, ContainerDir dir);
    }

    public class ContainerDirTasks: IContainerDirTasks
    {
        public void RenameSingleAcceptableFolderImageWhenWrongName(IList<string> dirCoverImages, ContainerDir dir)
        {
            throw new System.NotImplementedException();
        }
    }
}