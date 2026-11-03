using System;

namespace LibLala.Jellyfin
{
    // The message is for the user to read. It never has the API key in it.
    public sealed class JellyfinException : Exception
    {
        public JellyfinException()
        {
        }

        public JellyfinException(string message) : base(message)
        {
        }

        public JellyfinException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
