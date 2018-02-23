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
            //if (context.Request["Action"] == null)
            //{
            //    context.Response.StatusCode = 400;
            //    context.Response.End();
            //}
            try
            {
                switch (context.Request.QueryString["Action"].ToString())
                {
                    case "GetServices":
                        GetServices(context);
                        break;
                    case "GetServiceProperties":
                        GetServiceProperties(context);
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
                        CreateServiceProperties(context);
                        break;
                    case "UpdateServiceProperties":
                        UpdateServiceProperties(context);
                        break;
                    default:
                        GenerateErrorResponse(400, string.Format("Invalid Action ({0})", context.Request["Action"].ToString()));
                        break;
                }
            }
            catch (System.Threading.ThreadAbortException e)
            { }
            catch (OrdersManagement.Exceptions.ServiceException e)
            {
                GenerateErrorResponse(500, e.Message);
            }
            catch (Exception e)
            {
                GenerateErrorResponse(500, e.Message);
            }
        }


        private void GetServices(HttpContext context)
        {
            short serviceId = 0;
            bool onlyActive = true;
            bool includeServiceProperties = true;
            byte productId = 0;
            if (context.Request["ProductId"] == null || !byte.TryParse(context.Request["ProductId"].ToString(), out productId))
                GenerateErrorResponse(400, string.Format("ProductId is mandatory", context.Request["ProductId"].ToString()));
            if (context.Request["ServiceId"] != null && !short.TryParse(context.Request["ServiceId"].ToString(), out serviceId))
                GenerateErrorResponse(400, string.Format("ServiceId value ({0}) is not a valid number", context.Request["ServiceId"].ToString()));
            if (context.Request["OnlyActive"] != null && !bool.TryParse(context.Request["OnlyActive"].ToString(), out onlyActive))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            if (context.Request["IncludeServiceProperties"] != null && !bool.TryParse(context.Request["IncludeServiceProperties"].ToString(), out includeServiceProperties))
                GenerateErrorResponse(400, string.Format("IncludeServiceProperties value ({0}) is not a valid boolean value", context.Request["IncludeServiceProperties"].ToString()));
            OrdersManagement.Core.Client client = new OrdersManagement.Core.Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetServices(productId: productId, serviceId: serviceId, includeServiceProperties: includeServiceProperties, onlyActive: onlyActive));
        }

        private void GetServiceProperties(HttpContext context)
        {
            short serviceId = 0;
            bool onlyActive = true;

            if (context.Request["ServiceId"] != null && !short.TryParse(context.Request["ServiceId"].ToString(), out serviceId))
                GenerateErrorResponse(400, string.Format("ServiceId value ({0}) is not a valid number", context.Request["ServiceId"].ToString()));
            if (context.Request["OnlyActive"] != null && !bool.TryParse(context.Request["OnlyActive"].ToString(), out onlyActive))
                GenerateErrorResponse(400, string.Format("OnlyActive value ({0}) is not a valid boolean value", context.Request["OnlyActive"].ToString()));
            OrdersManagement.Core.Client client = new OrdersManagement.Core.Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.GetServiceProperties(serviceId: serviceId, onlyActive: onlyActive, tablePreferences: null));
        }
        private void CreateService(HttpContext context)
        {
            bool areMultipleEntriesAllowed = false;
            byte productId = 0;
            if (context.Request["ProductId"] == null || byte.TryParse(context.Request["ProductId"], out productId))
                GenerateErrorResponse(400, string.Format("ProductId Is Mandatory"));
            if (context.Request["DisplayName"] == null || context.Request["DisplayName"].ToString().Trim().Length == 0)
                GenerateErrorResponse(400, string.Format("DisplayName Is Mandatory"));
            if (context.Request["MetaDataCode"] == null || context.Request["MetaDataCode"].ToString().Trim().Length == 0)
                GenerateErrorResponse(400, string.Format("MetaDataCode Is Mandatory"));
            if (context.Request["AreMultipleEntriesAllowed"] == null || !bool.TryParse(context.Request["AreMultipleEntriesAllowed"].ToString(), out areMultipleEntriesAllowed))
                GenerateErrorResponse(400, string.Format("Parameter AreMultipleEntriesAllowed is missing or not a valid boolean value"));
            OrdersManagement.Core.Client client = new OrdersManagement.Core.Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.CreateService(productId: Convert.ToByte(context.Request["ProductId"]), displayName: context.Request["DisplayName"].ToString(),
                                                        metaDataCode: context.Request["MetaDataCode"].ToString(),
                                                        areMultipleEntriesAllowed: bool.Parse(context.Request["AreMultipleEntriesAllowed"])));
        }
        private void UpdateService(HttpContext context)
        {
            bool areMultipleEntriesAllowed = false;
            Int16 serviceId = 0;
            if (context.Request["ServiceId"] == null || !Int16.TryParse(context.Request["ServiceId"].ToString(), out serviceId))
                GenerateErrorResponse(400, string.Format("Parameter ServiceId is missing or not a valid number"));
            if (context.Request["DisplayName"] == null || context.Request["DisplayName"].ToString().Trim().Length == 0)
                GenerateErrorResponse(400, string.Format("DisplayName Is Mandatory"));
            if (context.Request["MetaDataCode"] == null || context.Request["MetaDataCode"].ToString().Trim().Length == 0)
                GenerateErrorResponse(400, string.Format("MetaDataCode Is Mandatory"));
            if (context.Request["AreMultipleEntriesAllowed"] == null || !bool.TryParse(context.Request["AreMultipleEntriesAllowed"].ToString(), out areMultipleEntriesAllowed))
                GenerateErrorResponse(400, string.Format("Parameter AreMultipleEntriesAllowed is missing or not a valid boolean value"));
            if (serviceId <= 0)
                GenerateErrorResponse(400, string.Format("ServiceId must be greater than 0"));
            OrdersManagement.Core.Client client = new OrdersManagement.Core.Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.UpdateService(serviceId: serviceId,
                displayName: context.Request["DisplayName"].ToString(), metaDataCode: context.Request["MetaDataCode"].ToString(),
                areMultipleEntriesAllowed: bool.Parse(context.Request["AreMultipleEntriesAllowed"].ToString())));
        }
        private void DeleteService(HttpContext context)
        {
            Int16 serviceId = 0;
            if (context.Request["ServiceId"] == null || !Int16.TryParse(context.Request["ServiceId"].ToString(), out serviceId))
                GenerateErrorResponse(400, string.Format("Parameter ServiceId is missing or not a valid number"));
            if (serviceId <= 0)
                GenerateErrorResponse(400, string.Format("ServiceId must be greater than 0"));
            OrdersManagement.Core.Client client = new OrdersManagement.Core.Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.DeleteService(serviceId));
        }
        private void CreateServiceProperties(HttpContext context)
        {
            Int16 serviceId = 0;
            JArray properiesArray = null;
            JArray propertyFieldsArray = null;

            List<ServiceProperty> servicePropertiesList = new List<ServiceProperty>();
            List<ServicePropertyFields> servicePropertiesFieldList = new List<ServicePropertyFields>();
            if (context.Request["ServiceId"] == null || !Int16.TryParse(context.Request["ServiceId"].ToString(), out serviceId))
                GenerateErrorResponse(400, string.Format("Parameter ServiceId is missing or not a valid number"));
            try
            {
                properiesArray = JArray.Parse(context.Request["Properties"].ToString());

            }
            catch (Exception e)
            {
                GenerateErrorResponse(400, string.Format("Invalid JSON"));
            }
            foreach (JObject propertyObject in properiesArray)
            {
                try
                {
                    propertyFieldsArray = JArray.Parse(propertyObject.SelectToken("PropertyFields").ToString());
                }
                catch (Exception ex)
                {
                    GenerateErrorResponse(400, string.Format("Invalid JSON"));
                }

                ServiceProperty serviceProperty = ValidateServiceProperties(propertyObject);
                servicePropertiesFieldList = ValidateServicePropertyFields(propertyFieldsArray, serviceProperty);

                servicePropertiesList.Add(serviceProperty);
            }


            OrdersManagement.Core.Client client = new OrdersManagement.Core.Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.CreateServiceProperties(serviceId, servicePropertiesList, servicePropertiesFieldList));
        }

        private void UpdateServiceProperties(HttpContext context)
        {
            Int16 servicePropertId = 0;
            JObject propertyObject = null;
            JArray propertyFieldsArray = null;

            ServiceProperty servicePropert = new ServiceProperty();
            List<ServicePropertyFields> servicePropertiesFieldList = new List<ServicePropertyFields>();
            if (context.Request["ServicePropertyId"] == null || !Int16.TryParse(context.Request["ServicePropertyId"].ToString(), out servicePropertId))
                GenerateErrorResponse(400, string.Format("Parameter ServicePropertyId is missing or not a valid number"));
            try
            {
                propertyObject = JObject.Parse(context.Request["Property"].ToString());

            }
            catch (Exception e)
            {
                GenerateErrorResponse(400, string.Format("Invalid JSON"));
            }
            try
            {
                propertyFieldsArray = JArray.Parse(propertyObject.SelectToken("PropertyFields").ToString());
            }
            catch (Exception ex)
            {
                GenerateErrorResponse(400, string.Format("Invalid JSON"));
            }

            ServiceProperty serviceProperty = ValidateServiceProperties(propertyObject);
            serviceProperty.Id = servicePropertId;
            List<ServicePropertyFields> servicePropertFields = ValidateServicePropertyFields(propertyFieldsArray, serviceProperty);

            OrdersManagement.Core.Client client = new OrdersManagement.Core.Client(responseFormat: ResponseFormat.JSON);
            context.Response.Write(client.UpdateServiceProperty(serviceProperty, servicePropertiesFieldList));
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

        private ServiceProperty ValidateServiceProperties(JObject propertyObject)
        {


            bool isRequired = true;
            bool includeInOrderAmount = false;
            byte inputTypeId = 0;
            byte dataTypeId = 0;
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


            serviceProperty.DisplayName = propertyObject.SelectToken("DisplayName").ToString();
            serviceProperty.MetaDataCode = propertyObject.SelectToken("MetaDataCode").ToString();
            serviceProperty.IsRequired = isRequired;
            serviceProperty.DefaultValue = propertyObject.SelectToken("DefaultValue") == null ? string.Empty : propertyObject.SelectToken("DefaultValue").ToString();
            serviceProperty.IncludeInOrderAmount = includeInOrderAmount;
            serviceProperty.InputTypeId = inputTypeId;
            serviceProperty.DataTypeId = dataTypeId;


            return serviceProperty;
        }
        private List<ServicePropertyFields> ValidateServicePropertyFields(JArray propertyFieldsArray, ServiceProperty serviceProperty)
        {


            byte inputTypeId = 0;
            byte minLength = 0;
            Int16 maxlength = 0;
            string options = string.Empty;
            bool isAllowSpecialChars = false;

            List<ServicePropertyFields> servicePropertiesFieldList = new List<ServicePropertyFields>();
            foreach (JObject propertyFields in propertyFieldsArray)
            {
                ServicePropertyFields servicePropertyFields = new ServicePropertyFields();

                if (serviceProperty.DataTypeId == (int)PropertyDataType.STRING)
                {
                    if (propertyFields.SelectToken("MinLength") == null || !byte.TryParse(propertyFields.SelectToken("MinLength").ToString(), out minLength))
                        GenerateErrorResponse(400, string.Format("Parameter Minlength of Property '{0}' is missing or invalid", serviceProperty.DisplayName));
                    if (propertyFields.SelectToken("MaxLength") == null || !byte.TryParse(propertyFields.SelectToken("MaxLength").ToString(), out minLength))
                        GenerateErrorResponse(400, string.Format("Parameter MaxLength of Property '{0}' is missing or invalid", serviceProperty.DisplayName));
                }
                if (serviceProperty.InputTypeId == (int)PropertyInputType.TEXT_BOX || inputTypeId == (int)PropertyInputType.TEXT_AREA)
                    if (propertyFields.SelectToken("IsAllowSpecialChars") == null || !byte.TryParse(propertyFields.SelectToken("IsAllowSpecialChars").ToString(), out minLength))
                        GenerateErrorResponse(400, string.Format("Parameter IsAllowSpecialCharacters of Property '{0}' is missing or invalid", serviceProperty.DisplayName));

                if (serviceProperty.InputTypeId != (int)PropertyInputType.TEXT_BOX && inputTypeId == (int)PropertyInputType.TEXT_AREA)
                    if (propertyFields.SelectToken("Options") == null || !byte.TryParse(propertyFields.SelectToken("Options").ToString(), out minLength))
                        GenerateErrorResponse(400, string.Format("Parameter Options of Property '{0}' is missing or invalid", serviceProperty.DisplayName));

                servicePropertyFields.FieldId = serviceProperty.Id;
                servicePropertyFields.MetaDataCode = serviceProperty.MetaDataCode;
                servicePropertyFields.MinLength = minLength;
                servicePropertyFields.MaxLength = maxlength;
                servicePropertyFields.IsAllowSpecialChars = isAllowSpecialChars;
                servicePropertyFields.Options = options;
                servicePropertiesFieldList.Add(servicePropertyFields);

            }
            return servicePropertiesFieldList;
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