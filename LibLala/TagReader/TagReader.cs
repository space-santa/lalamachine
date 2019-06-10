﻿using System;

namespace LibLala.TagReader
{
    public class TagReaderException : Exception
    {
        public TagReaderException(string message) : base(message)
        {
        }
    }

    public abstract class ITagCreator
    {
        public abstract Tags Create(string path);
    }

    public class TagCreator : ITagCreator
    {
        public override Tags Create(string path)
        {
            var tags = new Tags();
            tags.FromTagLibFile(TagLib.File.Create(path));
            return tags;
        }
    }

    public class TagReader
    {
        public TagReader()
        {
            TagCreator = new TagCreator();
        }

        public ITagCreator TagCreator { get; set; }

        public Tags Read(string path)
        {
            path = Uri.UnescapeDataString(path);
            path = Utils.RemoveFilePrefix(path);

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