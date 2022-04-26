using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using TagLib;
using TagLibFile = TagLib.File;

namespace SortOutMyMusicLib.Lib
{
    public class RebuildTask
    {
        private readonly IDirWalker _dirWalker;
        private readonly IFileSystemHelpers _fileSystemHelpers;
        private string _sourceDir;
        private string _outDir;
        private static readonly ILog Log = LogManager.GetLogger(typeof(RebuildTask));

        public RebuildTask(IDirWalker dirWalker, IFileSystemHelpers fileSystemHelpers)
        {
            _dirWalker = dirWalker;
            _fileSystemHelpers = fileSystemHelpers;
        }

        public void RebuildFolderStructure(string sourceDir, bool rebuildOverwrite, string outDir = null)
        {
            _sourceDir = _fileSystemHelpers.StripPathTrailingSlashes(sourceDir);
            _outDir = outDir == null ? _sourceDir : _fileSystemHelpers.StripPathTrailingSlashes(outDir);
            //Console.WriteLine(_outDir);
            //Console.WriteLine(_sourceDir);

            var result = ScanFiles();
            foreach (var f in result.Where(r => r.IsMediaFile))
            {
                if (!f.IsValidToMove)
                {
                    Log.WarnFormat("File not valid to move: '{0}' (artist='{1}', album='{2}')", f.OldPath, f.Artist, f.Album);
                    continue;
                }
                if (f.RequiresMove)
                {
                    Console.WriteLine("MOVING: " + f.OldPath + " > " + f.NewPath);
                    _fileSystemHelpers.Rename(f.OldPath, f.NewPath, rebuildOverwrite);
                }
            }
            //TODO: handle orphaned cover images (not IsMediaFile)
        }

        private IEnumerable<RebuildWalkResult> ScanFiles()
        {
            return _dirWalker.Walk(_sourceDir, fp =>
            {
                if (fp.IsMediaFile())
                {
                    TagLibFile tLFile;
                    try
                    {
                        tLFile = TagLibFile.Create(fp);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{e.GetType()}: {e.Message}");
                        return new RebuildWalkResult
                        {
                            IsValidToMove = false,
                            IsMediaFile = true,
                            OldPath = fp,
                        };
                    }
                    var filename = Path.GetFileName(fp);
                    var album = _fileSystemHelpers.MakeStringPathSafe(tLFile.Tag.Album ?? "");
                    var artistTag = string.Join(", ", tLFile.Tag.AlbumArtists).Trim();
                    if (string.IsNullOrWhiteSpace(artistTag))
                        artistTag = string.Join(", ", tLFile.Tag.Artists).Trim();
                    if (string.IsNullOrWhiteSpace(artistTag))
                        artistTag = string.Join(", ", tLFile.Tag.Performers).Trim();
                    var artist = _fileSystemHelpers.MakeStringPathSafe(artistTag);
                    var newPath = "";
                    var isValid = !string.IsNullOrWhiteSpace(album) && !string.IsNullOrWhiteSpace(artist);
                    if (isValid)
                        newPath = Path.Combine(_outDir, artist, album, filename);
                    return new RebuildWalkResult
                    {
                        Album = album,
                        Artist = artist,
                        IsValidToMove = isValid,
                        OldPath = fp,
                        NewPath = newPath,
                        IsMediaFile = true,
                        RequiresMove = fp != newPath
                    };
                }
                return new RebuildWalkResult
                {
                    IsValidToMove = false,
                    IsMediaFile = false,
                    OldPath = fp,
                };
            });
        }

        public class RebuildWalkResult
        {
            public string Album { get; set; }
            public string Artist { get; set; }
            public string OldPath { get; set; }
            public string NewPath { get; set; }
            public bool IsMediaFile { get; set; }
            public bool IsValidToMove { get; set; }
            public bool RequiresMove { get; set; }
        }

    }
}
