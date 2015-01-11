using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace GoogleStorage
{
    class FileEnumerator
    {
        private DirectoryInfo _root;
        private string[] _fileMasks;
        private bool _resurse;

        public FileEnumerator(string root, string mask, bool recurse)
        {
            Debug.Assert(Directory.Exists(root));
            _root = new DirectoryInfo(root);
            _fileMasks = mask.Split(';');
            _resurse = recurse;
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
                foreach (var file in dir.GetFiles(mask.Trim()))
                {
                    yield return file;
                }
            }

            if (_resurse)
            {
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
}