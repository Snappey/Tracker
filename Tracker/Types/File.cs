﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
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
        private FileResults _results;

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

        public string[] GetContents()
        {
            return System.IO.File.ReadAllLines(_path + "\\" + _name);
        }

        public string GetPath()
        {
            return _path;
        }

        public string GetName()
        {
            return _name;
        }

        public void SetResults(FileResults results)
        {
            _results = results;
        }

        public void WriteResults(string path)
        {
            var _lex = _results.DocumentedResults;
            if (_lex != null)
            {
                string json = JsonConvert.SerializeObject(_results, Formatting.Indented);
                System.IO.File.WriteAllText(path, json);
            }
            else
            {
                Console.WriteLine("Tried to write a file, which has not been read! (" + _name + ")");
            }
        }

        private void Log()
        {
            _manager.Log("".PadRight((_level - 1) * 4) + " | --> Registered File: " + _name);
        }
    }
}
