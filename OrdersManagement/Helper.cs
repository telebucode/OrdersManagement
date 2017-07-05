using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace OrdersManagement
{
    internal class Helper
    {
        private ResponseFormat _responseFormat = ResponseFormat.JSON;
        private Core.Client _client = null;
        private JObject jObj = null;
        private JArray jArr = null;
        private XmlDocument xmlDoc = null;
        private XmlElement rootElement = null;
        internal Helper(Core.Client client)
        {
            this._client = client;

        }
        private void InitializeResponseVariables()
        {
            if (this._responseFormat.Equals(ResponseFormat.XML))
            {
                xmlDoc = new XmlDocument();
                rootElement = xmlDoc.CreateElement("Response");
                xmlDoc.AppendChild(rootElement);
            }
            else
            {
                jObj = new JObject();
                jArr = new JArray();
            }
        }
        private void CreateProperty(string key, object value, ref JObject json)
        {
            json.Add(new JProperty(key, value));
        }
        private void CreateProperty(string key, object value, ref XmlElement rootElement, ref XmlDocument xmlDoc)
        {
            XmlElement tempElement = xmlDoc.CreateElement(key);
            tempElement.InnerText = value.ToString();
            rootElement.AppendChild(tempElement);
        }
        internal void PopulateOutputParameters(ref System.Data.SqlClient.SqlCommand sqlCommand)
        {
            sqlCommand.Parameters.Add(ProcedureParameter.SUCCESS, System.Data.SqlDbType.Bit).Direction = System.Data.ParameterDirection.Output;
            sqlCommand.Parameters.Add(ProcedureParameter.MESSAGE, System.Data.SqlDbType.VarChar, 1000).Direction = System.Data.ParameterDirection.Output;
        }
        private void CreateProperty(string key, object value, bool isInsertFirst = false)
        {
            if (this._responseFormat.Equals(ResponseFormat.XML))
            {
                XmlElement tempElement = xmlDoc.CreateElement(key);
                tempElement.InnerText = value.ToString();
                if (isInsertFirst)
                {
                    rootElement.PrependChild(tempElement);
                }
                else
                {
                    rootElement.AppendChild(tempElement);
                }
            }
            else
            {
                if (isInsertFirst)
                {
                    jObj.AddFirst(new JProperty(key, value));
                }
                else
                {
                    jObj.Add(new JProperty(key, value));
                }
            }
        }
        internal dynamic GetServices(bool includeServiceProperties = false, bool isOnlyActive = true)
        {
            dynamic response = null;
            if(this._responseFormat.Equals(ResponseFormat.JSON))
            {
                foreach(KeyValuePair<string, Model.Service> serviceElement in this._client.Services)
                {
                    JObject serviceObject = new JObject();
                    this.CreateProperty(Label.ID, serviceElement.Value.Id, ref serviceObject);
                    this.CreateProperty(Label.DISPLAY_NAME, serviceElement.Value.DisplayName, ref serviceObject);
                    this.CreateProperty(Label.META_DATA_CODE, serviceElement.Value.MetaDataCode, ref serviceObject);
                    this.CreateProperty(Label.ARE_MULTIPLE_ENTRIES_ALLOWED, serviceElement.Value.AreMultipleAllowed, ref serviceObject);
                    this.CreateProperty(Label.IS_ACTIVE, serviceElement.Value.IsActive, ref serviceObject);
                    JArray servicePropertiesArray = new JArray();
                    foreach(KeyValuePair<string, Model.ServiceProperty> servicePropertyElement in serviceElement.Value.Properties)
                    {
                        JObject servicePropertyObject = new JObject();
                        this.CreateProperty(Label.ID, servicePropertyElement.Value.Id, ref servicePropertyObject);
                        this.CreateProperty(Label.DISPLAY_NAME, servicePropertyElement.Value.DisplayName, ref servicePropertyObject);
                        this.CreateProperty(Label.META_DATA_CODE, servicePropertyElement.Value.MetaDataCode, ref servicePropertyObject);
                        this.CreateProperty(Label.IS_REQUIRED, servicePropertyElement.Value.IsRequired, ref servicePropertyObject);
                        this.CreateProperty(Label.IS_ACTIVE, servicePropertyElement.Value.IsActive, ref servicePropertyObject);
                        this.CreateProperty(Label.DEFAULT_VALUE, servicePropertyElement.Value.DefaultValue, ref servicePropertyObject);
                        this.CreateProperty(Label.INCLUDE_IN_ORDER_AMOUNT, servicePropertyElement.Value.IncludeInOrderAmount, ref servicePropertyObject);
                        servicePropertiesArray.Add(servicePropertyObject);
                    }
                }
            }
            return response;
        }
        internal ResponseFormat ResponseFormat { get { return this._responseFormat; } set { this._responseFormat = value; } }
    }
}
