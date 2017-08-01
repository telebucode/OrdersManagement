using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Exceptions
{
    public class QuotationException : Exception
    {
        private string _message = string.Empty;
        public QuotationException(string message)
        {
            this._message = message;
        }
        public string Message { get { return this._message; } }
    }
}
