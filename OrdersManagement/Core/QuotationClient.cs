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
    internal class QuotationClient
    {
        #region PRIVATE VARIABLES

        private SqlConnection _sqlConnection = null;
        private SqlCommand _sqlCommand = null;
        private SqlDataAdapter _da = null;
        private DataSet _ds = null;
        private Helper _helper = null;
        private decimal _orderAmount = 0;

        #endregion        

        #region CONSTRUCTOR

        internal QuotationClient(ref Helper helper)
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
        private dynamic ErrorResponse(string message = null)
        {
            this._helper.CreateProperty(Label.SUCCESS, false);
            if (message != null)
                this._helper.CreateProperty(Label.MESSAGE, message);
            else
                this._helper.CreateProperty(Label.MESSAGE, this._sqlCommand.GetMessage());
            return this._helper.GetResponse();
        }
        private void ValidateQuotation(string metaData)
        {
            if (!SharedClass.ServiceLoaded)
                this._helper.LoadServices();
            JObject quotationMetaData = null;
            try
            {
                quotationMetaData = JObject.Parse(metaData);
            }
            catch(Exception e)
            {
                Logger.Error(string.Format("Unable to format Quotation MetaData. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to format Quotation MetaData. {0}", e.Message));
            }
            JObject tempJObj = null;
            JArray tempJArray = null;
            _orderAmount = 0;
            foreach (JProperty property in quotationMetaData.Properties())
            {
                if (!SharedClass.Services.ContainsKey(property.Name))
                    throw new QuotationException(string.Format("Service ({0}) doesn't exist", property.Name));
                if(SharedClass.Services[property.Name].AreMultipleEntriesAllowed)
                {
                    tempJArray = quotationMetaData.SelectToken(property.Name) as JArray;
                    if (tempJArray == null)
                        throw new QuotationException(string.Format("Invalid JSon Array for Service {0}", property.Name));
                    foreach(JToken child in tempJArray.Children())
                    {
                        tempJObj = child as JObject;
                        if (tempJObj == null)
                            throw new QuotationException(string.Format("Invalid Child MetaData for {0}", property.Name));
                        ParseServicePropertyJSon(property.Name, tempJObj);
                    }
                }
                else
                {
                    tempJObj = property.Value as JObject;
                    if (tempJObj == null)
                        throw new QuotationException(string.Format("Invalid MetaData for Service ({0})", property.Name));
                    ParseServicePropertyJSon(property.Name, tempJObj);
                }
            }            
        }
        private void ParseServicePropertyJSon(string serviceName, JObject serviceProperty)
        {
            foreach (KeyValuePair<string, ServiceProperty> servicePropertyEntry in SharedClass.Services[serviceName].Properties)
            {
                if (servicePropertyEntry.Value.IsRequired && (serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode) == null
                        || serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString().Trim().Length == 0))
                    throw new QuotationException(string.Format("Property {0} for Service {1} is marked as Required. But it is not found or empty.",
                        servicePropertyEntry.Value.MetaDataCode, serviceName));
                if (serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode) != null && serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString().Trim().Length > 0)
                {
                    switch (servicePropertyEntry.Value.DataType)
                    {
                        case PropertyDataType.INT:
                            try
                            {
                                int.Parse(serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString());                                
                            }
                            catch (Exception e)
                            {
                                throw new QuotationException(string.Format("Property {0} for Service {1} requires Int value. But '{2}' is not an Int value.",
                                    servicePropertyEntry.Value.MetaDataCode, serviceName, serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString()));
                            }
                            break;
                        case PropertyDataType.FLOAT:
                            try
                            {
                                float.Parse(serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString());
                            }
                            catch (Exception e)
                            {
                                throw new QuotationException(string.Format("Property {0} for Service {1} requires float value. But '{2}' is not a float value.",
                                    servicePropertyEntry.Value.MetaDataCode, serviceName, serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString()));
                            }
                            break;
                        case PropertyDataType.DATETIME:
                            try
                            {
                                DateTime.Parse(serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString());
                            }
                            catch (Exception e)
                            {
                                throw new QuotationException(string.Format("Property {0} for Service {1} requires DateTime value. But '{2}' is not a valid DateTime value.",
                                    servicePropertyEntry.Value.MetaDataCode, serviceName, serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString()));
                            }
                            break;
                        default:
                            break;
                    }
                    if (servicePropertyEntry.Value.IncludeInOrderAmount)                    
                        this._orderAmount += Convert.ToDecimal(serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString());                    
                    if(serviceProperty.SelectToken(Label.EXTRA_CHARGES) != null)
                    {
                        JArray extraCharges = serviceProperty.SelectToken(Label.EXTRA_CHARGES) as JArray;
                        if (extraCharges == null)
                            throw new QuotationException(string.Format("Unable to parse {0} for Service {1}", Label.EXTRA_CHARGES, serviceName));
                        foreach (JToken child in extraCharges.Children())
                        { 
                            try
                            {
                                JObject childObject = child as JObject;
                                if (childObject == null)
                                    throw new QuotationException(string.Format("Error Parsing {0} Child for Service {1}", Label.EXTRA_CHARGES, serviceName));
                                if (childObject.SelectToken(Label.AMOUNT) == null)
                                    throw new QuotationException(string.Format("{0} Property is missing in {1} for Service {2}", Label.AMOUNT, Label.EXTRA_CHARGES, serviceName));
                                this._orderAmount += Convert.ToDecimal(childObject.SelectToken(Label.AMOUNT).ToString());
                            }
                            catch(Exception e)
                            {
                                throw new QuotationException(string.Format("Unable add {0} to order amount for Service {1}", Label.EXTRA_CHARGES, serviceName));
                            }
                        }
                    }
                }
            }
        }
        private void ValidateQuotationOld(string metaData)
        {
            JObject quotationMetaData = null;
            try
            {
                quotationMetaData = JObject.Parse(metaData);
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to format Quotation MetaData. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to format Quotation MetaData. {0}", e.Message));
            }
            ServiceClient client = new ServiceClient(ref this._helper);
            this._helper.ResetResponseVariables();
            JObject services = client.GetServices(includeServiceProperties: true);
            bool isServiceExist = false;
            foreach (JProperty property in quotationMetaData.Properties())
            {
                isServiceExist = false;
                foreach (JObject serviceObject in services.SelectToken(Label.SERVICES))
                {
                    if (serviceObject.SelectToken(Label.META_DATA_CODE) != null && serviceObject.SelectToken(Label.META_DATA_CODE).ToString().Equals(property.Name))
                    {
                        isServiceExist = true;
                        break;
                    }
                }
                if (!isServiceExist)
                    throw new QuotationException(string.Format("Invalid Service {0}", property.Name));

            }
            this._helper.ResetResponseVariables();
        }

        #endregion

        #region INTERNAL METHODS

        internal dynamic GetStatuses(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_QUOTATION_STATUSES, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = onlyActive;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.QUOTATION_STATUSES;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch(Exception e)
            {
                Logger.Error(string.Format("Unable to fetch QuotationStatuses. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch QuotationStatuses. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic GetChannels(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_QUOTATION_CHANNELS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = onlyActive;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.QUOTATION_CHANNELS;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch(Exception e)
            {
                Logger.Error(string.Format("Unable to fetch QuotationChannels. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch QuotationChannels. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic Search(int quotationId = 0, string quotationNumber = "", int accountId = 0, int employeeId = 0, int ownerShipId = 0, byte statusId = 0, sbyte channelId = 0, string ipAddress = "", byte billingModeId = 0, Nullable<DateTime> fromDateTime = null, Nullable<DateTime> toDateTime = null, int pageNumber = 1, byte limit = 20, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_QUOTATIONS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_NUMBER, SqlDbType.VarChar, 20).Value = quotationNumber;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_ID, SqlDbType.Int).Value = accountId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMPLOYEE_ID, SqlDbType.Int).Value = employeeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.OWNERSHIP_ID, SqlDbType.Int).Value = ownerShipId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.STATUS_ID, SqlDbType.TinyInt).Value = statusId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.CHANNEL_ID, SqlDbType.TinyInt).Value = channelId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IP_ADDRESS, SqlDbType.VarChar, 50).Value = ipAddress;
                this._sqlCommand.Parameters.Add(ProcedureParameter.BILLING_MODE_ID, SqlDbType.TinyInt).Value = billingModeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.FROM_DATE_TIME, SqlDbType.DateTime).Value = fromDateTime;
                this._sqlCommand.Parameters.Add(ProcedureParameter.TO_DATE_TIME, SqlDbType.DateTime).Value = toDateTime;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PAGE_NUMBER, SqlDbType.Int).Value = pageNumber;
                this._sqlCommand.Parameters.Add(ProcedureParameter.LIMIT, SqlDbType.TinyInt).Value = limit;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.QUOTATIONS;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch(Exception e)
            {
                Logger.Error(string.Format("Unable to Search Quotations. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to Search Quotations. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic Create(int accountId, int employeeId, byte channelId, string metaData, string ipAddress, int stateId)
        {
            this.ValidateQuotation(metaData);
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.CREATE_QUOTATION, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_ID, SqlDbType.Int).Value = accountId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMPLOYEE_ID, SqlDbType.Int).Value = employeeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.CHANNEL_ID, SqlDbType.TinyInt).Value = channelId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.META_DATA, SqlDbType.VarChar, 2000).Value = metaData;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IP_ADDRESS, SqlDbType.VarChar, 50).Value = ipAddress;
                this._sqlCommand.Parameters.Add(ProcedureParameter.STATE_ID, SqlDbType.Int).Value = stateId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ORDER_AMOUNT, SqlDbType.Decimal).Value = this._orderAmount;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.QUOTATION;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                TablePreferences tp = new TablePreferences(RootName: Label.QUOTATION, childElementNameForRows: null, columnValuesAsXmlAttributes: true, singleRowAsSingleEntity: true);
                Dictionary<string, TablePreferences> tpDic = new Dictionary<string, TablePreferences>();
                tpDic.Add(Label.QUOTATION, tp);
                this._helper.ParseDataSet(this._ds, tpDic);
                return this._helper.GetResponse();
            }
            catch(Exception e)
            {
                Logger.Error(string.Format("Unable to create Quotation. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to create Quotation. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic Update(int quotationId, int employeeId, string metaData, string ipAddress, int stateId)
        {
            this.ValidateQuotation(metaData);
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.UPDATE_QUOTATION, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMPLOYEE_ID, SqlDbType.Int).Value = employeeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.META_DATA, SqlDbType.VarChar, 2000).Value = metaData;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IP_ADDRESS, SqlDbType.VarChar, 50).Value = ipAddress;
                this._sqlCommand.Parameters.Add(ProcedureParameter.STATE_ID, SqlDbType.Int).Value = stateId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ORDER_AMOUNT, SqlDbType.Decimal).Value = this._orderAmount;                
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.QUOTATION;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                TablePreferences tp = new TablePreferences(RootName: Label.QUOTATION, childElementNameForRows: null, columnValuesAsXmlAttributes: true, singleRowAsSingleEntity: true);
                Dictionary<string, TablePreferences> tpDic = new Dictionary<string, TablePreferences>();
                tpDic.Add(Label.QUOTATION, tp);
                this._helper.ParseDataSet(this._ds, tpDic);
                return this._helper.GetResponse();
            }
            catch(Exception e)
            {
                Logger.Error(string.Format("Exception while updating the Quotation. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to Update. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic Delete(int quotationId, bool isPostPaidQuotation)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.DELETE_QUOTATION, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_POSTPAID_QUOTATION, SqlDbType.Bit).Value = isPostPaidQuotation;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.QUOTATION;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds);
                return this._helper.GetResponse();
            }
            catch(Exception e)
            {
                Logger.Error(string.Format("Exception while delete quotation. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to delete. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        #endregion
    }
}