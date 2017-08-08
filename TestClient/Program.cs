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
                Client c = new Client(responseFormat: ResponseFormat.JSON);
                Dictionary<string, TablePreferences> prefs = new Dictionary<string, TablePreferences>();
                //prefs.Add("InputTypes", new TablePreferences(RootName: "FieldTypes", childElementNameForRows: "FieldType", columnValuesAsXmlAttributes: true, singleRowAsSingleEntity: true));                
                dynamic d = c.GetQuotationStatuses();
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
