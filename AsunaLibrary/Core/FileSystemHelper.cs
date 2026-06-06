using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AsunaLocalSearch.Core
{
    internal class FileSystemHelper
    {

        internal static List<FileNameAndPath> Search(string path, string searchPattern)
        {
            var results = new List<FileNameAndPath>();
            Search(path, searchPattern, results);
            return results;
        }

        private static void Search(string path, string searchPattern, List<FileNameAndPath> results)
        {
            try
            {
                int pathHash = path.GetHashCode();
                foreach (var file in Directory.GetFiles(path, searchPattern))
                {
                    results.Add(new FileNameAndPath
                    {
                        Filename = Path.GetFileName(file),
                        Path = path,
                        PathHash = pathHash
                    });
                }
                
                foreach (var subdirectory in Directory.GetDirectories(path))
                    Search(subdirectory, searchPattern, results);
            }
            catch(Exception e)
            { }
        }
    }

    internal struct FileNameAndPath
    {
        internal string Filename { get; set; }
        internal string Path { get; set; }
        internal int PathHash { get; set; }
    }
}
