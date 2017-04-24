using System;
using System.Threading;
using Tracker.Core;

namespace Tracker
{
    class Program
    {
        private static FolderManager manager;

        private static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            manager = new FolderManager(@"F:\ll-city\beta\gamemodes");
           // manager = new FolderManager(@"F:\tracker-test");
            manager.AddExtension(".lua");
            manager.SetFlags(LoggingFlags.Console | LoggingFlags.File);
            manager.Start();

            while (manager.GetThreadState() == ThreadState.Running)
            {
                Thread.Sleep(50);
            }
            Console.ReadKey();
        }
    }
}