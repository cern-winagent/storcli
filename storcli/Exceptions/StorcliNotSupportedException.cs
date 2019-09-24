using System.ComponentModel;

namespace storcli.Exceptions
{
    class StorcliNotSupportedException : WarningException
    {
        public StorcliNotSupportedException(string message) : base(message)
        {
            Data["continue"] = false;
        }
    }
}
