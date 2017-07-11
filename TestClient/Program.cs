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
                Client c = new Client(responseFormat: ResponseFormat.XML);
                Dictionary<string, TablePreferences> prefs = new Dictionary<string, TablePreferences>();
                prefs.Add("Services", new TablePreferences(RootName: "Products", childElementNameForRows: "Product", columnValuesAsXmlAttributes: false, singleRowAsSingleEntity: true));
                dynamic d = c.GetServices(serviceId:0, includeServiceProperties: true, tablePreferences: prefs, onlyActive: true);
                Console.WriteLine(d.ToString());
            }
            catch(ClientInitializationException e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
    }
}
