using System;
namespace UdemyDownloader.Exceptions
{
    public class MissingCredentialsException : Exception
    {
        public MissingCredentialsException()
            : base("Unable to acquire credentials during an operation that requires them.")
        {
        }
    }
}
