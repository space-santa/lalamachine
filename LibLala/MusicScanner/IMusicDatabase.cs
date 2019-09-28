namespace LibLala.MusicScanner
{
    public interface IMusicDatabase
    {
        void EnsureDatabase();
        void AddTagsToDatabase(LibLala.LibLalaTagReader.LibLalaTags tags);
        void SaveChanges();
    }
}
