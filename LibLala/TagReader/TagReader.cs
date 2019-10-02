using System;

namespace LibLala.LibLalaTagReader
{
    public class TagReaderException : Exception
    {
        public TagReaderException() : base() { }
        public TagReaderException(string message) : base(message) { }

        public TagReaderException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class TagCreator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "NSubstitute for testing.")]
        virtual public LibLalaTags Create(string path)
        {
            using (var file = TagLib.File.Create(path))
            {
                return new LibLalaTags(file, System.IO.Path.GetFullPath(path));
            }
        }
    }

    public class TagReader
    {
        public TagReader()
        {
            TagCreator = new TagCreator();
        }

        public TagCreator TagCreator { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public LibLalaTags Read(string path)
        {
            path = Uri.UnescapeDataString(path);
            path = Utils.StringUtils.RemoveFilePrefix(path);

            if (path.Length == 0)
            {
                throw new TagReaderException("You must give a value for path.");
            }

            try
            {
                return TagCreator.Create(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new TagReaderException($"Can't open `{path}`.");
            }
        }
    }
}
