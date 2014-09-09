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
        private readonly IImageHelpers _imageHelpers;

        public DirToDoList(IImageHelpers imageHelpers)
        {
            _imageHelpers = imageHelpers;
        }

        public void Add(IList<ContainerDir> containerDirs)
        {
            Log.Info("Adding all Media container dirs to To-Do list");
            _list = containerDirs;
        }

        public ContainerDir GetNext()
        {
            //TODO: establish my 'workflow' before going any further - get next, whitelist, re-check, run external processes?
            if (_list.Count == 0)
                return null;
            var dir = _list[0];
            //TODO: but what if we can't process a file, or want to add it to a white-list
            //TODO: we need to be able to hold  a path in a pending state before doing the remove
            _list.RemoveAt(0);
            _imageHelpers.SetImagesFor(dir);
            return dir;
        }
    }
}