using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tracker.Core;

namespace Tracker.Types
{
    internal class File
    {
        private string _path;
        private string _name;
        private int _level;
        private Folder _parent;
        private FolderManager _manager;

        private string[] _contents;
        public readonly string Contents; // TODO: determine if we should store it as a string array or a whole string

        public File(string path, string name, FolderManager manager, Folder parent, int level=-1)
        {
            _path = path;
            _name = name;
            _parent = parent;
            _manager = manager;
            _level = level;

            Log();
            //GetContents(); 
        }

        private void GetContents()
        {
            _contents = System.IO.File.ReadAllLines(_path + "\\" + _name);
        }

        private void Log()
        {
            Console.WriteLine("".PadRight(_level * 4) + " | --> Registered File: " + _name);
        }
    }
}
