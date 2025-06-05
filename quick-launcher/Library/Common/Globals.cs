using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quick_launcher.Library.Common
{
    public class Globals
    {
        public class Paths
        {
            //Root path for the application
            public static string AppDataPath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IMMS"); }
            public static string DbPath { get => Path.Combine(AppDataPath, "bin", "data"); }
        }
    }
}
