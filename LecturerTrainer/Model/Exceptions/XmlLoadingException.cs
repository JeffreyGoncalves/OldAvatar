using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LecturerTrainer.Model.Exceptions
{
    /// <summary>
    /// Exception Launched when an error occurs while loading a xml file
    /// </summary>
    class XmlLoadingException : Exception
    {
        public string title { get; set; }
        public XmlLoadingException()
        {

        }

        public XmlLoadingException(string title, string message) : base(message)
        {
            this.title = title;
        }
    }
}
