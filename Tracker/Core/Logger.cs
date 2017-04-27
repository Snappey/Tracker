using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tracker.Core
{
    [Flags]
    public enum LoggingFlags
    {
        None,
        Console,
        File,
    }

    class Logger
    {
        private string _name;
        private string _path;
        private FolderManager _manager;
        private LoggingFlags _flags;

        private FileStream _logFile;
        public BinaryWriter LogWriter;

        public Logger(FolderManager manager)
        {
            _manager = manager;
            _name = manager.GetName();
            _path = Directory.GetCurrentDirectory() + _name + ".txt";
            CreateLogFile();
        }

        public void Log(string log)
        {
            if (_flags.HasFlag(LoggingFlags.Console))
            {
                Console.WriteLine(log);
            }
            if (_flags.HasFlag(LoggingFlags.File))
            {
                LogWriter.Write(log + "\n");
                LogWriter.Flush();
            }
        }

        public void SetFlag(LoggingFlags flag)
        {
            _flags = flag;
        }

        private void CreateLogFile()
        {
            if (!File.Exists(_path))
            {
                _logFile = new FileStream(_path, FileMode.CreateNew);
            }
            else
            {
                _logFile = new FileStream(_path, FileMode.Open);
                _logFile.Flush(false);
            }
            LogWriter = new BinaryWriter(_logFile);
        }
    }
}
