using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Data;

namespace OrdersManagement
{
    internal class Helper
    {
        private ResponseFormat _responseFormat = ResponseFormat.JSON;
        private Core.Client _client = null;
        private JObject _jObj = null;
        private JArray _jArr = null;
        private XmlDocument xmlDoc = null;
        private XmlElement rootElement = null;
        internal Helper(Core.Client client)
        {
            this._client = client;
            this.InitializeResponseVariables();
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
                _jObj = new JObject();
                _jArr = new JArray();
            }
        }
        private void CreateProperty(string key, object value, ref JObject json)
        {
            bool tempBoolean = false;
            if (bool.TryParse(value.ToString(), out tempBoolean))
                json.Add(new JProperty(key, tempBoolean));
            else
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
            bool tempBoolean = false;
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
                    if (bool.TryParse(value.ToString(), out tempBoolean))
                        _jObj.AddFirst(new JProperty(key, tempBoolean));
                    else
                        _jObj.AddFirst(new JProperty(key, value));
                }
                else
                {
                    if (bool.TryParse(value.ToString(), out tempBoolean))
                        _jObj.Add(new JProperty(key, tempBoolean));
                    else
                        _jObj.Add(new JProperty(key, value));
                }
            }
        }
        internal JObject GetServices(bool includeServiceProperties = false, bool isOnlyActive = true)
        {
            try
            {
                foreach (KeyValuePair<string, Model.Service> serviceElement in this._client.Services)
                {
                    JObject serviceObject = new JObject();
                    this.CreateProperty(Label.ID, serviceElement.Value.Id, ref serviceObject);
                    this.CreateProperty(Label.DISPLAY_NAME, serviceElement.Value.DisplayName, ref serviceObject);
                    this.CreateProperty(Label.META_DATA_CODE, serviceElement.Value.MetaDataCode, ref serviceObject);
                    this.CreateProperty(Label.ARE_MULTIPLE_ENTRIES_ALLOWED, serviceElement.Value.AreMultipleAllowed, ref serviceObject);
                    this.CreateProperty(Label.IS_ACTIVE, serviceElement.Value.IsActive, ref serviceObject);
                    JArray servicePropertiesArray = new JArray();
                    foreach (KeyValuePair<string, Model.ServiceProperty> servicePropertyElement in serviceElement.Value.Properties)
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
                    this.CreateProperty(Label.PROPERTIES, servicePropertiesArray, ref serviceObject);
                    this._jArr.Add(serviceObject);
                }
                this.CreateProperty(Label.SUCCESS, true);
                this.CreateProperty(Label.MESSAGE, "Action Completed");
                this.CreateProperty(Label.SERVICES, this._jArr);
            }
            catch (Exception e)
            {
                this.InitializeResponseVariables();
                this.CreateProperty("Success", false);
                this.CreateProperty("Message", e.Message);
            }
            return this.GetResponse();
        }
        public JObject GetResponse()
        {
            //if (this.IsOutputXmlFormat)
            //{
            //    return xmlDoc.InnerXml;
            //}
            //else
            //{
                return _jObj;
            //}
        }

        /// <summary>
        /// Converts the data set to json object.
        /// </summary>
        /// <param name="ds">The DataSet.</param>
        /// <returns></returns>        
        public void ParseDataSet(DataSet ds, Dictionary<string, Model.TablePreferences> tablePreferences = null, bool constructAsArrayForOneRow = true)
        {
            if (ds == null)
            {
                return;
            }
            try
            {
                Model.TablePreferences currentTablePreferences = null;
                string childXmlElementNameForRows = "";
                bool columnValuesAsAttributes = true;
                foreach (DataTable table in ds.Tables)
                {
                    currentTablePreferences = null;
                    childXmlElementNameForRows = "";
                    columnValuesAsAttributes = true;
                    if (tablePreferences != null && tablePreferences.ContainsKey(table.TableName))
                    {
                        tablePreferences.TryGetValue(table.TableName, out currentTablePreferences);
                        if (currentTablePreferences != null)
                        {
                            childXmlElementNameForRows = currentTablePreferences.ChildXmlElementNameForRows == null ? string.Empty : currentTablePreferences.ChildXmlElementNameForRows;
                            columnValuesAsAttributes = currentTablePreferences.ColumnValuesAsAttributes;
                        }
                    }
                    if (table.TableName.Equals(Label.OUTPUT_PARAMETERS))
                    {
                        foreach (DataColumn column in table.Columns)
                            this.CreateProperty(column.ColumnName, table.Rows[0][column.ColumnName], true);
                    }
                    else
                    {
                        this._jArr = new JArray();
                        JObject rowJObj = new JObject();
                        XmlElement tableRootElement = null;
                        XmlElement tableRowElement = null;
                        XmlElement columnElement = null;
                        string columnValue = "";
                        if (this.IsOutputXmlFormat)
                        {
                            tableRootElement = xmlDoc.CreateElement(table.TableName);
                        }
                        if (!constructAsArrayForOneRow && table.Rows.Count <= 1)
                        {
                            rowJObj = new JObject();
                            if (table.Rows.Count > 0)
                            {
                                foreach (DataColumn column in table.Columns)
                                {
                                    if (table.Rows[0][column.ColumnName] is DBNull || table.Rows[0][column.ColumnName] == null)
                                    {
                                        columnValue = "";
                                    }
                                    else
                                    {
                                        columnValue = table.Rows[0][column.ColumnName].ToString();
                                    }
                                    //if (tableRootElement != null && childElementNameForRows.Length > 0) {
                                    //    tableRowElement = xmlDoc.CreateElement(childElementNameForRows);
                                    //}
                                    //else if (tableRowElement != null) {
                                    //    tableRowElement = xmlDoc.CreateElement(table.TableName);
                                    //}
                                    if (this.IsOutputXmlFormat)
                                    {
                                        if (columnValuesAsAttributes)
                                        {
                                            //tableRowElement.SetAttribute(column.ColumnName, columnValue);
                                            tableRootElement.SetAttribute(column.ColumnName, columnValue);
                                        }
                                        else
                                        {
                                            columnElement = xmlDoc.CreateElement(column.ColumnName);
                                            columnElement.InnerText = columnValue;
                                            //tableRowElement.AppendChild(columnElement);
                                            tableRootElement.AppendChild(columnElement);
                                        }
                                    }
                                    else
                                    {
                                        rowJObj.Add(new JProperty(column.ColumnName, columnValue));
                                    }
                                }
                            }
                            if (this.IsOutputXmlFormat)
                            {
                                rootElement.AppendChild(tableRootElement);
                            }
                            else
                            {
                                this._jObj.Add(new JProperty(table.TableName, rowJObj));
                            }
                        }
                        else
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                rowJObj = new JObject();
                                if (tableRootElement != null && childXmlElementNameForRows.Length > 0)
                                {
                                    tableRowElement = xmlDoc.CreateElement(childXmlElementNameForRows);
                                }
                                else if (tableRootElement != null)
                                {
                                    tableRowElement = xmlDoc.CreateElement(table.TableName);
                                }
                                foreach (DataColumn column in table.Columns)
                                {
                                    if (row[column.ColumnName] is DBNull || row[column.ColumnName] == null)
                                    {
                                        columnValue = "";
                                    }
                                    else
                                    {
                                        columnValue = row[column.ColumnName].ToString();
                                    }
                                    if (this.IsOutputXmlFormat)
                                    {
                                        if (columnValuesAsAttributes)
                                        {
                                            tableRowElement.SetAttribute(column.ColumnName, columnValue);
                                        }
                                        else
                                        {
                                            columnElement = xmlDoc.CreateElement(column.ColumnName);
                                            columnElement.InnerText = columnValue;
                                            tableRowElement.AppendChild(columnElement);
                                        }
                                    }
                                    else
                                    {
                                        rowJObj.Add(new JProperty(column.ColumnName, columnValue));
                                    }
                                }
                                if (this.IsOutputXmlFormat)
                                {
                                    tableRootElement.AppendChild(tableRowElement);
                                }
                                else
                                {
                                    this._jArr.Add(rowJObj);
                                }
                            }
                            if (this.IsOutputXmlFormat)
                            {
                                rootElement.AppendChild(tableRootElement);
                            }
                            else
                            {
                                this._jObj.Add(new JProperty(table.TableName, this._jArr));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Utilities.Logger.Error(ex.ToString());
            }
            finally { }
        }

        //internal ResponseFormat ResponseFormat { get { return this._responseFormat; } set { this._responseFormat = value; } }
        private bool IsOutputXmlFormat { get { return this._responseFormat.Equals(ResponseFormat.XML) ? true : false; } }
    }
}
