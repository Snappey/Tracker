using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Tracker.Core;

namespace Tracker.Types
{
    internal class Folder
    {
        private string _path;

        private Folder _parent;
        private List<Folder> _children;
        private List<File> _files;
        private int _level;
        private FolderManager _manager;

        public Folder(string path, int level, FolderManager manager, Folder parent=null)
        {
            _path = path;
            _level = ++level;
            _manager = manager;

            if(parent == null)
            {
                _parent = this;
            }
            else
            {
                _parent = parent;
            }

            Log();
            manager.AddFolder(this);
            Scan();
        }

        public List<File> GetFiles()
        {
            return _files;
        }

        public List<Folder> GetDirectories()
        {
            return _children;
        }

        public List<Folder> GetDirectoriesFlat()
        {
            return _manager.Folders;
        }

        private void Scan()
        {
            _files = ScanFiles();
            _children = ScanChildren();
        }

        private List<Folder> ScanChildren()
        {
            string[] dirs = Directory.GetDirectories(_path);
            List<Folder> dirsList = new List<Folder>();

            for(int i = 0; i < dirs.Length; i++)
            {
                dirsList.Add(new Folder(dirs[i], _level, _manager, this));
            }
            return dirsList;
        }

        private List<File> ScanFiles()
        {
            string[] files = Directory.GetFiles(_path);
            List<File> filesList = new List<File>();

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                if (!file.Contains(".")) { continue; } // Continue on files that have no extension
                if (_manager.GetExtensions().ContainsKey(file.Substring(file.LastIndexOf("."))))
                {
                    filesList.Add(new File(_path, file.Substring(file.LastIndexOf("\\") + 1), _manager, this, _level));

                }
            }
            return filesList;
        }

        private void Log()
        {
            _manager.Log("".PadRight((_level - 1) * 4) + "| Registered Folder: " + _path);
        }

        private void CheckThreadStatus()
        {
            throw new NotImplementedException(); // Will be used to check if the thread should be stopped by checking the FolderManagers requested ThreadState
        }
    }
}
