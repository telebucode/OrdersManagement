using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrdersManagement;
using OrdersManagement.Core;
using OrdersManagement.Model;
using OrdersManagement.Exceptions;
using Newtonsoft.Json.Linq;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Client c = new Client();
                JObject j = c.GetServices();
                Console.WriteLine(j);
            }
            catch(ClientInitializationException e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
    }
}
