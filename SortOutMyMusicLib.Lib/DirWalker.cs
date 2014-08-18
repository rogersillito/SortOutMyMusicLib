using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SortOutMyMusicLib.Lib
{
    public class DirWalker
    {
        public static IEnumerable<T> Walk<T>(string sDir, Func<string, T> callback)
        {
            foreach (var yielded in Directory.GetDirectories(sDir).SelectMany(d => Walk(d, callback)))
            {
                yield return yielded;
            }
            foreach (var f in Directory.GetFiles(sDir))
            {
                yield return callback(f);
            }
        }
    }
}