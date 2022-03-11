using System;

namespace UpdaterLibrary.Exceptions
{
    internal class EmptySettingsException : Exception
    {
        public EmptySettingsException()
        {

        }

        public EmptySettingsException(string message)
            : base(message)
        {
        }
    }
}
