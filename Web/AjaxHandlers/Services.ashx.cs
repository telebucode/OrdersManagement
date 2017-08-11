using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OrdersManagement;
using OrdersManagement.Model;
using OrdersManagement.Core;

namespace Web.AjaxHandlers
{
    /// <summary>
    /// Summary description for Services
    /// </summary>
    public class Services : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if(context.Request["Action"] == null)
            {
                context.Response.StatusCode = 400;
                context.Response.End();
            }
            switch(context.Request["Action"].ToString())
            {
                case "GetServices":
                    Client client = new Client(responseFormat: ResponseFormat.JSON);
                    context.Response.Write(client.GetServices(0, true, true));
                    break;
                default:
                    context.Response.StatusCode = 400;
                    context.Response.End();
                    break;
            }
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