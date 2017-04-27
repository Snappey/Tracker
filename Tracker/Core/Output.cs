using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tracker.Types;

namespace Tracker.Core
{
    public enum OutputFlag
    {
        Replicate, // Replicate Folder Structure and Files (replacing files with generated json / undocumented function
        OneFileDocumented, // Push all the functions that are documented into one file (Big)
        OneFileUnDocumented, // Pusha all the functions that are undocumented into one file
        FolderOnly, // One File for each folder, replicating folder structure
    }

    class Output
    {
        private FolderManager _manager;
        private LexType _lexFlags;
        private OutputFlag _outputFlag;
        private string _path;

        public Output(FolderManager manager)
        {
            _manager = manager;
            _lexFlags = (LexType.Command | LexType.Function | LexType.Hook | LexType.HookCall);
            _outputFlag = OutputFlag.Replicate;
            _path = Directory.GetCurrentDirectory() +  "\\" + _manager.GetName();
        }

        public void SetLexFlags(LexType flag)
        {
            _lexFlags = flag;
        }

        public LexType GetLexFlags()
        {
            return _lexFlags;
        }

        public void SetOutputFlags(OutputFlag flag)
        {
            _outputFlag = flag;
        }

        public OutputFlag GetOutputFlags()
        {
            return _outputFlag;
        }

        public void Write()
        {
            switch (_outputFlag)
            {
                case OutputFlag.Replicate:
                    Replicate();
                    break;
                case OutputFlag.OneFileDocumented:
                    OneFileDocumented();
                    break;
                case OutputFlag.OneFileUnDocumented:
                    OneFileUnDocumented();
                    break;
                case OutputFlag.FolderOnly:
                    FolderOnly();
                    break;
            }


        }

        private void Replicate()
        {
            if (!FolderExists())
            {
                FolderCreate();
            }

            foreach (Folder folder in _manager.GetFoldersFlat())
            {
                Directory.CreateDirectory(_path + "\\" + folder.GetPath().Substring(folder.GetPath().LastIndexOf("gamemode")));

                foreach (Types.File file in folder.GetFiles())
                {
                    file.WriteResults(_path + "\\" + folder.GetPath().Substring(folder.GetPath().LastIndexOf("gamemode")) + "\\" + file.GetName() + ".txt");
                }
            }
        }

        private void OneFileDocumented()
        {
            
        }

        private void OneFileUnDocumented()
        {
            
        }

        private void FolderOnly()
        {
            
        }

        private bool FolderExists()
        {
            if (Directory.Exists(_path))
            {
                return true;
            }
            return false;
        }

        private void FolderCreate()
        {
            Directory.CreateDirectory(_path);
        }
    }
}
