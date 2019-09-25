namespace LibLala.MusicScanner
{
    public interface IMusicDatabase
    {
        void EnsureDatabase();
        void AddTagsToDatabase(LibLala.TagReader.Tags tags);
        void SaveChanges();
    }
}
