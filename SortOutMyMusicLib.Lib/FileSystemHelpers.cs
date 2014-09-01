using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SortOutMyMusicLib.Lib
{
    public interface IFileSystemHelpers
    {
        void RenameIfExistingFile(string filePath);
        IList<ContainerDir> GetContainerDirsIn(string musicRoot);
    }

    public class FileSystemHelpers : IFileSystemHelpers
    {
        private IList<ContainerDir> GetPathsByContainerDirFrom(IEnumerable<string> filePaths)
        {
            var dirGroups = filePaths.Select(fp => new {Dir = Path.GetDirectoryName(fp), File = fp}).GroupBy(d => d.Dir, d => d.File);
            var containerDirs = dirGroups.Select(dg => new ContainerDir {Path = dg.Key, Files = dg.ToList().Select(fp => new MediaFile { Path = fp, Name = Path.GetFileName(fp)}).ToList()});
            return containerDirs.ToList();
        }

        public void RenameIfExistingFile(string filePath)
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

        public IList<ContainerDir> GetContainerDirsIn(string musicRoot)
        {
            throw new NotImplementedException();
            //TODO:  pulled this out of MusicLibTask..
            //var allMediaFiles = _dirWalker.Walk(_appConstants.MyMusicRoot, fp => fp).Where(x => x.IsMediaFile()).ToList();
            // then return GetPathsByContainerDirFrom(allmediafiles)
        }
    }
}