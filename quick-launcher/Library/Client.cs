using quick_launcher.Library.Common;
using quick_launcher.Library.Data;
using quick_launcher.Library.DataAccess;
using quick_launcher.Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quick_launcher.Library
{
    public class Client
    {
        public SQLiteDB IndexDB { get; set; }
        public Indexer Indexer { get; set; }

        public Client()
        {
            IndexDB = new SQLiteDB(Globals.Paths.DbPath, "indexDB");
            Indexer = new Indexer(IndexDB);
        }
    }
}
