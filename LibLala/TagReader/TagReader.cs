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
            using var file = TagLib.File.Create(path);
            return new LibLalaTags(file, System.IO.Path.GetFullPath(path));
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
            ArgumentNullException.ThrowIfNull(path);
            // A file URI to its path. A path stays as it is: % is a letter in a file name, %20 is no space.
            if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase) && Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsFile)
            {
                path = uri.LocalPath;
            }

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
                throw new TagReaderException($"Can't open `{path}`.", ex);
            }
        }
    }
}
