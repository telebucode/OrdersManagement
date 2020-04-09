using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using OrdersManagement.Model;
using OrdersManagement.Exceptions;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;

namespace OrdersManagement.Core
{
    internal class OrdersClient
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

        internal OrdersClient(ref Helper helper)
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
        #endregion

        #region INTERNAL METHODS
        internal dynamic ActivateOrder(int quotationId, bool isPostPaidQuotation, float activationAmount, string activationUrl,string activationComments, int employeeId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                StreamReader SReader = null;
                StreamWriter SWriter = null;
                string HttpAPIResponseString = "";
                JObject repsonseObj;
                dynamic metaData;
                //dynamic logResponse;
                string _ApiKey = "";
                string _ApiSecret = "";
                dynamic ds=this.GetProductDetails(quotationId);
                _ApiKey = ds.Table.ApiKey;
                _ApiSecret = ds.Table.ApiSecret;
                metaData = this.GetRequestObjectForActivation(quotationId, isPostPaidQuotation, activationAmount, activationComments,employeeId);
                Logger.Error(string.Format("Metadata Reuest : {0} | Url : {1}", metaData,activationUrl));
                CredentialCache credentials = new CredentialCache();
                //credentials.Add(uriPrefix: new Uri(activationUrl), authType: "Basic", cred: new NetworkCredential("smscountry", "smsc"));
                credentials.Add(new Uri(activationUrl), "Basic", new NetworkCredential(_ApiKey, _ApiSecret));
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(activationUrl);
                request.Credentials = credentials;
                request.Method = "POST";
                request.ContentType = "application/json";
                SWriter = new StreamWriter(request.GetRequestStream());
                SWriter.Write(metaData.ToString());
                SWriter.Flush();
                SWriter.Close();
                SReader = new StreamReader(request.GetResponse().GetResponseStream());
                HttpAPIResponseString = SReader.ReadToEnd();
                SReader.Close();
                repsonseObj = JObject.Parse(HttpAPIResponseString);
                //logResponse = this.LogOrderData(employeeId, Convert.ToString(metaData), Convert.ToString(repsonseObj));


                return repsonseObj;
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to fetch OrderSummary. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch OrderSummary. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        private dynamic LogOrderData(int employeeId, string requestObject, string responseObject)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.ORDERS_LOG, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMPLOYEE_ID, SqlDbType.Int).Value = employeeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.REQUEST_OBJECT, SqlDbType.VarChar, -1).Value = requestObject;
                this._sqlCommand.Parameters.Add(ProcedureParameter.RESPONSE_OBJECT, SqlDbType.VarChar, -1).Value = responseObject;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, null);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to Log Order Data. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to Log Order Data. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }

        }
        internal dynamic GetOrderSummary(int quotationId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_ORDER_SUMMARY, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.ORDER_SUMMARY;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to fetch OrderSummary. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch OrderSummary. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic GetOrderStatuses(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_ORDER_STATUSES, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = onlyActive;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.ORDER_STATUSES;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to fetch Order Statuses. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch OnliePaymentGateways. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        internal dynamic GetOrders(byte productId, int accountId, string mobile, string email, int orderStatus,
            string number, byte billingMode, DateTime fromDateTime, DateTime toDateTime, string accountName, int pageNumber, byte limit,
            Dictionary<string, TablePreferences> tablePreferences = null,bool isdownload = false)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_ORDERS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.PRODUCT_ID, SqlDbType.TinyInt).Value = productId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_ID, SqlDbType.Int).Value = accountId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_NAME, SqlDbType.VarChar, 128).Value = accountName;
                this._sqlCommand.Parameters.Add(ProcedureParameter.MOBILE, SqlDbType.VarChar, 15).Value = mobile;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMAIL, SqlDbType.VarChar, 126).Value = email;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ORDER_STATUS_ID, SqlDbType.TinyInt).Value = orderStatus;
                this._sqlCommand.Parameters.Add(ProcedureParameter.NUMBER, SqlDbType.VarChar, 32).Value = number;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ISDOWNLOAD, SqlDbType.Bit).Value = isdownload;
                this._sqlCommand.Parameters.Add(ProcedureParameter.BILLING_MODE_ID, SqlDbType.TinyInt).Value = billingMode;
                this._sqlCommand.Parameters.Add(ProcedureParameter.FROM_DATE_TIME, SqlDbType.DateTime).Value = fromDateTime;
                this._sqlCommand.Parameters.Add(ProcedureParameter.TO_DATE_TIME, SqlDbType.DateTime).Value = toDateTime;
                this._sqlCommand.Parameters.Add(ProcedureParameter.LIMIT, SqlDbType.TinyInt).Value = limit;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PAGE_NUMBER, SqlDbType.Int).Value = pageNumber;
                this._sqlCommand.Parameters.Add(ProcedureParameter.COUNT, SqlDbType.Int).Direction = ParameterDirection.Output;

                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.ORDERS;

                JObject jobj = new JObject();
                if (isdownload == true)
                {
                    ExportToExcel.ExportDsToExcelSheet(_ds, "OrderActivations");
                }
                else
                {
                    this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                    this._helper.ParseDataSet(this._ds, tablePreferences);
                    jobj = this._helper.GetResponse();
                }
                return jobj;

                
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to fetch Orders. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch OnliePaymentGateways. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        internal dynamic VerifyOrderStatuses(long orderId, float activationAmount, bool isActivated, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.VERIFY_ORDER_STATUS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.ORDER_ID, SqlDbType.BigInt).Value = orderId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACTIVATION_AMOUNT, SqlDbType.Float).Value = activationAmount;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ISACTIVATED, SqlDbType.Bit).Value = isActivated;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to verify  OrderStatus. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to verify  OrderStatus. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        public dynamic UpdateUnlimitedActivation(long orderId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.UPDATE_UNLIMITED_ACTIVATION, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.ORDER_ID, SqlDbType.BigInt).Value = orderId;                
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to Update Unlimited Activation. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to Update Unlimited Activation. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        public dynamic GenerateOrderForOnlinePayments(long accountId,int productId,long productAccountId, string metaData, float orderAmount,float taxAmount,float TotalAmount, string paymentGatewayOrderId, string paymentGatewayPaymentid, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GENERATE_ORDER_FOR_ONLINE_PAYMENTS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_ID, SqlDbType.BigInt).Value = accountId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PRODUCT_ID, SqlDbType.Int).Value = productId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PRODUCT_USER_ID, SqlDbType.BigInt).Value = productAccountId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.META_DATA, SqlDbType.VarChar, -1).Value = metaData;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ORDER_AMOUNT, SqlDbType.Float).Value = orderAmount;
                this._sqlCommand.Parameters.Add(ProcedureParameter.TAX, SqlDbType.Float).Value = taxAmount;
                this._sqlCommand.Parameters.Add(ProcedureParameter.TOTAL_AMOUNT, SqlDbType.Float).Value = TotalAmount;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PAYMENT_GATEWAY_ORDER_ID, SqlDbType.VarChar,256).Value = paymentGatewayOrderId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PAYMENT_GATEWAY_PAYMENT_ID, SqlDbType.VarChar, 256).Value = paymentGatewayPaymentid;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if(this._ds.Tables.Count > 0)
                {
                    this._ds.Tables[0].TableName = "InvoiceDetails";
                }
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to Update Unlimited Activation. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to Update Unlimited Activation. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        public dynamic GetRequestObjectForActivation(int quotationId, bool isPostPaidQuotation, float activationAmount, string activationComments,int employeeId)
        {
            dynamic activationObject = new JObject();
            dynamic servicesObject;
            string serviceName = string.Empty;
            bool areMultipleEntriesAllowed = false;
            int quotationServiceId = 0;
            DataRow[] drServiceProperies;
            JToken serviceToken;
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
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
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
                //    if (this._ds.Tables[Label.QUOTATION_SERVICES].Rows.Count != 0)
                //    {
                //        //dtServicesUniqueTable = this._ds.Tables[Label.QUOTATION_SERVICES].DefaultView.ToTable(true, "ServiceId");
                //        for (int quotationServices = 0; quotationServices < this._ds.Tables[Label.QUOTATION_SERVICES].Rows.Count; quotationServices++)
                //        {

                //            serviceName = Convert.ToString(this._ds.Tables[Label.QUOTATION_SERVICES].Rows[quotationServices]["MetaDataCode"]);
                //            areMultipleEntriesAllowed = Convert.ToBoolean(this._ds.Tables[Label.QUOTATION_SERVICES].Rows[quotationServices]["AreMultipleEntriesAllowed"]);
                //            quotationServiceId = Convert.ToInt32(this._ds.Tables[Label.QUOTATION_SERVICES].Rows[quotationServices]["Id"]);
                //            activationObject.Add(new JProperty(serviceName, new JObject()));
                //            activationObject.SelectToken(serviceName).Add(new JProperty(Label.ARE_MULTIPLE_ENTRIES_ALLOWED, areMultipleEntriesAllowed));
                //            serviceToken = activationObject[serviceName];
                //            if (serviceToken.Type != JTokenType.Null)
                //            {
                //                if (areMultipleEntriesAllowed)
                //                {
                //                    activationObject.SelectToken(serviceName).Add(new JProperty(Label.DATA, new JArray()));
                //                }
                //                else
                //                {
                //                    activationObject.SelectToken(serviceName).Add(new JProperty(Label.DATA, new JObject()));
                //                }
                //            }
                //            drServiceProperies = this._ds.Tables[Label.QUOTATION_SERVICE_PROPERTIES].Select(Label.QUOTATION_SERVICE_ID + "=" + quotationServiceId);
                //            foreach (DataRow drServiceProperty in drServiceProperies)
                //            {
                //                activationObject.SelectToken(serviceName).SelectToken(Label.DATA).Add(new JProperty(drServiceProperty["MetaDataCode"].ToString(), drServiceProperty["Value"]));
                //            }
                //        }

                //    }

                //}
                //catch (Exception ex)
                //{

                //}

                //return activationObject;
                if (this._ds.Tables[Label.QUOTATION_SERVICES].Rows.Count != 0)
                {
                    //dtServicesUniqueTable = this._ds.Tables[Label.QUOTATION_SERVICES].DefaultView.ToTable(true, "ServiceId");
                    for (int quotationServices = 0; quotationServices < this._ds.Tables[Label.QUOTATION_SERVICES].Rows.Count; quotationServices++)
                    {
                        serviceName = Convert.ToString(this._ds.Tables[Label.QUOTATION_SERVICES].Rows[quotationServices]["MetaDataCode"]);
                        areMultipleEntriesAllowed = Convert.ToBoolean(this._ds.Tables[Label.QUOTATION_SERVICES].Rows[quotationServices]["AreMultipleEntriesAllowed"]);
                        quotationServiceId = Convert.ToInt32(this._ds.Tables[Label.QUOTATION_SERVICES].Rows[quotationServices]["ServiceId"]);

                        if (activationObject[serviceName] == null)
                        {
                            activationObject.Add(new JProperty(serviceName, new JObject()));
                            activationObject.SelectToken(serviceName).Add(new JProperty(Label.ARE_MULTIPLE_ENTRIES_ALLOWED, areMultipleEntriesAllowed));

                            if (areMultipleEntriesAllowed)
                            {
                                activationObject.SelectToken(serviceName).Add(new JProperty(Label.DATA, new JArray()));
                            }
                            else
                            {
                                activationObject.SelectToken(serviceName).Add(new JProperty(Label.DATA, new JObject()));
                            }
                        }
                        drServiceProperies = this._ds.Tables[Label.QUOTATION_SERVICE_PROPERTIES].Select(Label.SERVICE_ID + "=" + quotationServiceId);
                        JObject servicesData = new JObject();
                        servicesData.Add(new JProperty("QuotationServiceId", quotationServiceId));

                        foreach (DataRow drServiceProperty in drServiceProperies)
                        {
                            servicesData.Add(new JProperty(drServiceProperty["MetaDataCode"].ToString(), drServiceProperty["Value"]));
                        }

                        if (activationObject.SelectToken(serviceName).SelectToken(Label.DATA).Type == JTokenType.Array)
                        {
                            JArray serviceJarray = new JArray();
                            serviceJarray = (JArray)(activationObject.SelectToken(serviceName).SelectToken(Label.DATA));
                            serviceJarray.Add(servicesData);
                            activationObject.SelectToken(serviceName).SelectToken(Label.DATA).Replace(serviceJarray);
                        }
                        else
                        {
                            activationObject.SelectToken(serviceName).SelectToken(Label.DATA).Replace(servicesData);
                        }
                    }

                    JObject ActivationServiceJobj = new JObject();
                    ActivationServiceJobj.Add(new JProperty(Label.PRODUCT_USERID, this._ds.Tables[Label.QUOTATION].Rows[0][Label.PRODUCT_USERID]));
                    ActivationServiceJobj.Add(new JProperty(Label.ACTIVATION_AMOUNT, activationAmount));
                    ActivationServiceJobj.Add(new JProperty(Label.BALANCE_TYPE, this._ds.Tables[Label.QUOTATION].Rows[0][Label.BALANCE_TYPE]));
                    ActivationServiceJobj.Add(new JProperty(Label.ORDER_ID, this._ds.Tables[Label.QUOTATION].Rows[0][Label.ID]));
                    ActivationServiceJobj.Add(new JProperty(Label.COMMENTS, activationComments));
                    ActivationServiceJobj.Add(new JProperty(Label.ACCOUNT_ID, this._ds.Tables[Label.QUOTATION].Rows[0][Label.ACCOUNT_ID]));
                    ActivationServiceJobj.Add(new JProperty(Label.EMPLOYEE_EMAILID, this._ds.Tables[Label.QUOTATION].Rows[0][Label.EMPLOYEE_EMAILID]));
                    ActivationServiceJobj.Add(new JProperty(Label.SERVICES_LIST, activationObject));                    
                    activationObject = new JObject();
                    activationObject = ActivationServiceJobj;
                }

            }
            catch (Exception ex)
            {

            }

            return activationObject;
        }

        public dynamic GetProductDetails(int quotationId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_PRODUCT_DETAILS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.BigInt).Value = quotationId;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to Get ProductDetails. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to Get ProductDetails. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        #endregion
    }
}
