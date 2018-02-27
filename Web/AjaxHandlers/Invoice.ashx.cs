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
                    case "Create":
                        CreateInvoice(context);
                        break;
                    case "View":
                        View(context);
                        break;
                    case "Download":
                        Download(context);
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