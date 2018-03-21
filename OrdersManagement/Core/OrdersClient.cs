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
        internal dynamic ActivateOrder(string metaData,string activationUrl,Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                CredentialCache credentials = new CredentialCache();
                credentials.Add(uriPrefix: new Uri(activationUrl), authType: "Basic", cred: new NetworkCredential("smscountry", "smsc"));
                WebRequest request = (HttpWebRequest)WebRequest.Create(activationUrl);
                var data = Encoding.ASCII.GetBytes(metaData);
                request.Method = "POST";
                request.Credentials = credentials;
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return JObject.Parse(responseString);
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
            string number, byte billingMode, DateTime fromDateTime, DateTime toDateTime, string accountName, int pageNumber,byte limit,
            Dictionary<string, TablePreferences> tablePreferences = null)
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
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
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

        #endregion
    }
}
