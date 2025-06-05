using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quick_launcher.Library.Objects
{
    public class FileObject
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Directory { get; set; }
        public DateTime LastModified { get; set; }
        public long Size { get; set; }

        public override string ToString()
        {
            return $"{FileName} ({Size} bytes) - Last Modified: {LastModified}";
        }
    }
}
