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
            Console.WriteLine("Hello World!");
            manager = new FolderManager(@"F:\ll-city\beta");
            manager.AddExtension(".lua");
            manager.Start();

            while (manager.GetThreadState() == ThreadState.Running)
            {
                Thread.Sleep(50);
            }
        }
    }
}