using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Core
{
    public class Client
    {
        private string _connectionString = string.Empty;
        /// <summary>
        /// Initializes a Client Object.
        /// </summary>
        /// <param name="connectionString">Connection String To Orders Database</param>
        public Client(string connectionString)
        {
            this._connectionString = connectionString;            
        }
    }
}
