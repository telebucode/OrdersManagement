using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Exceptions
{
    public class ServiceException : Exception
    {
        private string _message = string.Empty;
        public ServiceException(string message)
        {
            this._message = message;
        }
        public string Message { get { return this._message; } }
    }
}
