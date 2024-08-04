using System;

namespace LibLala.Unittests.Tagreader
{
    public class TestException : Exception
    {
        public TestException(string? message) : base(message)
        {
        }

        public TestException()
        {
        }

        public TestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
