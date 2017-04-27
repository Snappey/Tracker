using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tracker.Types;

namespace Tracker.Core
{
    class FolderManager
    {
        private string _path;
        private int _level = 0;
        private Dictionary<string, bool> _extensions = new Dictionary<string, bool>();

        private Thread _thread;
        private Folder _root;
        private Logger _logger;
        private Lexer _lexer;
        private Output _output;

        public List<Folder> Folders = new List<Folder>(); // All scanned folders in one 1d list

        public FolderManager(string path)
        {
            _path = path;
            _logger = new Logger(this);
            _logger.SetFlag(LoggingFlags.Console); // Default Flags
            _lexer = new Lexer();
            _output = new Output(this);
        }

        #region Public Functions

        public bool Start()
        {
            if ((_thread == null) || !_thread.IsAlive)
            {
                _thread = new Thread(Thread);
                _thread.Name = "FolderManager (" + _path + ")";
                _thread.Start();
                return true;
            }
            return false;
        }

        public bool Stop()
        {
            if ((_thread != null) && _thread.IsAlive)
            {
                _thread.Join(5000);
                return true;
            }
            return false;
        }

        public ThreadState GetThreadState()
        {
            if (_thread != null)
            {
                return _thread.ThreadState;
            }
            return ThreadState.Unstarted;
        }

        public string GetPath()
        {
            return _path;
        }

        public string GetName()
        {
            return _path.Substring(_path.LastIndexOf("\\") + 1);
        }

        public void AddExtension(string extension)
        {
            if (extension.StartsWith("."))
            {
                _extensions.Add(extension.Substring(1), true);
            }
            _extensions.Add(extension, true);
        }

        public void SetExtension(string extension, bool status)
        {
            if (_extensions.ContainsKey(extension))
            {
                _extensions[extension] = status;
            }
        }

        public Dictionary<string, bool> GetExtensions()
        {
            return _extensions;
        }

        public void AddFolder(Folder folder)
        {
            Folders.Add(folder);
        }

        public List<Folder> GetFoldersFlat()
        {
            return Folders;
        }

        public void Log(string log)
        {
            _logger.Log(log);
        }

        public void SetFlags(LoggingFlags flags)
        {
            _logger.SetFlag(flags);
        }

        #endregion

        private void Thread()
        {
            BuildFolderStructure();
            ReadFiles();
        }

        private void BuildFolderStructure()
        {
            Log("Building FileStructure for " + _path + " ~ " + DateTime.Now);
            _root = new Folder(_path, _level, this);
            Log("Finished Building FileStructure for " + _path + " ~ " + DateTime.Now);
        }

        private void ReadFiles() // TODO: Come up with better names
        {
            Log("File Lexing Started! ~ " + DateTime.Now);
            var folders =  _root.GetDirectoriesFlat();
            foreach(Folder folder in folders)
            {
                var files = folder.GetFiles();
                foreach (File file in files)
                {
                    var results = _lexer.LexFile(file.GetContents(),file.GetPath() + "\\" + file.GetName());
                    file.SetResults(results);
                    //file.WriteResults();
                }
            }
            Log("File Lexing Finished! ~ " + DateTime.Now);

            _output.Write();
        }
    }
}
