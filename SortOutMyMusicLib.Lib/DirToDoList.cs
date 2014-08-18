using System.Collections.Generic;
using log4net;

namespace SortOutMyMusicLib.Lib
{
    public interface IDirToDoList
    {
        void Add(IList<ContainerDir> containerDirs);
        ContainerDir GetNext();
    }

    public class DirToDoList : IDirToDoList
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (DirToDoList));
        private IList<ContainerDir> _list;

        public void Add(IList<ContainerDir> containerDirs)
        {
            Log.Info("Adding all Media container dirs to To-Do list");
            _list = containerDirs;
        }

        public ContainerDir GetNext()
        {
            if (_list.Count == 0)
                return null;
            var dir = _list[0];
            _list.RemoveAt(0);
            return dir;
        }
    }
}