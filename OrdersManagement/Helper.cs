using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using OrdersManagement.Model;
using OrdersManagement.Exceptions;

namespace OrdersManagement
{
    internal class Helper
    {
        #region PRIVATE VARIABLES

        private string _connectionString = string.Empty;
        private SqlConnection _sqlConnection = null;
        private SqlCommand _sqlCommand = null;
        private SqlDataAdapter _da = null;
        private DataSet _ds = null;
        private ResponseFormat _responseFormat = ResponseFormat.JSON;
        private Core.Client _client = null;
        private JObject _jObj = null;
        private JArray _jArr = null;
        private XmlDocument xmlDoc = null;
        private XmlElement rootElement = null;
        private bool _isOutputXmlFormat = false;

        #endregion

        #region CONSTRUCTOR

        internal Helper(Core.Client client, ResponseFormat responseFormat)
        {
            this._client = client;
            this._responseFormat = responseFormat;
            this.InitializeResponseVariables();
            this._connectionString = this._client.ConnectionString;
            this._sqlConnection = new SqlConnection(this._connectionString);
        }

        #endregion

        #region PRIVATE METHODS

        private void InitializeResponseVariables()
        {
            if (this._responseFormat.Equals(ResponseFormat.XML))
            {   
                xmlDoc = new XmlDocument();
                rootElement = xmlDoc.CreateElement("Response");
                xmlDoc.AppendChild(rootElement);
                this._isOutputXmlFormat = true;
            }
            else
            {
                _jObj = new JObject();
                _jArr = new JArray();
            }
        }        

        #endregion

        #region INTERNAL METHODS
        internal void LoadServiceRelatedStaticData()
        {
            this.LoadInputTypes();
            this.LoadInputDataTypes();
        }

        internal dynamic GetResponse()
        {
            if (this._isOutputXmlFormat)
            {
                return xmlDoc.InnerXml;
            }
            else
            {
                return _jObj;
            }
        }

