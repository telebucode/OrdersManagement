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
using System.Text.RegularExpressions;
using SelectPdf;
using System.IO;
using System.Web;
using System.Net;
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
            //if (this._ds != null)
            //    this._ds.Dispose();
            //this._ds = null;
        }

        private bool SpecialCharactersCheck(string value)
        {
            var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
            var isAllow = false;

            if (regexItem.IsMatch(value))
            {
                isAllow = true;
            }
            return isAllow;
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
        private void ValidateQuotation(string metaData, byte productId = 0)
        {
            if (!SharedClass.ServiceLoaded)
                this._helper.LoadServices();
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
            JObject tempJObj = null;
            JArray tempJArray = null;
            _orderAmount = 0;
            foreach (JProperty property in quotationMetaData.Properties())
            {

                //if (SharedClass.Services[property.Name].ProductId != productId)
                //    throw new QuotationException(string.Format("Service ({0}) doesn't belong to this product", property.Name));

                if (!SharedClass.Services.ContainsKey(property.Name))
                    throw new QuotationException(string.Format("Service ({0}) doesn't exist", property.Name));

                tempJArray = quotationMetaData.SelectToken(property.Name) as JArray;
                if (SharedClass.Services[property.Name].AreMultipleEntriesAllowed && tempJArray != null)
                {

                    if (tempJArray == null)
                        throw new QuotationException(string.Format("Invalid JSon Array for Service {0}", property.Name));
                    foreach (JToken child in tempJArray.Children())
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
                        case PropertyDataType.MONEY:
                            try
                            {
                                Decimal.Parse(serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString());
                            }
                            catch (Exception e)
                            {
                                throw new QuotationException(string.Format("Property {0} for Service {1} requires Money value. But '{2}' is not a valid money value.",
                                    servicePropertyEntry.Value.MetaDataCode, serviceName, serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString()));
                            }

                            break;
                        default:
                            break;
                    }
                    if ((servicePropertyEntry.Value.InputType == PropertyInputType.TEXT_AREA || servicePropertyEntry.Value.InputType == PropertyInputType.TEXT_BOX) && servicePropertyEntry.Value.DataType == PropertyDataType.STRING)
                    {
                        //servicePropertyEntry.Value.PropertFields;
                        if (serviceProperty.SelectToken(servicePropertyEntry.Key).ToString().Length < servicePropertyEntry.Value.PropertFields[servicePropertyEntry.Key].MinLength)
                        {
                            throw new QuotationException(string.Format("Property {0} for Service {1} value is invalid or it's length is not supported",
                                   servicePropertyEntry.Value.MetaDataCode, serviceName));

                        }
                        if (serviceProperty.SelectToken(servicePropertyEntry.Key).ToString().Length > servicePropertyEntry.Value.PropertFields[servicePropertyEntry.Key].MaxLength)
                        {
                            throw new QuotationException(string.Format("Property {0} for Service {1} value length is exceeded",
                                   servicePropertyEntry.Value.MetaDataCode, serviceName));

                        }
                    }
                    if (servicePropertyEntry.Value.InputType == PropertyInputType.TEXT_AREA || servicePropertyEntry.Value.InputType == PropertyInputType.TEXT_BOX)
                    {
                        if (!SpecialCharactersCheck((serviceProperty.SelectToken(servicePropertyEntry.Key).ToString())))
                        {
                            throw new QuotationException(string.Format("Property {0} for Service {1} have special characters which are not allowed ", servicePropertyEntry.Value.MetaDataCode, serviceName));

                        }

                    }
                    if (servicePropertyEntry.Value.InputType != PropertyInputType.TEXT_AREA && servicePropertyEntry.Value.InputType != PropertyInputType.TEXT_BOX)
                    {
                        if (!servicePropertyEntry.Value.PropertFields.Keys.Contains(servicePropertyEntry.Value.ToString()))
                        {

                            throw new QuotationException(string.Format("Property {0} for Service {1} value is not valid value ", servicePropertyEntry.Value.MetaDataCode, serviceName));
                        }
                    }

                    if (servicePropertyEntry.Value.IncludeInOrderAmount)
                        this._orderAmount += Convert.ToDecimal(serviceProperty.SelectToken(servicePropertyEntry.Value.MetaDataCode).ToString());


                    //ValidateServicePropertFieldJSon(serviceName, serviceProperty);
                }
            }
            if (serviceProperty.SelectToken(Label.EXTRA_CHARGES) != null)
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
                    catch (Exception e)
                    {
                        throw new QuotationException(string.Format("Unable add {0} to order amount for Service {1}", Label.EXTRA_CHARGES, serviceName));
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

        /// <summary>
        /// Gets the statuses.
        /// </summary>
        /// <param name="onlyActive">if set to <c>true</c> [only active].</param>
        /// <param name="tablePreferences">The table preferences.</param>
        /// <returns></returns>
        /// <exception cref="QuotationException"></exception>
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
            catch (Exception e)
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
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to fetch QuotationChannels. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch QuotationChannels. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic Search(int productId = 0, int quotationId = 0, string quotationNumber = "", int accountId = 0, int employeeId = 0, int ownerShipId = 0, byte statusId = 0, sbyte channelId = 0, string ipAddress = "", byte billingModeId = 0, Nullable<DateTime> fromDateTime = null, Nullable<DateTime> toDateTime = null, int pageNumber = 1, byte limit = 20, Dictionary<string, TablePreferences> tablePreferences = null, string mobile = "", string email = "")
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_QUOTATIONS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.PRODUCT_ID, SqlDbType.Int).Value = productId;
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
                this._sqlCommand.Parameters.Add(ProcedureParameter.MOBILE, SqlDbType.VarChar, 15).Value = mobile;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMAIL, SqlDbType.VarChar, 200).Value = email;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PAGE_NUMBER, SqlDbType.Int).Value = pageNumber;
                this._sqlCommand.Parameters.Add(ProcedureParameter.LIMIT, SqlDbType.TinyInt).Value = limit;
                this._sqlCommand.Parameters.Add(ProcedureParameter.COUNT, SqlDbType.Int).Direction = ParameterDirection.Output;
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
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to Search Quotations. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to Search Quotations. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic Create(byte productId, int accountId, int employeeId, byte channelId, string metaData, string ipAddress, int stateId)
        {
            this.ValidateQuotation(metaData, productId);
            try
            {
                List<QuotationServices> quotationServices = ListOfQuotationServices(metaData);
                List<QuotationServiceProperties> quotationServiceProperties = ListOfQuotationServiceProperties(metaData, Label.CREATE);

                this._sqlCommand = new SqlCommand(StoredProcedure.CREATE_QUOTATION, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.PRODUCT_ID, SqlDbType.Int).Value = productId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_ID, SqlDbType.Int).Value = accountId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMPLOYEE_ID, SqlDbType.Int).Value = employeeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.CHANNEL_ID, SqlDbType.TinyInt).Value = channelId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_SERVICES, SqlDbType.Structured).Value = quotationServices.ToQuotationServicesDataTable();
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_SERVICE_PROPERTIES, SqlDbType.Structured).Value = quotationServiceProperties.ToQuotationServicePropertiesDataTable();
                this._sqlCommand.Parameters.Add(ProcedureParameter.IP_ADDRESS, SqlDbType.VarChar, 50).Value = ipAddress;
                this._sqlCommand.Parameters.Add(ProcedureParameter.STATE_ID, SqlDbType.Int).Value = stateId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ORDER_AMOUNT, SqlDbType.Decimal).Value = this._orderAmount;
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.BigInt).Direction = ParameterDirection.Output;
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
            catch (Exception e)
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
            this.ValidateQuotation(metaData: metaData);
            try
            {

                List<QuotationServices> quotationServices = ListOfQuotationServices(metaData);
                List<QuotationServiceProperties> quotationServiceProperties = ListOfQuotationServiceProperties(metaData, Label.UPDATE);
                this._sqlCommand = new SqlCommand(StoredProcedure.UPDATE_QUOTATION, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMPLOYEE_ID, SqlDbType.Int).Value = employeeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.META_DATA, SqlDbType.VarChar, 2000).Value = metaData;
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_SERVICES, SqlDbType.Structured).Value = quotationServices.ToQuotationServicesDataTable();
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_SERVICE_PROPERTIES, SqlDbType.Structured).Value = quotationServiceProperties.ToQuotationServicePropertiesDataTable();
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
            catch (Exception e)
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
            catch (Exception e)
            {
                Logger.Error(string.Format("Exception while delete quotation. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to delete. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic GetQuotationDetails(int quotationId, bool isPostPaidQuotation)
        {
            DataSet tempDataSet = new DataSet();
            dynamic quotationObj = null;
            dynamic quotationServicesObj = null;
            dynamic quotationServicePropertiesObj = null;
            string entityName = string.Empty;
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_QUOTATION_DETAILS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_POSTPAID_QUOTATION, SqlDbType.Bit).Value = isPostPaidQuotation;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 2)
                {
                    this._ds.Tables[0].TableName = Label.QUOTATION;
                    this._ds.Tables[1].TableName = Label.QUOTATION_SERVICES;
                    this._ds.Tables[2].TableName = Label.QUOTATION_SERVICE_PROPERTIES;
                }
                else if (this._ds.Tables.Count > 1)
                {
                    this._ds.Tables[0].TableName = Label.QUOTATION;
                    this._ds.Tables[1].TableName = Label.QUOTATION_SERVICES;
                }
                else
                {
                    this._ds.Tables[0].TableName = Label.QUOTATION;
                }
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                //this._helper.ParseDataSet(this._ds);

                if (this._ds.Tables.Contains(Label.QUOTATION) && this._ds.Tables[Label.QUOTATION].Rows.Count > 0)
                {
                    tempDataSet.Tables.Add(this._ds.Tables[Label.QUOTATION].Copy());
                    this._helper.ParseDataSet(tempDataSet);
                    quotationObj = this._helper.GetResponse();
                    this._helper.ResetResponseVariables();
                    if (!this._helper.IsOutputXmlFormat)
                    {
                        quotationObj.SelectToken(Label.QUOTATION).Add(new JProperty(Label.QUOTATION_SERVICES, new JArray()));
                    }



                    #region POPULATE_QUOTATION_SERVICES
                    if (this._ds.Tables.Contains(Label.QUOTATION_SERVICES) && this._ds.Tables[Label.QUOTATION_SERVICES].Rows.Count > 0)
                    {
                        tempDataSet.Tables.Clear();
                        tempDataSet.Tables.Add(this._ds.Tables[Label.QUOTATION_SERVICES].Copy());
                        this._helper.ParseDataSet(tempDataSet);
                        quotationServicesObj = this._helper.GetResponse();
                        this._helper.ResetResponseVariables();
                        if (this._ds.Tables.Contains(Label.QUOTATION_SERVICE_PROPERTIES) && this._ds.Tables[Label.QUOTATION_SERVICE_PROPERTIES].Rows.Count > 0)
                        {

                            tempDataSet.Tables.Clear();
                            tempDataSet.Tables.Add(this._ds.Tables[Label.QUOTATION_SERVICE_PROPERTIES].Copy());
                            this._helper.ParseDataSet(tempDataSet);
                            quotationServicePropertiesObj = this._helper.GetResponse();
                            this._helper.ResetResponseVariables();
                            //if (!this._helper.IsOutputXmlFormat)
                            //{
                            //    (quotationObj.SelectToken(Label.QUOTATION).SelectToken(Label.QUOTATION_SERVICES) as JArray).Add(quotationServicesObj.SelectToken(Label.QUOTATION_SERVICES));
                            //}

                        }

                        if (!this._helper.IsOutputXmlFormat)
                        {
                            if (_ds.Tables[Label.QUOTATION_SERVICES].Rows.Count == 1)
                            {
                                quotationServicesObj.SelectToken(Label.QUOTATION_SERVICES).Add(new JProperty(Label.QUOTATION_SERVICE_PROPERTIES, new JArray()));
                                quotationServicesObj.SelectToken(Label.QUOTATION_SERVICES).SelectToken(Label.QUOTATION_SERVICE_PROPERTIES).Add(quotationServicePropertiesObj.SelectToken(Label.QUOTATION_SERVICE_PROPERTIES).Children());
                                quotationObj.SelectToken(Label.QUOTATION).SelectToken(Label.QUOTATION_SERVICES).Add(quotationServicesObj.SelectToken(Label.QUOTATION_SERVICES));

                            }
                            else
                            {
                                foreach (JObject quotationServices in quotationServicesObj.SelectToken(Label.QUOTATION_SERVICES))
                                {
                                    quotationServices.Add(new JProperty(Label.QUOTATION_SERVICE_PROPERTIES, new JArray()));

                                    foreach (JObject quotationServiceProperties in quotationServicePropertiesObj.SelectToken(Label.QUOTATION_SERVICE_PROPERTIES))
                                    {
                                        if (Convert.ToInt32(quotationServices.SelectToken(Label.ID).ToString()) == Convert.ToInt32(quotationServiceProperties.SelectToken(Label.QUOTATION_SERVICE_ID).ToString()))
                                        {
                                            (quotationServices.SelectToken(Label.QUOTATION_SERVICE_PROPERTIES) as JArray).Add(quotationServiceProperties);
                                        }
                                    }
                                }

                                quotationObj.SelectToken(Label.QUOTATION).SelectToken(Label.QUOTATION_SERVICES).Add(quotationServicesObj.SelectToken(Label.QUOTATION_SERVICES).Children());

                            }

                        }


                    }
                    #endregion

                }


            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to get quotation details. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to get quotation details. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }

            return this.IncludeOutputParamsInObj(_ds: _ds, servicesXmlDocument: null, servicesRootElement: null, quotationObj: quotationObj);
        }
        internal dynamic ViewQuotation(int quotationId, bool isPostPaidQuotation)
        {
            DataSet tempDataSet = new DataSet();
            string entityName = string.Empty;
            string quotationData = string.Empty;
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.VIEW_OR_DOWNLOAD_QUOTATION, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_POSTPAID_QUOTATION, SqlDbType.Bit).Value = isPostPaidQuotation;
                this._sqlCommand.Parameters.Add(ProcedureParameter.HTML, SqlDbType.VarChar, -1).Direction = ParameterDirection.Output;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                quotationData = this._sqlCommand.Parameters[ProcedureParameter.HTML].IsDbNull() ? "Null Message" : this._sqlCommand.Parameters[ProcedureParameter.HTML].Value.ToString();

            }
            catch (Exception e)
            {
                quotationData = "";
                Logger.Error(string.Format("Unable to get quotation details. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to get quotation details. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
            return quotationData;
        }
        internal dynamic DownloadQuotation(int quotationId, bool isPostPaidQuotation)
        {
            DataSet tempDataSet = new DataSet();
            string entityName = string.Empty;
            string quotationData = string.Empty;
            JObject responseJobj = null;
            string filePath = string.Empty;
            string fileName = string.Empty;
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.VIEW_OR_DOWNLOAD_QUOTATION, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.HTML, SqlDbType.VarChar, -1).Direction = ParameterDirection.Output;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();

                fileName = "Quotation_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                filePath = HttpContext.Current.Server.MapPath("/Files/Quotations") + '/' + fileName;
                if (ConvertHtmlToPdf(this._sqlCommand.Parameters[ProcedureParameter.HTML].IsDbNull() ? "Null Message" : this._sqlCommand.Parameters[ProcedureParameter.HTML].Value.ToString(), filePath))
                {
                    this._helper.CreateProperty("Success", true);
                    this._helper.CreateProperty("Message", "Success");
                    this._helper.CreateProperty("FilePath", "Files/Quotations/" + fileName);
                }
                else
                {
                    this._helper.CreateProperty("Success", false);
                    this._helper.CreateProperty("Message", "Error while converting pdf");
                }


            }
            catch (Exception e)
            {
                this._helper.CreateProperty("Success", true);
                this._helper.CreateProperty("Message", e.Message.ToString());
                Logger.Error(string.Format("Unable to get quotation details. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to get quotation details. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
            return this._helper.GetResponse();
        }
        internal dynamic ConvertHtmlToPdf(string html, string filePath)
        {
            JObject jobj = new JObject();
            try
            {
                HtmlToPdf pdfConverter = new HtmlToPdf();
                PdfDocument doc = pdfConverter.ConvertHtmlString(html);
                doc.Save(filePath);
                doc.Close();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to convert html to pdf. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to convert html to pdf. {0}", e.Message));
                return false;
            }

        }
        internal List<QuotationServices> ListOfQuotationServices(string metaData)
        {
            List<QuotationServices> quotationServicesList = new List<QuotationServices>();
            JObject serviceProperties = JObject.Parse(metaData);
            JToken tempJArray = null;
            foreach (JProperty jpropert in serviceProperties.Properties())
            {

                tempJArray = jpropert.Value;
                if (tempJArray.Type == JTokenType.Array)
                {
                    foreach (JObject serviceObj in tempJArray)
                    {
                        QuotationServices quotationService = new QuotationServices();
                        quotationService.MetaDataCode = jpropert.Name;
                        quotationService.Id = Convert.ToInt16(serviceObj.SelectToken(Label.ID).ToString());
                        quotationService.ServiceId = SharedClass.Services[quotationService.MetaDataCode].Id;
                        quotationService.Occurance = Convert.ToByte(serviceObj.SelectToken(Label.OCCURANCE).ToString());
                        quotationService.ExtraCharges = OrdersManagement.ExtensionMethods.ToExtraChargesString(serviceObj.SelectToken(Label.EXTRA_CHARGES), quotationService.MetaDataCode);
                        quotationServicesList.Add(quotationService);

                    }
                }
                else
                {

                    QuotationServices quotationService = new QuotationServices();
                    quotationService.MetaDataCode = jpropert.Name;
                    quotationService.Id = Convert.ToByte(jpropert.Value.SelectToken(Label.ID).ToString());
                    quotationService.ServiceId = SharedClass.Services[quotationService.MetaDataCode].Id;
                    quotationService.Occurance = Convert.ToByte(jpropert.Value.SelectToken(Label.OCCURANCE).ToString());
                    quotationService.ExtraCharges = OrdersManagement.ExtensionMethods.ToExtraChargesString(jpropert.Value.SelectToken(Label.EXTRA_CHARGES), quotationService.MetaDataCode);
                    quotationServicesList.Add(quotationService);

                }
            }

            return quotationServicesList;

        }
        internal List<QuotationServiceProperties> ListOfQuotationServiceProperties(string metaData, string actionType)
        {
            List<QuotationServiceProperties> quotationServicePropertiesList = new List<QuotationServiceProperties>();
            QuotationServiceProperties quotationserviceProperties;
            JObject serviceProperties = JObject.Parse(metaData);
            JToken tempJArray = null;
            foreach (JProperty jpropert in serviceProperties.Properties())
            {

                tempJArray = jpropert.Value;
                if (tempJArray.Type == JTokenType.Array)
                {
                    foreach (JObject serviceObj in tempJArray)
                    {
                        foreach (JProperty services in serviceObj.Properties())
                        {
                            if (services.Name != Label.ID && services.Name != Label.EXTRA_CHARGES && services.Name != Label.OCCURANCE)
                            {
                                quotationserviceProperties = new QuotationServiceProperties();
                                if (actionType == Label.UPDATE && Convert.ToInt16(serviceObj.SelectToken(Label.ID).ToString()) != 0)
                                {
                                    quotationserviceProperties.QuotationServiceId = Convert.ToInt16(serviceObj.SelectToken(Label.ID).ToString());
                                }
                                else
                                {
                                    quotationserviceProperties.QuotationServiceId = Convert.ToInt16(serviceObj.SelectToken(Label.OCCURANCE).ToString());

                                }
                                quotationserviceProperties.MetaDataCode = services.Name;
                                quotationserviceProperties.ServiceId = SharedClass.Services[jpropert.Name].Id;
                                quotationserviceProperties.ServicePropertiId = SharedClass.Services[jpropert.Name].Properties[quotationserviceProperties.MetaDataCode].Id;
                                quotationserviceProperties.Value = services.Value.ToString();
                                quotationServicePropertiesList.Add(quotationserviceProperties);
                            }
                        }

                    }
                }
                else
                {

                    foreach (JProperty services in tempJArray)
                    {
                        if (services.Name != Label.ID && services.Name != Label.EXTRA_CHARGES && services.Name != Label.OCCURANCE)
                        {
                            quotationserviceProperties = new QuotationServiceProperties();

                            if (actionType == Label.UPDATE && Convert.ToInt16(jpropert.Value.SelectToken(Label.ID).ToString()) != 0)
                            {
                                quotationserviceProperties.QuotationServiceId = Convert.ToInt32(jpropert.Value.SelectToken(Label.ID).ToString());
                            }
                            else
                            {
                                quotationserviceProperties.QuotationServiceId = Convert.ToInt32(jpropert.Value.SelectToken(Label.OCCURANCE).ToString());

                            }
                            quotationserviceProperties.QuotationServiceId = Convert.ToInt16(jpropert.Value.SelectToken(Label.ID).ToString());
                            quotationserviceProperties.MetaDataCode = services.Name;
                            quotationserviceProperties.ServiceId = SharedClass.Services[jpropert.Name].Id;
                            quotationserviceProperties.ServicePropertiId = SharedClass.Services[jpropert.Name].Properties[quotationserviceProperties.MetaDataCode].Id;
                            quotationserviceProperties.Value = services.Value.ToString();
                            quotationServicePropertiesList.Add(quotationserviceProperties);
                        }
                    }

                }
            }

            return quotationServicePropertiesList;
        }

        #endregion
        internal dynamic IncludeOutputParamsInObj(DataSet _ds, XmlDocument servicesXmlDocument, XmlElement servicesRootElement, dynamic quotationObj)
        {
            try
            {
                DataSet tempDataSet = new DataSet();
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
                        quotationObj.AddFirst(new JProperty(jproperty.Name, jproperty.Value));
                    }
                }
            }
            catch (Exception e)
            {
                throw new ClientInitializationException(string.Format("Could not load output parameters in ServiceObj or XmlDocument", e.Message));
            }
            finally
            {

            }

            return this._helper.IsOutputXmlFormat ? servicesXmlDocument.OuterXml : quotationObj;
        }





    }
}