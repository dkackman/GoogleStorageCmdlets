using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace GoogleStorage
{
    class FileEnumerator
    {
        private DirectoryInfo _root;
        private string[] _fileMasks;

        public FileEnumerator(string root, string mask)
        {
            Debug.Assert(Directory.Exists(root));
            _root = new DirectoryInfo(root);
            _fileMasks = mask.Split(';');
        }

        public IEnumerable<FileInfo> GetFiles()
        {
            foreach (var file in ScanDirectory(_root))
            {
                yield return file;
            }
        }

        private IEnumerable<FileInfo> ScanDirectory(DirectoryInfo dir)
        {
            foreach (var mask in _fileMasks)
            {
                foreach (var file in dir.GetFiles(mask))
                {
                    yield return file;
                }
            }

            foreach (var subDir in dir.GetDirectories())
            {
                foreach (var file in ScanDirectory(subDir))
                {
                    yield return file;
                }
            }
        }
    }
}
