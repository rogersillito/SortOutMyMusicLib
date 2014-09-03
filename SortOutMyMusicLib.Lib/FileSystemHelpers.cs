using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SystemWrapper.IO;

namespace SortOutMyMusicLib.Lib
{
    public interface IFileSystemHelpers
    {
        void RenameIfThereIsAnExistingFileAt(string filePath);
        IList<ContainerDir> GetContainerDirsIn(string musicRoot);
        void Rename(string oldPath, string newPath);
    }

    public class FileSystemHelpers : IFileSystemHelpers
    {
        private readonly IDirWalker _dirWalker;
        private readonly IFileWrap _fileWrap;

        public FileSystemHelpers(IDirWalker dirWalker, IFileWrap fileWrap)
        {
            _dirWalker = dirWalker;
            _fileWrap = fileWrap;
        }

        public IList<ContainerDir> GetContainerDirsIn(string musicRoot)
        {
            var allMediaFiles = _dirWalker.Walk(musicRoot, fp => fp).Where(x => x.IsMediaFile()).ToList();
            return GetPathsByContainerDirFrom(allMediaFiles);
        }

        public void Rename(string oldPath, string newPath)
        {
            _fileWrap.Move(oldPath, newPath);
        }

        private IList<ContainerDir> GetPathsByContainerDirFrom(IEnumerable<string> filePaths)
        {
            var dirGroups = filePaths
                .Select(fp => new {Dir = Path.GetDirectoryName(fp), File = fp})
                .GroupBy(d => d.Dir, d => d.File);
            var containerDirs = dirGroups
                .Select(dg => new ContainerDir
                {
                    Path = dg.Key, 
                    Files = dg.ToList().Select(fp => new MediaFile
                    {
                        Path = fp, 
                        Name = Path.GetFileName(fp)
                    }).ToList()
                });
            return containerDirs.ToList();
        }

        public void RenameIfThereIsAnExistingFileAt(string filePath)
        {
            if (!File.Exists(filePath))
                return;
            var counter = 1;
            var dir = Path.GetDirectoryName(filePath);
            var ext = Path.GetExtension(filePath);
            var fname = Path.GetFileNameWithoutExtension(filePath);
            Func<int, string> getNewName = c =>
                string.Concat(dir, "\\", fname, " (", c.ToString(CultureInfo.InvariantCulture), ")", ext);
            var newPath = getNewName(counter);
            while (File.Exists(newPath))
            {
                newPath = getNewName(counter++);
            }
            File.Move(filePath, newPath);
        }
    }
}