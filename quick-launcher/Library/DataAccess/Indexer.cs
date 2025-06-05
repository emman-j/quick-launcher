using quick_launcher.Library.Data;
using quick_launcher.Library.Objects;
using quick_launcher.Library.Objects.ObjectCollection;
using quick_launcher.Library.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quick_launcher.Library.DataAccess
{
    public class Indexer
    {
        internal SQLiteDB _database;
        internal string _tableName = "FileIndex";

        public Indexer(SQLiteDB database)
        {
            _database = database;
            CreateDatabase();
        }

        private void CreateDatabase()
        {
            string schema = $@"
                CREATE TABLE IF NOT EXISTS {_tableName} (
                    Id INTEGER PRIMARY KEY,
                    FilePath TEXT UNIQUE,
                    FileName TEXT,
                    Extension TEXT,
                    Directory TEXT,
                    LastModified DATETIME,
                    Size INTEGER
                );
                CREATE INDEX IF NOT EXISTS idx_filename ON FileIndex(FileName);";

            if (_database.TableExists(_tableName)) return;

            _database.CreateTable(schema);
        }
        private void InsertOrIgnore(FileObject file)
        {
            var columnValues = new Dictionary<string, string>
            {
                { "FilePath", file.FilePath },
                { "FileName", file.FileName },
                { "Directory", file.Directory },
                { "LastModified", file.LastModified.ToString("o") },
                { "Size", file.Size.ToString() }
            };

            try
            {
                _database.InsertData(_tableName, columnValues);
            }
            catch
            {
                // File might already exist due to UNIQUE constraint — ignore
            }
        }
        public void IndexDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            foreach (var filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                FileInfo info = new FileInfo(filePath);
                var fileObject = new FileObject
                {
                    FilePath = filePath,
                    FileName = info.Name,
                    Directory = info.DirectoryName,
                    LastModified = info.LastWriteTimeUtc,
                    Size = info.Length
                };

                InsertOrIgnore(fileObject);
            }
        }
        public FileCollection Search(string keyword)
        {
            var results = new FileCollection();

            string query = $"SELECT * FROM {_tableName} WHERE FileName LIKE @keyword OR FilePath LIKE @keyword;";
            var parameters = new Dictionary<string, object> { { "keyword", $"%{keyword}%" } };

            DataTable dt = _database.ExecuteDynamicQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                var file = new FileObject
                {
                    FilePath = row["FilePath"].ToString(),
                    FileName = row["FileName"].ToString(),
                    Directory = row["Directory"].ToString(),
                    LastModified = DateTime.TryParse(row["LastModified"].ToString(), out var date) ? date : DateTime.MinValue,
                    Size = long.TryParse(row["Size"].ToString(), out var size) ? size : 0
                };
                results.Add(file);
            }

            return results;
        }
    }
}
