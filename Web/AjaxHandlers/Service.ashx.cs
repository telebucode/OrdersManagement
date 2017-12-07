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
    /// Summary description for Services
    /// </summary>
    public class Service : IHttpHandler
    {
        private JObject errorJSon = new JObject(new JProperty("Success", false), new JProperty("Message", ""));

        public void ProcessRequest(HttpContext context)
        {
            if(context.Request["Action"] == null)
            {
                context.Response.StatusCode = 400;
                context.Response.End();
            }
            try
            {
                switch (context.Request["Action"].ToString())
                {
                    case "GetServices":
                        GetServices(context);
                        break;
                    case "CreateService":
                        CreateService(context);
                        break;
                    case "UpdateService":
                        UpdateService(context);
                        break;
                    case "DeleteService":
                        DeleteService(context);
                        break;
                    case "CreateServiceProperties":
                        CreateerviceProperties(context);
                        break;
                    default:
                        GenerateErrorResponse(400, string.Format("Invalid Action ({0})", context.Request["Action"].ToString()));
                        break;
                }
            }
            catch(System.Threading.ThreadAbortException e)
            { }
            catch(OrdersManagement.Exceptions.ServiceException e)
            {
                GenerateErrorResponse(500, e.Message);
            }
            catch(Exception e)
            {
                GenerateErrorResponse(500, e.Message);
            }
        }
        private void GetServices(HttpContext context)
        {
            byte serviceId = 0;            
            bool onlyActive = true;
            bool includeServiceProperties = true;            
            if(context.Request["ServiceId"] != null && !byte.TryParse(context.Request["ServiceId"].ToString(), out serviceId))
                GenerateErrorResponse(400, string.Format("ServiceId value ({0}) is not a valid number", context.Request["ServiceId"].ToString()));
            if (context.Request["OnlyActive"] != null && !bool.TryParse(context.Request["OnlyActive"].ToString(), out onlyActive))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            if (context.Request["IncludeServiceProperties"] != null && !bool.TryParse(context.Request["IncludeServiceProperties"].ToString(), out includeServiceProperties))
                GenerateErrorResponse(400, string.Format("IncludeServiceProperties value ({0}) is not a valid boolean value", context.Request["IncludeServiceProperties"].ToString()));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetServices(serviceId: serviceId, includeServiceProperties: includeServiceProperties, onlyActive: onlyActive));
        }
        private void CreateService(HttpContext context)
        {
            bool areMultipleEntriesAllowed = false;
            if (context.Request["DisplayName"] == null || context.Request["DisplayName"].ToString().Trim().Length == 0)
                GenerateErrorResponse(400, string.Format("DisplayName Is Mandatory"));
            if (context.Request["MetaDataCode"] == null || context.Request["MetaDataCode"].ToString().Trim().Length == 0)
                GenerateErrorResponse(400, string.Format("MetaDataCode Is Mandatory"));
            if (context.Request["AreMultipleEntriesAllowed"] == null || !bool.TryParse(context.Request["AreMultipleEntriesAllowed"].ToString(), out areMultipleEntriesAllowed))
                GenerateErrorResponse(400, string.Format("Parameter AreMultipleEntriesAllowed is missing or not a valid boolean value"));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.CreateService(displayName: context.Request["DisplayName"].ToString(),
                                                        metaDataCode: context.Request["MetaDataCode"].ToString(),
                                                        areMultipleEntriesAllowed: bool.Parse(context.Request["AreMultipleEntriesAllowed"])));
        }
        private void UpdateService(HttpContext context)
        {
            bool areMultipleEntriesAllowed = false;
            byte serviceId = 0;
            if (context.Request["ServiceId"] == null || !byte.TryParse(context.Request["ServiceId"].ToString(), out serviceId))
                GenerateErrorResponse(400, string.Format("Parameter ServiceId is missing or not a valid number"));
            if (context.Request["DisplayName"] == null || context.Request["DisplayName"].ToString().Trim().Length == 0)
                GenerateErrorResponse(400, string.Format("DisplayName Is Mandatory"));
            if (context.Request["MetaDataCode"] == null || context.Request["MetaDataCode"].ToString().Trim().Length == 0)
                GenerateErrorResponse(400, string.Format("MetaDataCode Is Mandatory"));
            if (context.Request["AreMultipleEntriesAllowed"] == null || !bool.TryParse(context.Request["AreMultipleEntriesAllowed"].ToString(), out areMultipleEntriesAllowed))
                GenerateErrorResponse(400, string.Format("Parameter AreMultipleEntriesAllowed is missing or not a valid boolean value"));
            if (serviceId <= 0)
                GenerateErrorResponse(400, string.Format("ServiceId must be greater than 0"));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.UpdateService(serviceId: serviceId,
                displayName: context.Request["DisplayName"].ToString(), metaDataCode: context.Request["MetaDataCode"].ToString(),
                areMultipleEntriesAllowed: bool.Parse(context.Request["AreMultipleEntriesAllowed"].ToString())));
        }
        private void DeleteService(HttpContext context)
        {
            byte serviceId = 0;
            if (context.Request["ServiceId"] == null || !byte.TryParse(context.Request["ServiceId"].ToString(), out serviceId))
                GenerateErrorResponse(400, string.Format("Parameter ServiceId is missing or not a valid number"));
            if (serviceId <= 0)
                GenerateErrorResponse(400, string.Format("ServiceId must be greater than 0"));
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.DeleteService(serviceId));
        }
        private void CreateerviceProperties(HttpContext context)
        {
            byte serviceId = 0;
            JArray properiesArray = null;
            bool isRequired = true;
            bool includeInOrderAmount = false;
            byte inputTypeId = 0;
            byte dataTypeId = 0;
            List<ServiceProperty> servicePropertiesList = new List<ServiceProperty>();
            if(context.Request["ServiceId"] == null || !byte.TryParse(context.Request["ServiceId"].ToString(), out serviceId))
                GenerateErrorResponse(400, string.Format("Parameter ServiceId is missing or not a valid number"));
            try
            {
                properiesArray = JArray.Parse(context.Request["Properties"].ToString());
            }
            catch(Exception e)
            {   
                GenerateErrorResponse(400, string.Format("Invalid JSON"));
            }
            foreach(JObject propertyObject in properiesArray)
            {
                isRequired = true;
                ServiceProperty serviceProperty = new ServiceProperty();
                if (propertyObject.SelectToken("DisplayName") == null || propertyObject.SelectToken("DisplayName").ToString().Trim().Length == 0)
                    GenerateErrorResponse(400, string.Format("DisplayName is mandatory"));
                if (propertyObject.SelectToken("MetaDataCode") == null || propertyObject.SelectToken("MetaDataCode").ToString().Trim().Length == 0)
                    GenerateErrorResponse(400, string.Format("MetaDataCode is mandatory for Property {0}", propertyObject.SelectToken("DisplayName").ToString()));
                if (propertyObject.SelectToken("IsRequired") != null && !bool.TryParse(propertyObject.SelectToken("IsRequired").ToString(), out isRequired))
                    GenerateErrorResponse(400, string.Format("Parameter IsRequired should be a boolean value for Property {0}", propertyObject.SelectToken("DisplayName").ToString()));
                if (propertyObject.SelectToken("IncludeInOrderAmount") != null && !bool.TryParse(propertyObject.SelectToken("IncludeInOrderAmount").ToString(), out includeInOrderAmount))
                    GenerateErrorResponse(400, string.Format("Parameter IncludeInOrderAmount should be a boolean value for Property {0}", propertyObject.SelectToken("DisplayName").ToString()));
                if (propertyObject.SelectToken("InputTypeId") == null || !byte.TryParse(propertyObject.SelectToken("InputTypeId").ToString(), out inputTypeId))
                    GenerateErrorResponse(400, string.Format("Parameter InputTypeId of Property '{0}' is missing or invalid", propertyObject.SelectToken("DisplayName").ToString()));
                if (propertyObject.SelectToken("DataTypeId") == null || !byte.TryParse(propertyObject.SelectToken("DataTypeId").ToString(), out dataTypeId))
                    GenerateErrorResponse(400, string.Format("Parameter DataTypeId of Property '{0}' is missing or invalid", propertyObject.SelectToken("DisplayName").ToString()));
                if (serviceId <= 0)
                    GenerateErrorResponse(400, string.Format("ServiceId must be greater than 0"));
                serviceProperty.DisplayName = propertyObject.SelectToken("DisplayName").ToString();
                serviceProperty.MetaDataCode = propertyObject.SelectToken("MetaDataCode").ToString();
                serviceProperty.IsRequired = isRequired;
                serviceProperty.DefaultValue = propertyObject.SelectToken("DefaultValue") == null ? string.Empty : propertyObject.SelectToken("DefaultValue").ToString();
                serviceProperty.IncludeInOrderAmount = includeInOrderAmount;
                serviceProperty.InputTypeId = inputTypeId;
                serviceProperty.DataTypeId = dataTypeId;
                servicePropertiesList.Add(serviceProperty);
            }
            Client client = new Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.CreateServiceProperties(serviceId, servicePropertiesList));
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