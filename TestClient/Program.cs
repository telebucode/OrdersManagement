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
                //dynamic d = c.CreateQuotation(1, 1, 1, 1, "{'Balance':{'Id':'1','Occurance':'1','Amount':'1500','PPPS':'1','PromotionalPulse/Voi':'1','TPPS':'1','TransactionalPulse/V':'1','ValidityInDays':'15','ExtraCharges':[{'Description':'Additional','Amount':'15'}]}}", "", 1);
                dynamic d = c.CreateQuotation(1, 3, 1, 1, "{'Balance':{'Id':'1','Occurance':'1','Amount':'1500','PPPS':'1','PromotionalPulse/Voi':'1','TPPS':'1','TransactionalPulse/V':'1','ValidityInDays':'15','ExtraCharges':[{'Description':'Additional','Amount':'15'}]}}", "::1", 1);
                //dynamic d = c.CreateQuotation(1, 1, 1, 1, "{'Balance':[{'Id':'1','Occurance':'1','Amount': '100','ValidityInDays': '10','TPPS': '10','PPPS': '10','TransactionalPulse/V': '','PromotionalPulse/Voi': '','ExtraCharges' : [{'Description': 'raja','Amount': '109'}]},{'Id':'2','Occurance':'2','Amount': '500','ValidityInDays': '50','TPPS': '50','PPPS': '50','TransactionalPulse/V': '','PromotionalPulse/Voi': '','ExtraCharges' : [{'Description': 'raja','Amount': '8909'}]}],'SenderName':{'Id':'1','Occurance':'1','SetupCharge': '0','Name': 'Tez','Price': '1000','ValidityInDays': '30','MonthlyRental': '1000'}}", "", 1);
                //dynamic d1 = c.GetServices(productId: 1, includeServiceProperties: true);
                //dynamic d = c.GetServiceProperties(1, true, null);
                //dynamic d = c.GetServices(productId: 1, includeServiceProperties: true, serviceId: 1);
                //dynamic d1 = c.GetQuotationDetails(1, false);
                //dynamic d = c.UpdateQuotation(1, 1, "{'Balance':[{'Id':'1','Occurance':'1','Amount': '1200','ValidityInDays': '120','TPPS': '120','PPPS': '120','TransactionalPulse/V': '','PromotionalPulse/Voi': '','ExtraCharges' : [{'Description': 'raja','Amount': '1090'}]},{'Id':'2','Occurance':'2','Amount': '5200','ValidityInDays': '520','TPPS': '520','PPPS': '520','TransactionalPulse/V': '','PromotionalPulse/Voi': '','ExtraCharges' : [{'Description': 'raja','Amount': '8909'}]},{'Id':'0','Occurance':'3','Amount': '500','ValidityInDays': '50','TPPS': '50','PPPS': '50','TransactionalPulse/V': '','PromotionalPulse/Voi': '','ExtraCharges' : [{'Description': 'raja','Amount': '8909'}]}]}", "", 1);
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
