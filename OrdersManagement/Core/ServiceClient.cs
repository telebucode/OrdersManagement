using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using OrdersManagement.Model;
using OrdersManagement.Exceptions;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace OrdersManagement.Core
{
    internal class ServiceClient
    {
        #region PRIVATE VARIABLES

        private SqlConnection _sqlConnection = null;
        private SqlCommand _sqlCommand = null;
        private SqlDataAdapter _da = null;
        private DataSet _ds = null;
        private Helper _helper = null;

        #endregion        

        #region CONSTRUCTOR

        internal ServiceClient(ref Helper helper)
        {
            this._helper = helper;
            this._sqlConnection = new SqlConnection(this._helper.ConnectionString);
            this._helper.LoadServiceRelatedStaticData();
        }

        #endregion
  
        #region PRIVATE METHODS

        private void Clean()
        {
            if (this._da != null)
                this._da.Dispose();
            this._da = null;
            if (this._ds != null)
                this._ds.Dispose();
            this._ds = null;
        }

        #endregion

        #region INTERNAL METHODS

        public dynamic GetServices(short serviceId = 0, bool includeServiceProperties = false, bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            DataSet tempDataSet = new DataSet();
            dynamic servicesObj = null;
            dynamic propertiesObj = null;
            XmlDocument servicesXmlDocument = null;
            XmlElement servicesRootElement = null;
            TablePreferences pref = null;
            string entityName = string.Empty;
            if (tablePreferences != null && tablePreferences.ContainsKey(Label.SERVICES))
            {
                pref = tablePreferences[Label.SERVICES];                
                tablePreferences.Add(Label.PROPERTIES, new TablePreferences(Label.PROPERTIES, Label.PROPERTY, pref.ColumnValuesAsXmlAttributes, false));
            }
            else
                entityName = Label.SERVICES;
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_SERVICES, this._sqlConnection);
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._sqlCommand.Parameters.Add(ProcedureParameter.SERVICE_ID, SqlDbType.TinyInt).Value = serviceId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.INCLUDE_SERVICE_PROPERTIES, SqlDbType.Bit).Value = includeServiceProperties;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = onlyActive;
                this._da = new SqlDataAdapter();
                this._da.SelectCommand = this._sqlCommand;
                this._ds = new DataSet();
                this._da.Fill(this._ds);
                if(!this._sqlCommand.IsSuccess())
                {
                    this._helper.CreateProperty(Label.SUCCESS, false);
                    this._helper.CreateProperty(Label.MESSAGE, this._sqlCommand.GetMessage());
                    return this._helper.GetResponse();
                }
                if (this._ds.Tables.Count > 1)
                {
                    this._ds.Tables[0].TableName = Label.SERVICES;
                    this._ds.Tables[1].TableName = Label.PROPERTIES;
                }
                else if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.SERVICES;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                if(this._ds.Tables.Contains(Label.SERVICES) && this._ds.Tables[Label.SERVICES].Rows.Count > 0)
                {
                    if (tablePreferences != null && tablePreferences.ContainsKey(Label.SERVICES))
                    {
                        pref = tablePreferences[Label.SERVICES];
                        if (pref.SingleRowAsSingleEntity && this._ds.Tables[Label.SERVICES].Rows.Count == 1)
                            entityName = pref.GetSingleRowName();
                        if (entityName.Length == 0)
                            entityName = pref.RootName.Length > 0 ? pref.RootName : Label.SERVICES;
                    }
                    else
                        entityName = Label.SERVICES;
                    tempDataSet.Tables.Add(this._ds.Tables[Label.SERVICES].Copy());
                    this._helper.ParseDataSet(tempDataSet, tablePreferences);
                    servicesObj = this._helper.GetResponse();
                    this._helper.ResetResponseVariables();

                    #region ADD EMPTY PROPERTIES ENTITY
                    if(this._helper.IsOutputXmlFormat)
                    {
                        servicesXmlDocument = new XmlDocument();
                        servicesXmlDocument.LoadXml(servicesObj.ToString());
                        servicesRootElement = servicesXmlDocument.FirstChild as XmlElement;
                        if(pref != null && pref.SingleRowAsSingleEntity && this._ds.Tables[Label.SERVICES].Rows.Count == 1)
                        {
                            XmlElement propertiesElement = servicesXmlDocument.CreateElement(Label.PROPERTIES);
                            servicesRootElement.SelectSingleNode(entityName).AppendChild(propertiesElement);
                        }
                        else
                        {
                            foreach (XmlElement serviceElement in servicesRootElement.SelectNodes(entityName + "/" + (pref != null && pref.ChildElementNameForRows.Length > 0 ? pref.ChildElementNameForRows : entityName)))
                            {
                                XmlElement propertiesElement = servicesXmlDocument.CreateElement(Label.PROPERTIES);
                                serviceElement.AppendChild(propertiesElement);
                            }
                        }                            
                    }
                    else
                    {
                        if(pref != null && pref.SingleRowAsSingleEntity && this._ds.Tables[Label.SERVICES].Rows.Count == 1)
                            servicesObj.SelectToken(entityName).Add(new JProperty(Label.PROPERTIES, new JArray()));                                
                        else
                        {
                            foreach(JObject serviceObject in servicesObj.SelectToken(entityName))
                            {
                                serviceObject.Add(new JProperty(Label.PROPERTIES, new JArray()));
                            }
                        }
                    }
                    #endregion

                    #region POPULATE PROPERTIES
                    if (this._ds.Tables.Contains(Label.PROPERTIES) && this._ds.Tables[Label.PROPERTIES].Rows.Count > 0)
                    {
                        tempDataSet.Tables.Clear();
                        tempDataSet.Tables.Add(this._ds.Tables[Label.PROPERTIES].Copy());
                        this._helper.ParseDataSet(tempDataSet, tablePreferences);
                        propertiesObj = this._helper.GetResponse();
                        this._helper.ResetResponseVariables();
                        if (this._helper.IsOutputXmlFormat)
                        {
                            XmlDocument propertiesXmlDocument = new XmlDocument();
                            propertiesXmlDocument.LoadXml(propertiesObj);
                            XmlElement propertiesRootElement = propertiesXmlDocument.FirstChild as XmlElement;
                            if (pref != null && pref.SingleRowAsSingleEntity && this._ds.Tables[Label.SERVICES].Rows.Count == 1)
                            {
                                servicesRootElement.SelectSingleNode(entityName).SelectSingleNode(Label.PROPERTIES).InnerXml = propertiesRootElement.InnerXml;
                            }
                            else
                            {
                                foreach (XmlElement serviceElement in servicesRootElement.SelectNodes(entityName + "/" + (pref != null && pref.ChildElementNameForRows.Length > 0 ? pref.ChildElementNameForRows : entityName)))
                                {
                                    foreach (XmlElement servicePropertyElement in propertiesRootElement.SelectNodes(Label.PROPERTIES + "/" + Label.PROPERTY))
                                    {
                                        if (servicePropertyElement.Attributes.Count > 0)
                                        {
                                            if (Convert.ToSByte(servicePropertyElement.Attributes[Label.SERVICE_ID].Value) == Convert.ToSByte(serviceElement.Attributes[Label.ID].Value))
                                            {   
                                                serviceElement.SelectSingleNode(Label.PROPERTIES).AppendChild(servicesXmlDocument.ImportNode(servicePropertyElement as XmlNode, true));
                                            }
                                        }
                                        else
                                        {
                                            if (Convert.ToSByte(servicePropertyElement.SelectSingleNode(Label.SERVICE_ID).InnerText) == Convert.ToSByte(serviceElement.SelectSingleNode(Label.ID).InnerText))
                                            {
                                                serviceElement.SelectSingleNode(Label.PROPERTIES).AppendChild(servicePropertyElement);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (pref != null && pref.SingleRowAsSingleEntity && this._ds.Tables[Label.SERVICES].Rows.Count == 1)
                            {
                                servicesObj.SelectToken(entityName).SelectToken(Label.PROPERTIES).Add(propertiesObj.SelectToken(Label.PROPERTIES).Children());
                            }
                            else
                            {
                                foreach (JObject serviceObject in servicesObj.SelectToken(entityName))
                                {
                                    foreach (JObject servicePropertyObject in propertiesObj.SelectToken(Label.PROPERTIES))
                                    {
                                        if (Convert.ToSByte(servicePropertyObject.SelectToken(Label.SERVICE_ID).ToString()) == Convert.ToSByte(serviceObject.SelectToken(Label.ID).ToString()))
                                        {
                                            (serviceObject.SelectToken(Label.PROPERTIES) as JArray).Add(servicePropertyObject);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                #region Constructing Output Parameters
                tempDataSet.Tables.Clear();
                tempDataSet.Tables.Add(this._ds.Tables[Label.OUTPUT_PARAMETERS].Copy());
                this._helper.ResetResponseVariables();
                this._helper.ParseDataSet(tempDataSet);
                dynamic outputParamsObj = this._helper.GetResponse();
                if (this._helper.IsOutputXmlFormat)
                {
                    XmlDocument outputParamsXmlDocument = new XmlDocument();
                    XmlElement outputParamsRootElement = null;
                    outputParamsXmlDocument.LoadXml(outputParamsObj);
                    outputParamsRootElement = outputParamsXmlDocument.FirstChild as XmlElement;
                    foreach (XmlElement childElement in outputParamsRootElement.ChildNodes)
                    {
                        XmlElement newElement = servicesXmlDocument.CreateElement(childElement.Name);
                        newElement.InnerText = childElement.InnerText;
                        servicesRootElement.PrependChild(newElement);
                    }
                }
                else
                {
                    foreach (JProperty jproperty in outputParamsObj.Properties())
                    {
                        servicesObj.AddFirst(new JProperty(jproperty.Name, jproperty.Value));
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                throw new ClientInitializationException(string.Format("Could not load Services list from database. Reason : {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
            return this._helper.IsOutputXmlFormat ? servicesXmlDocument.OuterXml : servicesObj;
        }
        public dynamic CreateService(string displayName, string metaDataCode, bool areMultipleEntriesAllowed)
        {
            try
            {
                if (displayName.Trim().Length == 0 || metaDataCode.Trim().Length == 0)
                    throw new ServiceException(string.Format("DisplayName & MetaDataCode are mandatory"));
                this._sqlCommand = new SqlCommand(StoredProcedure.CREATE_SERVICE, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.DISPLAY_NAME, SqlDbType.VarChar, 50).Value = displayName;
                this._sqlCommand.Parameters.Add(ProcedureParameter.META_DATA_CODE, SqlDbType.VarChar, 20).Value = metaDataCode;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ARE_MULTIPLE_ENTRIES_ALLOWED, SqlDbType.Bit).Value = areMultipleEntriesAllowed;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.SERVICES;
                //if (!this._sqlCommand.IsSuccess())
                //    throw new ServiceException(string.Format("Procedure {0} Returned False. {1}", StoredProcedure.CREATE_SERVICE, this._sqlCommand.GetMessage()));
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds);
                return this._helper.GetResponse();                
            }
            catch (Exception e)
            {
                throw new ServiceException(string.Format("Unable to create Service. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        #endregion
    }
}
