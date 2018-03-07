using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json.Linq;
using OrdersManagement;
using OrdersManagement.Core;

namespace Web.AjaxHandlers
{
    /// <summary>
    /// Summary description for Payments
    /// </summary>
    public class Payments : IHttpHandler
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
                    case "GetBankAccounts":
                        GetBankAccounts(context);
                        break;
                    case "GetPaymentGateways":
                        GetPaymentGateways(context);
                        break;
                    case "GeneratePayment":
                        GeneratePayment(context);
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
        private void GetBankAccounts(HttpContext context)
        {
            bool onlyActive = true;
            if (context.Request["OnlyActive"] != null && !bool.TryParse(context.Request["OnlyActive"].ToString(), out onlyActive))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetBankAccounts(onlyActive));
        }
        private void GetPaymentGateways(HttpContext context)
        {
            bool onlyActive = true;
            if (context.Request["OnlyActive"] != null && !bool.TryParse(context.Request["OnlyActive"].ToString(), out onlyActive))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetPaymentGateways(onlyActive));
        }

        private void GeneratePayment(HttpContext context)
        {
            byte productId = 0;
            int invoiceId = 0;
            int accountId = 0;
            int paymentGatewayId = 0;
            float paymentAmount = 0;
            int employeeId = 0;
            int bankAccountId = 0;
            string comments = string.Empty;
            string ipAddress = string.Empty;
            int activatePercentage = 0;
            int billingModeId = 0;
            DateTime depositeDate;
            bool isTDSApplicable = false;
            int tdsPercentage = 0;
            JObject paymentData = new JObject();
            paymentData = JObject.Parse(context.Request["SearchData"]);
            if (paymentData.SelectToken("ProductId") != null && !byte.TryParse(paymentData.SelectToken("ProductId").ToString(), out productId))
                GenerateErrorResponse(400, string.Format("ProductId must be a number"));
            if (paymentData.SelectToken("InvoiceId") != null && !int.TryParse(paymentData.SelectToken("InvoiceId").ToString(), out invoiceId))
                GenerateErrorResponse(400, string.Format("InvoiceId must be a number"));
            if (paymentData.SelectToken("AccountId") != null && !int.TryParse(paymentData.SelectToken("AccountId").ToString(), out accountId))
                GenerateErrorResponse(400, string.Format("AccountId must be a number"));
             if (paymentData.SelectToken("BillingModeId") != null && !int.TryParse(paymentData.SelectToken("BillingModeId").ToString(), out billingModeId))
                GenerateErrorResponse(400, string.Format("BillingModeId must be a number"));
            if (paymentData.SelectToken("PaymentGatewayId") != null && !int.TryParse(paymentData.SelectToken("PaymentGatewayId").ToString(), out paymentGatewayId))
                GenerateErrorResponse(400, string.Format("PaymentGatewayId must be a number"));
            if (paymentData.SelectToken("PaymentAmount") != null && !float.TryParse(paymentData.SelectToken("PaymentAmount").ToString(), out paymentAmount))
                GenerateErrorResponse(400, string.Format("Payment Amount must be a Float"));
            if (paymentData.SelectToken("BankAccountId") != null && !int.TryParse(paymentData.SelectToken("BankAccountId").ToString(), out bankAccountId))
                GenerateErrorResponse(400, string.Format("bankAccountId Amount must be a number"));
            if (paymentData.SelectToken("DepositeDate") != null && !DateTime.TryParse(paymentData.SelectToken("DepositeDate").ToString(), out depositeDate))
                GenerateErrorResponse(400, string.Format("FromDateTime is not a valid datetime"));
            if (paymentData.SelectToken("ActivatePercentage") != null && !int.TryParse(paymentData.SelectToken("ActivatePercentage").ToString(), out activatePercentage))
                GenerateErrorResponse(400, string.Format("Activate percentage must be a number"));
            if (paymentData.SelectToken("IsTDSApplicable") != null && !bool.TryParse(paymentData.SelectToken("IsTDSApplicable").ToString(), out isTDSApplicable))
                GenerateErrorResponse(400, string.Format("IsTDSApplicable percentage must be a boolean"));
            if (paymentData.SelectToken("TDSPercentage") != null && !int.TryParse(paymentData.SelectToken("TDSPercentage").ToString(), out tdsPercentage))
                GenerateErrorResponse(400, string.Format("TDSPercentage percentage must be a number"));
            if (paymentData.SelectToken("Comments") != null)
                comments = paymentData.SelectToken("Comments").ToString();
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GeneratePayment(productId: productId, accountId: accountId, employeeId: employeeId, invoiceId: invoiceId, billingModeId: billingModeId, paymentGatewayId: paymentGatewayId, paymentAmount: paymentAmount, bankAccountId: bankAccountId, depositeDate: depositeDate, activatePercentage: activatePercentage, comments: comments,isTDSApplicable:isTDSApplicable,tdsPercentage:tdsPercentage));
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