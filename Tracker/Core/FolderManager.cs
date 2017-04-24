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


        public FolderManager(string path)
        {
            _path = path;
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

        #endregion

        private void Thread()
        {
            ScanFolderStructure();

        }

        private void ScanFolderStructure()
        {
            _root = new Folder(_path, _level, this);
        }

    }
}
