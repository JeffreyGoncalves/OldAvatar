using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.Exceptions
{
    /// <summary>
    /// Exception raised when a file can't be create because of a conflicts
    /// Added by Baptiste Germond
    /// </summary>
    class CantCreateFileException : Exception
    {
        public string textError;

        public CantCreateFileException()
        {
        }

        public CantCreateFileException(string title, string text) : base(title)
        {
            textError = text;
        }
    }
}
