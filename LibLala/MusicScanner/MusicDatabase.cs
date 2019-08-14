using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace LibLala.MusicScanner
{
    public interface IMusicDatabase
    {
        void EnsureDatabase();
        void AddTagsToDatabase(LibLala.TagReader.Tags tags);
        void SaveChanges();
    }

    public class MusicDatabase : IMusicDatabase
    {
        private string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "test.db3"
        );

        public void SaveChanges()
        {
            //
        }

        private SQLiteConnection m_dbConnection;
        public string DbPath => dbPath;

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
                    var sql = "DELETE FROM musiclib";
                    var tableCommand = new SQLiteCommand(sql, m_dbConnection);
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
            var tableSql = "CREATE TABLE `musiclib` (\n" +
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
            var tableCommand = new SQLiteCommand(tableSql, m_dbConnection);
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

        private SQLiteCommand GetTagsInsertCommand(LibLala.TagReader.Tags tags)
        {
            var sql = "INSERT into `musiclib` (`album`, `artist`, `comment`, `genre`, " +
                "`length`, `lengthString`, `mrl`, `path`, `title`, `track`, `year`, `discNumber`) " +
                "VALUES (@album, @artist, @comment, @genre, " +
                "@length, @lengthString, @mrl, @path, " +
                "@title, @track, @year, @discNumber)";
            var command = m_dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.Parameters.Add(new SQLiteParameter("@album", tags.Album));
            command.Parameters.Add(new SQLiteParameter("@artist", tags.ArtistString));
            command.Parameters.Add(new SQLiteParameter("@comment", tags.Comment));
            command.Parameters.Add(new SQLiteParameter("@genre", tags.GenreString));
            command.Parameters.Add(new SQLiteParameter("@length", tags.length));
            command.Parameters.Add(new SQLiteParameter("@lengthString", tags.LengthString));
            command.Parameters.Add(new SQLiteParameter("@mrl", tags.path));
            command.Parameters.Add(new SQLiteParameter("@path", tags.path));
            command.Parameters.Add(new SQLiteParameter("@title", tags.Title));
            command.Parameters.Add(new SQLiteParameter("@track", tags.Track));
            command.Parameters.Add(new SQLiteParameter("@year", tags.Year));
            command.Parameters.Add(new SQLiteParameter("@discNumber", tags.DiscNumber));
            return command;
        }

        public void AddTagsToDatabase(LibLala.TagReader.Tags tags)
        {
            if (!tags.isValid())
            {
                return;
            }

            m_dbConnection.Open();
            var command = GetTagsInsertCommand(tags);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        public LibLala.TagReader.Tags GetTag(string path)
        {
            m_dbConnection.Open();
            var sql = $"SELECT * FROM `musiclib` WHERE path='{path}';";
            var command = new SQLiteCommand(sql, m_dbConnection);
            var reader = command.ExecuteReader();

            var tags = TagsFromReader(reader)[0];
            m_dbConnection.Close();
            return tags;
        }

        private static List<LibLala.TagReader.Tags> TagsFromReader(SQLiteDataReader reader)
        {
            var tagList = new List<LibLala.TagReader.Tags>();

            while (reader.Read())
            {
                var tags = new LibLala.TagReader.Tags
                {
                    Album = reader.GetString(0),
                    ArtistString = reader.GetString(1)
                };
                try
                {
                    tags.Comment = reader.GetString(2);
                }
                catch
                {
                    tags.Comment = "";
                }
                try
                {
                    tags.GenreString = reader.GetString(3);
                }
                catch
                {
                }
                tags.length = reader.GetInt32(4);
                tags.path = reader.GetString(7);
                tags.Title = reader.GetString(8);
                tags.Track = (uint)reader.GetInt32(9);
                tags.Year = (uint)reader.GetInt32(10);
                tags.DiscNumber = (uint)reader.GetInt32(11);
                tagList.Add(tags);
            }

            return tagList;
        }
    }
}
