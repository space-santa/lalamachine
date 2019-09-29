namespace LibLala.MusicScanner
{
    public interface IMusicDatabase
    {
        void EnsureDatabase();
        void AddTagsToDatabase(LibLalaTagReader.LibLalaTags tags);
        void SaveChanges();
    }
}
