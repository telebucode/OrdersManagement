using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using OrdersManagement;
using OrdersManagement.Core;
using OrdersManagement.Model;
using Newtonsoft.Json.Linq;

namespace Web.AjaxHandlers
{
    /// <summary>
    /// Summary description for Invoice
    /// </summary>
    public class Invoice : IHttpHandler
    {
        private JObject errorJSon = new JObject(new JProperty("Success", false), new JProperty("Message", ""));
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request["Action"] == null)
            {
                context.Response.StatusCode = 400;
                errorJSon["Message"] = "Action parameter is mandatory";
                context.Response.Write(errorJSon);
                context.Response.End();
            }
            try
            {
                switch(context.Request["Action"].ToString())
                {
                    case "GetStatuses":
                        GetStatuses(context);
                        break;
                    case "Search":
                        Search(context);
                        break;
                    case "Create":
                        CreateInvoice(context);
                        break;
                    case "View":
                        View(context);
                        break;
                    case "Download":
                        Download(context);
                        break;
                    case "Cancel":
                        CancelInvoice(context);
                        break;
                    default:
                        GenerateErrorResponse(400, string.Format("Invalid Action ({0})", context.Request["Action"].ToString()));
                        break;
                }
            }
            catch (System.Threading.ThreadAbortException e)
            { }
            catch (OrdersManagement.Exceptions.QuotationException e)
            {
                GenerateErrorResponse(500, e.Message);
            }
            catch (Exception e)
            {
                GenerateErrorResponse(500, e.Message);
            }
        }
        private void GetStatuses(HttpContext context)
        {
            bool onlyActive = true;
            if (context.Request["OnlyActive"] != null && !bool.TryParse(context.Request["OnlyActive"].ToString(), out onlyActive))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetInvoiceStatuses(onlyActive: onlyActive));
        }
        private void CreateInvoice(HttpContext context)
        {
            int quotationId = 0;
            byte billingModeId = 0;
            int employeeId = 0;
            if (context.Request["QuotationId"] != null && !int.TryParse(context.Request["QuotationId"].ToString(), out quotationId))
                GenerateErrorResponse(400, string.Format("QuotationId Must be a number"));
            if (context.Request["BillingModeId"] != null && !byte.TryParse(context.Request["BillingModeId"].ToString(), out billingModeId))
                GenerateErrorResponse(400, string.Format("BillingModeId Must be a number"));
            if (context.Request["EmployeeId"] != null && !int.TryParse(context.Request["EmployeeId"].ToString(), out employeeId))
                GenerateErrorResponse(400, string.Format("EmployeeId Must be a number"));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.CreateInvoice(quotationId, billingModeId, employeeId));
        }
        private void Search(HttpContext context)
        {
            byte productId = 0;
            int invoiceId = 0;
            string quotationNumber = string.Empty;
            string mobile = string.Empty;
            string email = string.Empty;
            int accountId = 0;
            int employeeId = 0;
            int ownerShipId = 0;
            byte statusId = 0;
            sbyte channelId = 0;
            string ipAddress = string.Empty;
            byte billingModeId = 0;
            DateTime fromDateTime = DateTime.Now.Date;
            DateTime toDateTime = DateTime.Now.AddDays(1).AddTicks(-1);
            int pageNumber = 1;
            byte limit = 20;
            JObject searchData = new JObject();
            searchData = JObject.Parse(context.Request["SearchData"]);

            if (searchData.SelectToken("ProductId") != null && !byte.TryParse(searchData.SelectToken("ProductId").ToString(), out productId))
                GenerateErrorResponse(400, string.Format("ProductId must be a number"));
            if (searchData.SelectToken("InvoiceId") != null && !int.TryParse(searchData.SelectToken("InvoiceId").ToString(), out invoiceId))
                GenerateErrorResponse(400, string.Format("QuotationId must be a number"));
            if (searchData.SelectToken("QuotationNumber") != null)
                quotationNumber = searchData.SelectToken("QuotationNumber").ToString();
            if (searchData.SelectToken("AccountId") != null && !int.TryParse(searchData.SelectToken("AccountId").ToString(), out accountId))
                GenerateErrorResponse(400, string.Format("AccountId must be a number"));
            if (searchData.SelectToken("EmployeeId") != null && !int.TryParse(searchData.SelectToken("EmployeeId").ToString(), out employeeId))
                GenerateErrorResponse(400, string.Format("EmployeeId must be a number"));
            if (searchData.SelectToken("OwnerShipId") != null && !int.TryParse(searchData.SelectToken("OwnerShipId").ToString(), out ownerShipId))
                GenerateErrorResponse(400, string.Format("OwnerShipId must be a number"));
            if (searchData.SelectToken("StatusId") != null && !byte.TryParse(searchData.SelectToken("StatusId").ToString(), out statusId))
                GenerateErrorResponse(400, string.Format("StatusId must be a number"));
            if (searchData.SelectToken("ChannelId") != null && !sbyte.TryParse(searchData.SelectToken("ChannelId").ToString(), out channelId))
                GenerateErrorResponse(400, string.Format("ChannelId must be a number"));
            if (searchData.SelectToken("BillingModeId") != null && !byte.TryParse(searchData.SelectToken("BillingModeId").ToString(), out billingModeId))
                GenerateErrorResponse(400, string.Format("BillingModeId must be a number"));
            if (searchData.SelectToken("FromDateTime") != null && !DateTime.TryParse(searchData.SelectToken("FromDateTime").ToString(), out fromDateTime))
                GenerateErrorResponse(400, string.Format("FromDateTime is not a valid datetime"));
            if (searchData.SelectToken("ToDateTime") != null && !DateTime.TryParse(searchData.SelectToken("ToDateTime").ToString(), out toDateTime))
                GenerateErrorResponse(400, string.Format("ToDateTime is not a valid datetime"));
            if (searchData.SelectToken("PageNumber") != null && !int.TryParse(searchData.SelectToken("PageNumber").ToString(), out pageNumber))
                GenerateErrorResponse(400, string.Format("PageNumber must be a number"));
            if (searchData.SelectToken("Limit") != null && !byte.TryParse(searchData.SelectToken("Limit").ToString(), out limit))
                GenerateErrorResponse(400, string.Format("Limit must be a number"));
            if (searchData.SelectToken("Mobile") != null)
                mobile = searchData.SelectToken("Mobile").ToString();
            if (searchData.SelectToken("Email") != null)
                mobile = searchData.SelectToken("Email").ToString();
            TablePreferences invoiceTablePreferences = new TablePreferences("", "", true, false);
            Dictionary<string, TablePreferences> invoiceDictionary = new Dictionary<string, TablePreferences>();
            invoiceDictionary.Add("Invoices", invoiceTablePreferences);
            OrdersManagement.Core.Client client = new OrdersManagement.Core.Client(responseFormat: OrdersManagement.ResponseFormat.JSON);
            context.Response.Write(client.GetInvoices(productId: productId, invoiceId: invoiceId, quotationNumber: quotationNumber, accountId: accountId,
                employeeId: employeeId, ownerShipId: ownerShipId, statusId: statusId, channelId: channelId, ipAddress: ipAddress,
                billingModeId: billingModeId, fromDateTime: fromDateTime, toDateTime: toDateTime, pageNumber: pageNumber, limit: limit, mobile: mobile, email: email, tablePreferences: invoiceDictionary));
        }
        private void View(HttpContext context)
        {
            int quotationId = 0;
            bool isPostPaidQuotation = false;
            if (context.Request["QuotationId"] != null && !int.TryParse(context.Request["QuotationId"].ToString(), out quotationId))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            if (quotationId <= 0)
                GenerateErrorResponse(400, string.Format("QuoationId must be greater than 0"));
            if (context.Request["IsPostPaidQuotation"] != null && !bool.TryParse(context.Request["IsPostPaidQuotation"].ToString(), out isPostPaidQuotation))
                GenerateErrorResponse(400, string.Format("IsPostPaidQuotation must be a boolean value"));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.ViewInvoice(quotationId, isPostPaidQuotation));
        }
        private void Download(HttpContext context)
        {
            int quotationId = 0;
            bool isPostPaidQuotation = false;
            if (context.Request["QuotationId"] != null && !int.TryParse(context.Request["QuotationId"].ToString(), out quotationId))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            if (quotationId <= 0)
                GenerateErrorResponse(400, string.Format("QuoationId must be greater than 0"));
            if (context.Request["IsPostPaidQuotation"] != null && !bool.TryParse(context.Request["IsPostPaidQuotation"].ToString(), out isPostPaidQuotation))
                GenerateErrorResponse(400, string.Format("IsPostPaidQuotation must be a boolean value"));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.DownloadInvoice(quotationId, isPostPaidQuotation));
        }
        //cancel Invoice
        private void CancelInvoice(HttpContext context)
        {
            int quotationId = 0;
            int adminId = 0;
            if (context.Request["QuotationId"] != null && !int.TryParse(context.Request["QuotationId"].ToString(), out quotationId))
                GenerateErrorResponse(400, string.Format("QuotationId Must be a number"));
            if (context.Request["AdminId"] != null && !int.TryParse(context.Request["AdminId"].ToString(), out adminId))
                GenerateErrorResponse(400, string.Format("AdminId Must be a number"));

            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.CancelInvoice(quotationId, adminId));
        }
        private void GenerateErrorResponse(int statusCode, string message)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.StatusCode = statusCode;
            errorJSon["Message"] = message;
            HttpContext.Current.Response.Write(errorJSon);
            //HttpContext.Current.ApplicationInstance.CompleteRequest();
            try
            {
                HttpContext.Current.Response.End();
            }
            catch (System.Threading.ThreadAbortException e)
            { }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}