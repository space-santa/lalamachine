using System;
using System.Runtime.Serialization;

namespace Lalamachine.Wpf.ViewModel
{
    [Serializable]
    public class EndOfPlaylistException : Exception
    {
        public EndOfPlaylistException()
        {
        }

        public EndOfPlaylistException(string message) : base(message)
        {
        }

        public EndOfPlaylistException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
