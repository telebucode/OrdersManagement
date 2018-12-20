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
                dynamic d = c.ActivateOrder(7, false, 100, "http://staging.grptalk.com/v2.1/ServiceActivation", 1);
                //dynamic d = c.GetQuotationDetails(4026, false);
                // dynamic d1 = c.GetQuotationDetails(4029, false);
                //dynamic d = c.CreateQuotation(1, 1, 1, 1, "{'Balance':{'Id':'1','Occurance':'1','Amt':'1000','IBPrice':'1.2','OBPrice':'1.3','ValidityInDays':'150'},'QuotationType':'1'}", "", 1 , 1);
               // dynamic d = c.CreateQuotation(1, 1, 1, 1, "{'Balance':{'Id':'1','Occurance':'1','Amt':'165251.00','IBPrice':'0.65','OBPrice':'0.65','ValidityInDays':'120'}}", "", 1);
                //dynamic d1 = c.GetServices(productId: 1, includeServiceProperties: true);
                //dynamic d = c.UpdateServiceProperty(2, "{'DisplayName': 'Lines','MetaDataCode': 'Lines','IsRequired': false,'IncludeInOrderAmount': false,'InputTypeId': '1','DataTypeId': '1','PropertyFields': [{'MinLength': 0,'MaxLength': 0,'IsAllowSpecialChars': false}]}", null);
                //dynamic d = c.GetServices(productId: 1, includeServiceProperties: true, serviceId: 1);
                //dynamic d1 = c.GetQuotationDetails(1, false);
                //dynamic d = c.CreateServiceProperties(3,"{'DisplayName':'Concurrency','MetaDataCode':'Lines','IsRequired':true,'IncludeInOrderAmount':false,'InputTypeId':'1','DataTypeId':'1','PropertyFields':[{'MinLength':0,'MaxLength':0,'IsAllowSpecialChars':false}]}",null)
                //Console.WriteLine(d.ToString());
            }
            catch (ClientInitializationException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (QuotationException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
    }
}
