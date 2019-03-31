using System.Collections.Generic;
using TagReader;
using System.Data.SQLite;
using System.IO;
using System;
using System.Data;

namespace MusicScanner
{
    public interface IMusicDatabase
    {
        void EnsureDatabase();
        void AddTagsToDatabase(Tags tags);
    }

    public class MusicDatabase : IMusicDatabase
    {
        private string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "test.db3"
        );

        SQLiteConnection m_dbConnection;
        public string DbPath
        {
            get
            {
                return dbPath;
            }
        }

        public void Connect(string path)
        {
            if (path.Length > 0)
            {
                dbPath = path;
            }
            m_dbConnection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
        }

        private void ReplaceOldDatabaseFile()
        {
            if (File.Exists(dbPath))
            {
                try
                {
                    // If LalaServer is started by lalamachine, this delete will probably fail because the file is opened by lalamachine.
                    File.Delete(dbPath);
                }
                catch (IOException)
                {
                    m_dbConnection.Open();
                    string sql = "DELETE FROM musiclib";
                    SQLiteCommand tableCommand = new SQLiteCommand(sql, m_dbConnection);
                    try
                    {
                        tableCommand.ExecuteNonQuery();
                    }
                    catch (SQLiteException)
                    {
                        // Database seems to be empty.
                    }
                    m_dbConnection.Close();
                }
            }
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }
        }

        public void EnsureDatabase()
        {
            ReplaceOldDatabaseFile();
            m_dbConnection.Open();
            string tableSql = "CREATE TABLE `musiclib` (\n" +
                "`album` TEXT,\n" +
                "`artist` TEXT,\n" +
                "`comment` TEXT,\n" +
                "`genre` TEXT,\n" +
                "`length` int NOT NULL,\n" +
                "`lengthString` TEXT NOT NULL,\n" +
                "`mrl` TEXT NOT NULL PRIMARY KEY,\n" +
                "`path` TEXT NOT NULL,\n" +
                "`title` TEXT NOT NULL,\n" +
                "`track` int,\n" +
                "`year` int,\n" +
                "`discNumber` int NOT NULL DEFAULT 1\n" +
                ")";
            SQLiteCommand tableCommand = new SQLiteCommand(tableSql, m_dbConnection);
            try
            {
                tableCommand.ExecuteNonQuery();
            }
            catch
            {
                //
            }
            m_dbConnection.Close();
        }

        private SQLiteCommand GetTagsInsertCommand(Tags tags)
        {
            string sql = "INSERT into `musiclib` (`album`, `artist`, `comment`, `genre`, " +
                "`length`, `lengthString`, `mrl`, `path`, `title`, `track`, `year`, `discNumber`) " +
                "VALUES (@album, @artist, @comment, @genre, " +
                "@length, @lengthString, @mrl, @path, " +
                "@title, @track, @year, @discNumber)";
            SQLiteCommand command = m_dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.Parameters.Add(new SQLiteParameter("@album", tags.album));
            command.Parameters.Add(new SQLiteParameter("@artist", tags.artistString));
            command.Parameters.Add(new SQLiteParameter("@comment", tags.comment));
            command.Parameters.Add(new SQLiteParameter("@genre", tags.genreString));
            command.Parameters.Add(new SQLiteParameter("@length", tags.length));
            command.Parameters.Add(new SQLiteParameter("@lengthString", tags.lengthString));
            command.Parameters.Add(new SQLiteParameter("@mrl", tags.path));
            command.Parameters.Add(new SQLiteParameter("@path", tags.path));
            command.Parameters.Add(new SQLiteParameter("@title", tags.title));
            command.Parameters.Add(new SQLiteParameter("@track", tags.track));
            command.Parameters.Add(new SQLiteParameter("@year", tags.year));
            command.Parameters.Add(new SQLiteParameter("@discNumber", tags.discNumber));
            return command;
        }

        public void AddTagsToDatabase(Tags tags)
        {
            if (!tags.isValid())
            {
                return;
            }

            m_dbConnection.Open();
            SQLiteCommand command = GetTagsInsertCommand(tags);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        public Tags GetTag(string path)
        {
            m_dbConnection.Open();
            string sql = $"SELECT * FROM `musiclib` WHERE path='{path}';";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            var tags = TagsFromReader(reader)[0];
            m_dbConnection.Close();
            return tags;
        }

        private static List<Tags> TagsFromReader(SQLiteDataReader reader)
        {
            List<Tags> tagList = new List<Tags>();

            while (reader.Read())
            {
                Tags tags = new Tags();
                tags.album = reader.GetString(0);
                tags.artistString = reader.GetString(1);
                try
                {
                    tags.comment = reader.GetString(2);
                }
                catch
                {
                    tags.comment = "";
                }
                try
                {
                    tags.genreString = reader.GetString(3);
                }
                catch
                {
                }
                tags.length = reader.GetInt32(4);
                tags.path = reader.GetString(7);
                tags.title = reader.GetString(8);
                tags.track = (uint)reader.GetInt32(9);
                tags.year = (uint)reader.GetInt32(10);
                tags.discNumber = (uint)reader.GetInt32(11);
                tagList.Add(tags);
            }

            return tagList;
        }
    }
}
