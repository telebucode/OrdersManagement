using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using OrdersManagement;
using OrdersManagement.Model;
using OrdersManagement.Core;
using Newtonsoft.Json.Linq;

namespace Web.AjaxHandlers
{
    /// <summary>
    /// Summary description for Quotations
    /// </summary>
    public class Quotations : IHttpHandler
    {
        private JObject errorJSon = new JObject(new JProperty("Success", false), new JProperty("Message", ""));
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request["Action"] == null)
            {
                context.Response.StatusCode = 400;
                context.Response.End();
            }
            try
            {
                switch (context.Request["Action"].ToString())
                {
                    case "GetQuotationStatuses":
                        GetQuotationStatuses(context);
                        break;
                    case "GetQuotationChannels":
                        GetQuotationChannels(context);
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
        private void GetQuotationStatuses(HttpContext context)
        {
            bool onlyActive = true;
            if (context.Request["OnlyActive"] != null && !bool.TryParse(context.Request["OnlyActive"].ToString(), out onlyActive))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetQuotationStatuses(onlyActive));
        }
        private void GetQuotationChannels(HttpContext context)
        {
            bool onlyActive = true;
            if (context.Request["OnlyActive"] != null && !bool.TryParse(context.Request["OnlyActive"].ToString(), out onlyActive))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetQuotationChannels(onlyActive));
        }
        private void Search(HttpContext context)
        {
            int quotationId = 0;
            string quotationNumber = string.Empty;
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
            if (context.Request["QuotationId"] != null && !int.TryParse(context.Request["QuotationId"].ToString(), out quotationId))
                GenerateErrorResponse(400, string.Format("QuotationId Should be a number"));
            if (context.Request["QuotationNumber"] != null)
                quotationNumber = context.Request["QuotationNumber"].ToString();
            if (context.Request["AccountId"] != null && !int.TryParse(context.Request["AccountId"].ToString(), out accountId))
                GenerateErrorResponse(400, string.Format("AccountId Should be a number"));
            if (context.Request["EmployeeId"] != null && !int.TryParse(context.Request["EmployeeId"].ToString(), out employeeId))
                GenerateErrorResponse(400, string.Format("EmployeeId should be a number"));
            if (context.Request["OwnerShipId"] != null && !int.TryParse(context.Request["OwnerShipId"].ToString(), out ownerShipId))
                GenerateErrorResponse(400, string.Format("OwnerShipId should be a number"));
            if (context.Request["StatusId"] != null && !byte.TryParse(context.Request["StatusId"].ToString(), out statusId))
                GenerateErrorResponse(400, string.Format("StatusId should be a number"));
            if (context.Request["ChannelId"] != null && !sbyte.TryParse(context.Request["ChannelId"].ToString(), out channelId))
                GenerateErrorResponse(400, string.Format("ChannelId should be a number"));
            if (context.Request["BillingModeId"] != null && !byte.TryParse(context.Request["BillingModeId"].ToString(), out billingModeId))
                GenerateErrorResponse(400, string.Format("BillingModeId should be a number"));
            if (context.Request["FromDateTime"] != null && !DateTime.TryParse(context.Request["FromDateTime"].ToString(), out fromDateTime))
                GenerateErrorResponse(400, string.Format("FromDateTime is not a valid datetime"));
            if (context.Request["ToDateTime"] != null && !DateTime.TryParse(context.Request["ToDateTime"].ToString(), out toDateTime))
                GenerateErrorResponse(400, string.Format("ToDateTime is not a valid datetime"));
            if (context.Request["PageNumber"] != null && !int.TryParse(context.Request["PageNumber"].ToString(), out pageNumber))
                GenerateErrorResponse(400, string.Format("PageNumber should be a number"));
            if (context.Request["Limit"] != null && !byte.TryParse(context.Request["Limit"].ToString(), out limit))
                GenerateErrorResponse(400, string.Format("Limit should be a number"));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetQuotations(quotationId: quotationId, quotationNumber: quotationNumber, accountId: accountId,
                employeeId: employeeId, ownerShipId: ownerShipId, statusId: statusId, channelId: channelId, ipAddress: ipAddress,
                billingModeId: billingModeId, fromDateTime: fromDateTime, toDateTime: toDateTime, pageNumber: pageNumber, limit: limit));
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