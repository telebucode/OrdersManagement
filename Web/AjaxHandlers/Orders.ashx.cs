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
    /// Summary description for Orders
    /// </summary>
    public class Orders : IHttpHandler
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
                switch (context.Request["Action"].ToString())
                {
                    case "GetOrderStatuses":
                        GetOrderStatuses(context);
                        break;
                    case "Search":
                        Search(context);
                        break;
                    case "GenerateOrderForOnlinePayments":
                        GenerateOrderForOnlinePayments(context);
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
        private void GenerateOrderForOnlinePayments(HttpContext context)
        {
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GenerateOrderForOnlinePayments(1,1,5,"",100,18,118,"ABC","XYZ",null));
        }
        private void GetOrderSummary(HttpContext context)
        {
            int quotationId = 0;
            if (context.Request["QuotationId"] != null && !int.TryParse(context.Request["OnlyActive"].ToString(), out quotationId))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetOrderSummary(quotationId));
        }
        private void GetOrderStatuses(HttpContext context)
        {
            bool onlyActive = true;
            if (context.Request["OnlyActive"] != null && !bool.TryParse(context.Request["OnlyActive"].ToString(), out onlyActive))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetOrderStatuses(onlyActive));
        }
        private void Search(HttpContext context)
        {
            byte productId = 0;
            int accountId = 0;
            string mobile = string.Empty;
            string email = string.Empty;
            string number = string.Empty;
            byte orderStatus = 0;
            byte billingMode = 0;
            DateTime fromDateTime = DateTime.Now.Date;
            DateTime toDateTime = DateTime.Now.AddDays(1).AddTicks(-1);
            int pageNumber = 1;
            byte limit = 20;

            JObject searchData = new JObject();
            searchData = JObject.Parse(context.Request["SearchData"].ToString());
            if (searchData.SelectToken("ProductId") != null && !byte.TryParse(searchData.SelectToken("ProductId").ToString(), out productId))
                GenerateErrorResponse(400, string.Format("ProductId must be a number"));
            if (searchData.SelectToken("Number") != null)
                number = searchData.SelectToken("QuotationNumber").ToString();
            if (searchData.SelectToken("AccountId") != null && !int.TryParse(searchData.SelectToken("AccountId").ToString(), out accountId))
                GenerateErrorResponse(400, string.Format("AccountId must be a number"));
            if (searchData.SelectToken("Mobile") != null)
                mobile = searchData.SelectToken("Mobile").ToString();
            if (searchData.SelectToken("Email") != null)
                email = searchData.SelectToken("Email").ToString();
            if (searchData.SelectToken("OrderStatus") != null && !byte.TryParse(searchData.SelectToken("OrderStatus").ToString(), out orderStatus))
                GenerateErrorResponse(400, string.Format("PaymentStatus must be a number"));
            if (searchData.SelectToken("BillingMode") != null && !byte.TryParse(searchData.SelectToken("BillingMode").ToString(), out billingMode))
                GenerateErrorResponse(400, string.Format("BillingMode must be a number"));
            if (searchData.SelectToken("FromDateTime") != null && !DateTime.TryParse(searchData.SelectToken("FromDateTime").ToString(), out fromDateTime))
                GenerateErrorResponse(400, string.Format("FromDateTime is not a valid datetime"));
            if (searchData.SelectToken("ToDateTime") != null && !DateTime.TryParse(searchData.SelectToken("ToDateTime").ToString(), out toDateTime))
                GenerateErrorResponse(400, string.Format("ToDateTime is not a valid datetime"));
            if (searchData.SelectToken("PageNumber") != null && !int.TryParse(searchData.SelectToken("PageNumber").ToString(), out pageNumber))
                GenerateErrorResponse(400, string.Format("PageNumber must be a number"));
            if (searchData.SelectToken("Limit") != null && !byte.TryParse(searchData.SelectToken("Limit").ToString(), out limit))
                GenerateErrorResponse(400, string.Format("Limit must be a number"));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetOrders(productId: productId, accountId: accountId, mobile: mobile, email: email, orderStatus: orderStatus,
                number: number, billingMode: billingMode, fromDateTime: fromDateTime, toDateTime: toDateTime, accountName: "", pageNumber: pageNumber, limit: limit));
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