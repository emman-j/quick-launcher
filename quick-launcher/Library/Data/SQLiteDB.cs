using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace quick_launcher.Library.Data
{
    public class SQLiteDB : IDisposable
    {
        internal bool isDisposed = false;
        private string _dbName;
        private string _dbFile;
        private string ConnectionString => $"Data Source = {_dbFile}";

        public SQLiteDB(string path, string dbname)
        {
            CreateDatabase(path, dbname);
        }

        private DbType GetDbType(object value)
        {
            if (value == null || value == DBNull.Value)
                return DbType.String;

            Type type = value.GetType();

            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
                return DbType.Int64;
            if (type == typeof(bool))
                return DbType.Boolean;
            if (type == typeof(DateTime))
                return DbType.DateTime;
            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                return DbType.Double;
            if (type == typeof(Guid))
                return DbType.String;

            return DbType.String;
        }

        public bool CreateDatabase(string path, string dbname)
        {
            if (string.IsNullOrWhiteSpace(dbname))
            {
                return false;
            }
            _dbName = dbname;
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                _dbFile = Path.Combine(path, dbname);

                if (!File.Exists(_dbFile))
                {
                    File.Create(_dbFile).Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create database.", ex);
            }
            return true;
        }

        public bool CreateTable(string tableSchema)
        {
            try
            {
                ExecuteNonQuery(ConnectionString, tableSchema);
            }
            catch (SQLiteException ex)
            {
                //throw new SQLiteException("Failed to create database.", ex.SQLiteException);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create database.", ex);
            }
            return true;
        }
        public bool TableExists(string tableName)  // Method to check if a table exists in the database
        {
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = $"PRAGMA table_info({tableName});";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        // Returns true if the table exists, otherwise false
                        return reader.HasRows;
                    }
                }
            }
        }
        public bool InsertData(string tableName, Dictionary<string, string> columnValues)  // Method to insert data dynamically using a Dictionary (column name, value)
        {
            try
            {
                if (!File.Exists(_dbFile))
                {
                    File.Create(_dbFile).Close();
                }

                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    List<string> columnNames = new List<string>();
                    List<string> parameterNames = new List<string>();

                    foreach (var pair in columnValues)
                    {
                        columnNames.Add(pair.Key);
                        parameterNames.Add($"@{pair.Key}");
                    }

                    string query = $"INSERT INTO {tableName} ({string.Join(",", columnNames)}) VALUES ({string.Join(",", parameterNames)});";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        foreach (var pair in columnValues)
                        {
                            command.Parameters.AddWithValue($"@{pair.Key}", pair.Value);
                        }
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }

                Console.WriteLine("Data inserted successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
            return true;
        }
        public DataTable ExecuteDynamicQuery(string tableName, Dictionary<string, string> parameters)
        {
            DataTable resultTable = new DataTable();
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    //string whereClause = string.Join(" AND ", parameters.Keys);
                    //whereClause = string.Join(" AND ", parameters.Keys.Select(key => $"{key} = @{key}"));
                    //string query = $"SELECT * FROM {tableName} WHERE {whereClause};";

                    string query = $"SELECT * FROM {tableName}";
                    if (parameters.Count > 0)
                    {
                        string whereClause = string.Join(" AND ", parameters.Keys.Select(key => $"{key} = @{key}"));
                        query += $" WHERE {whereClause}";
                    }
                    query += ";";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }

                        using (var adapter = new SQLiteDataAdapter(command))
                        {
                            adapter.Fill(resultTable);
                        }
                    }
                }
            }
            catch
            {

            }
            return resultTable;
        }
        public DataTable ExecuteDynamicQuery(string tableName, Dictionary<string, string> parameters, string[] selectColumns)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string selectClause = (selectColumns != null && selectColumns.Length > 0)
                    ? string.Join(", ", selectColumns)
                    : "*";

                string whereClause = parameters.Any()
                    ? "WHERE " + string.Join(" AND ", parameters.Keys.Select(key => $"{key} = @{key}"))
                    : "";

                string query = $"SELECT {selectClause} FROM {tableName} {whereClause};";

                using (var command = new SQLiteCommand(query, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                    }

                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        DataTable resultTable = new DataTable();
                        adapter.Fill(resultTable);
                        return resultTable;
                    }
                }
            }
        }
        public DataTable ExecuteDynamicQuery(string query, Dictionary<string, object> parameters)
        {
            DataTable datatable = new DataTable();
            try
            {

                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        foreach (var param in parameters)
                        {
                            string paramName = $"@{param.Key}";
                            object paramValue = param.Value;

                            DbType sqlType = GetDbType(paramValue);

                            command.Parameters.Add(paramName, sqlType).Value = paramValue ?? DBNull.Value;
                        }

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                        {
                            adapter.Fill(datatable);
                        }
                    }
                }
            }
            catch
            {

            }
            return datatable;
        }
        public void ExecuteNonQuery(string connectionString, string command)
        {
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(command, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create database.", ex);
            }
        }
        public int ExecuteNonQuery(string query, Dictionary<string, object> parameters)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    foreach (var param in parameters)
                    {
                        string paramName = $"@{param.Key}";
                        DbType sqlType = GetDbType(param.Value);
                        command.Parameters.Add(paramName, sqlType).Value = param.Value ?? DBNull.Value;
                    }

                    return command.ExecuteNonQuery(); // returns number of affected rows
                }
            }
        }


        public void Dispose()
        {
            try
            {
                if (isDisposed)
                {
                    return;
                }
                _dbFile = null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error disposing.", ex);
            }
        }


    }
}
