using System.ComponentModel;

namespace storcli.Exceptions
{
    class WrongPathException : WarningException
    {
        public WrongPathException(string message) : base(message)
        {

        }
    }
}
