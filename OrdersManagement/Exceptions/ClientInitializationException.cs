using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Exceptions
{
    public class ClientInitializationException : Exception
    {
        private string _message = string.Empty;
        public ClientInitializationException(string message)
        {
            this._message = message;
        }
    }
}