        /// <summary>
        /// Converts the data set to JSon/Xml object.
        /// </summary>
        /// <param name="ds">The DataSet.</param>
        /// <returns></returns>        
        internal void ParseDataSet(DataSet ds, Dictionary<string, Model.TablePreferences> tablePreferences = null)
        {
            if (ds == null)
            {
                return;
            }
            try
            {
                Model.TablePreferences currentTablePreferences = null;
                string childXmlElementNameForRows = "";
                bool columnValuesAsXmlAttributes = true;
                bool singleRowAsSingleEntity = true;
                string currentTableRootName = string.Empty;
                foreach (DataTable table in ds.Tables)
                {
                    currentTablePreferences = null;
                    currentTableRootName = string.Empty;
                    childXmlElementNameForRows = "";
                    columnValuesAsXmlAttributes = true;
                    singleRowAsSingleEntity = true;                    
                    if (tablePreferences != null && tablePreferences.ContainsKey(table.TableName))
                    {
                        tablePreferences.TryGetValue(table.TableName, out currentTablePreferences);
                        if (currentTablePreferences != null)
                        {
                            currentTableRootName = currentTablePreferences.RootName == null ? string.Empty : currentTablePreferences.RootName;
                            childXmlElementNameForRows = currentTablePreferences.ChildElementNameForRows == null ? string.Empty : currentTablePreferences.ChildElementNameForRows;
                            columnValuesAsXmlAttributes = currentTablePreferences.ColumnValuesAsXmlAttributes;
                            singleRowAsSingleEntity = currentTablePreferences.SingleRowAsSingleEntity;
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
                        if (this._isOutputXmlFormat)
                        {
                            tableRootElement = xmlDoc.CreateElement(currentTableRootName.Length > 0 ? currentTableRootName : table.TableName);
                        }
                        if (singleRowAsSingleEntity && table.Rows.Count == 1)
                        {
                            if(this._isOutputXmlFormat)
                                tableRootElement = xmlDoc.CreateElement(childXmlElementNameForRows.Length > 0 ?
                                    childXmlElementNameForRows : currentTableRootName.Length > 0 ? currentTableRootName : table.TableName);
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
                                    if (this._isOutputXmlFormat)
                                    {
                                        if (columnValuesAsXmlAttributes)
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
                                        //rowJObj.Add(new JProperty(column.ColumnName, columnValue));
                                        bool boolResult = false;
                                        int intResult = 0;
                                        float floatResult = 0;
                                        if(column.ColumnName.Equals(Label.SUCCESS, StringComparison.CurrentCultureIgnoreCase) || bool.TryParse(columnValue, out boolResult))
                                            rowJObj.Add(new JProperty(column.ColumnName, boolResult));
                                        else if (int.TryParse(columnValue, out intResult))
                                            rowJObj.Add(new JProperty(column.ColumnName, intResult));
                                        else if(float.TryParse(columnValue, out floatResult))
                                            rowJObj.Add(new JProperty(column.ColumnName, floatResult));
                                        else
                                            rowJObj.Add(new JProperty(column.ColumnName, columnValue));
                                    }
                                }
                            }
                            if (this._isOutputXmlFormat)
                            {
                                rootElement.AppendChild(tableRootElement);
                            }
                            else
                            {
                                this._jObj.Add(new JProperty(childXmlElementNameForRows.Length > 0 ? 
                                                                    childXmlElementNameForRows : 
                                                                        currentTableRootName.Length > 0 ? 
                                                                                currentTableRootName : table.TableName, 
                                                                               rowJObj));
                                //this._jObj.Add(new JProperty(currentTableRootName.Length > 0 ? currentTableRootName : table.TableName, rowJObj));
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
                                    tableRowElement = xmlDoc.CreateElement(currentTableRootName.Length > 0 ? currentTableRootName : table.TableName);
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
                                    if (this._isOutputXmlFormat)
                                    {
                                        if (columnValuesAsXmlAttributes)
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
                                        bool boolResult = false;
                                        int intResult = 0;
                                        float floatResult = 0;
                                        if(column.ColumnName.Equals(Label.SUCCESS, StringComparison.CurrentCultureIgnoreCase) || bool.TryParse(columnValue, out boolResult))
                                            rowJObj.Add(new JProperty(column.ColumnName, boolResult));
                                        else if (int.TryParse(columnValue, out intResult))
                                            rowJObj.Add(new JProperty(column.ColumnName, intResult));
                                        else if(float.TryParse(columnValue, out floatResult))
                                            rowJObj.Add(new JProperty(column.ColumnName, floatResult));
                                        else
                                            rowJObj.Add(new JProperty(column.ColumnName, columnValue));
                                    }
                                }
                                if (this._isOutputXmlFormat)
                                {
                                    tableRootElement.AppendChild(tableRowElement);
                                }
                                else
                                {
                                    this._jArr.Add(rowJObj);
                                }
                            }
                            if (this._isOutputXmlFormat)
                            {
                                rootElement.AppendChild(tableRootElement);
                            }
                            else
                            {
                                this._jObj.Add(new JProperty(currentTableRootName.Length > 0 ? currentTableRootName : table.TableName, this._jArr));
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
        internal DataTable ConvertOutputParametersToDataTable(System.Data.SqlClient.SqlParameterCollection parameters)
        {
            DataTable outputParameters = new DataTable(Label.OUTPUT_PARAMETERS);
            for (int iterator = 0; iterator <= parameters.Count - 1; iterator++)
            {
                if (parameters[iterator].Direction.Equals(ParameterDirection.Output))
                {
                    outputParameters.Columns.Add(parameters[iterator].ParameterName.Replace("@", ""));
                }
            }
            DataRow row = outputParameters.NewRow();
            foreach (DataColumn column in outputParameters.Columns)
            {
                row[column.ColumnName] = parameters["@" + column.ColumnName].Value;
            }
            outputParameters.Rows.Add(row);
            return outputParameters;
        }
        internal void CreateProperty(string key, object value, bool isInsertFirst = false)
        {
            bool boolResult = false;
            int intResult = 0;
            float floatResult = 0;
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
                    if (bool.TryParse(value.ToString(), out boolResult))
                        _jObj.AddFirst(new JProperty(key, boolResult));
                    else if (int.TryParse(value.ToString(), out intResult))
                        _jObj.AddFirst(new JProperty(key, intResult));
                    else if (float.TryParse(value.ToString(), out floatResult))
                        _jObj.AddFirst(new JProperty(key, floatResult));
                    else
                        _jObj.AddFirst(new JProperty(key, value));
                }
                else
                {
                    if (bool.TryParse(value.ToString(), out boolResult))
                        _jObj.Add(new JProperty(key, boolResult));
                    else if (int.TryParse(value.ToString(), out intResult))
                        _jObj.Add(new JProperty(key, intResult));
                    else if (float.TryParse(value.ToString(), out floatResult))
                        _jObj.Add(new JProperty(key, floatResult));
                    else
                        _jObj.Add(new JProperty(key, value));
                }
            }
        }        
        internal void CreateProperty(string key, object value, ref JObject json, bool isInsertFirst = false)
        {
            bool boolResult = false;
            int intResult = 0;
            float floatResult = 0;
            if (isInsertFirst)
            {
                if (bool.TryParse(value.ToString(), out boolResult))
                    json.AddFirst(new JProperty(key, boolResult));
                else if (int.TryParse(value.ToString(), out intResult))
                    json.AddFirst(new JProperty(key, intResult));
                else if (float.TryParse(value.ToString(), out floatResult))
                    json.AddFirst(new JProperty(key, floatResult));
                else
                    json.AddFirst(new JProperty(key, value));
            }
            else
            {
                if (bool.TryParse(value.ToString(), out boolResult))
                    json.Add(new JProperty(key, boolResult));
                else if (int.TryParse(value.ToString(), out intResult))
                    json.Add(new JProperty(key, intResult));
                else if (float.TryParse(value.ToString(), out floatResult))
                    json.Add(new JProperty(key, floatResult));
                else
                    json.Add(new JProperty(key, value));
            }            
        }
        internal void CreateProperty(string key, object value, ref XmlElement rootElement, ref XmlDocument xmlDoc)
        {
            XmlElement tempElement = xmlDoc.CreateElement(key);
            tempElement.InnerText = value.ToString();
            rootElement.AppendChild(tempElement);
        }
        internal void PopulateCommonOutputParameters(ref System.Data.SqlClient.SqlCommand sqlCommand)
        {
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.Add(ProcedureParameter.SUCCESS, System.Data.SqlDbType.Bit).Direction = System.Data.ParameterDirection.Output;
            sqlCommand.Parameters.Add(ProcedureParameter.MESSAGE, System.Data.SqlDbType.VarChar, 1000).Direction = System.Data.ParameterDirection.Output;
        }        
        internal void ResetResponseVariables()
        {
            this.InitializeResponseVariables();
        }

        #endregion

        #region PRIVATE METHODS

        private void LoadInputTypes(bool isOnlyActive = true, bool forceLoad = false)
        {
            if (SharedClass.InputTypesLoaded && !forceLoad)
                return;
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_INPUT_TYPES, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = isOnlyActive;
                this.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter();
                this._da.SelectCommand = this._sqlCommand;
                this._ds = new DataSet();
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    throw new OrdersManagement.Exceptions.ClientInitializationException(string.Format("Procedure {0} Returned False. {0}", StoredProcedure.GET_INPUT_TYPES, this._sqlCommand.GetMessage()));
                if (this._ds.Tables.Count > 0 && this._ds.Tables[0].Rows.Count > 0)
                {
                    SharedClass.InputTypes.Clear();
                    foreach (DataRow inputTypeRow in _ds.Tables[0].Rows)
                    {
                        if (inputTypeRow[Label.TYPE].ToString().Equals(Label.TEXT_BOX, StringComparison.CurrentCultureIgnoreCase))
                            SharedClass.InputTypes.Add(Convert.ToByte(inputTypeRow[Label.ID]), Model.PropertyInputType.TEXT_BOX);
                        else if (inputTypeRow[Label.TYPE].ToString().Equals(Label.TEXT_AREA, StringComparison.CurrentCultureIgnoreCase))
                            SharedClass.InputTypes.Add(Convert.ToByte(inputTypeRow[Label.ID]), PropertyInputType.TEXT_AREA);
                        else
                            throw new ClientInitializationException(string.Format("Input Type is not defined for {0} in Orders Library.", inputTypeRow[Label.TYPE]));
                    }
                }
                else
                    throw new ClientInitializationException("No Input Types found.");
            }
            catch (SqlException e)
            {
                throw new ClientInitializationException(string.Format("Could not load Input Types from database. Reason : {0}", e.Message));
            }
            finally
            {
                this._ds = null;
                this._da = null;
            }
        }
        private void LoadInputDataTypes(bool isOnlyActive = true, bool forceLoad = false)
        {
            if (SharedClass.InputDataTypesLoaded && !forceLoad)
                return;
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_INPUT_DATA_TYPES, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = isOnlyActive;
                this.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter();
                this._da.SelectCommand = this._sqlCommand;
                this._ds = new DataSet();
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    throw new ClientInitializationException(string.Format("Procedure Returned False. {0}", this._sqlCommand.GetMessage()));
                if (this._ds.Tables.Count > 0 && this._ds.Tables[0].Rows.Count > 0)
                {
                    SharedClass.InputDataTypes.Clear();
                    foreach (DataRow inputDataTypeRow in _ds.Tables[0].Rows)
                    {
                        if (inputDataTypeRow[Label.DATA_TYPE].ToString().Equals(Label.INT, StringComparison.CurrentCultureIgnoreCase))
                            SharedClass.InputDataTypes.Add(Convert.ToByte(inputDataTypeRow[Label.ID]), PropertyDataType.INT);
                        else if (inputDataTypeRow[Label.DATA_TYPE].ToString().Equals(Label.FLOAT, StringComparison.CurrentCultureIgnoreCase))
                            SharedClass.InputDataTypes.Add(Convert.ToByte(inputDataTypeRow[Label.ID]), PropertyDataType.FLOAT);
                        else if (inputDataTypeRow[Label.DATA_TYPE].ToString().Equals(Label.STRING, StringComparison.CurrentCultureIgnoreCase))
                            SharedClass.InputDataTypes.Add(Convert.ToByte(inputDataTypeRow[Label.ID]), PropertyDataType.STRING);
                        else if (inputDataTypeRow[Label.DATA_TYPE].ToString().Equals(Label.DATE_TIME, StringComparison.CurrentCultureIgnoreCase))
                            SharedClass.InputDataTypes.Add(Convert.ToByte(inputDataTypeRow[Label.ID]), PropertyDataType.DATETIME);
                        else
                            throw new ClientInitializationException(string.Format("Input Data Type is not defined for {0} in Orders Library.", inputDataTypeRow[Label.DATA_TYPE]));
                    }
                }
                else
                    throw new ClientInitializationException("No Input Data Types found.");
            }
            catch (SqlException e)
            {
                throw new ClientInitializationException(string.Format("Could not load Input Data Types from database. Reason : {0}", e.Message));
            }
            finally
            {
                this._ds = null;
                this._da = null;
            }
        }

        #endregion

        #region INTERNAL PROPERTIES
        internal string ConnectionString { get { return this._connectionString; } set { this._connectionString = value; } }
        internal bool IsOutputXmlFormat { get { return this._isOutputXmlFormat; } }

        #endregion
    }
}
