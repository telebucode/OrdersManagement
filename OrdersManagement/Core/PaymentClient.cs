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

namespace OrdersManagement.Core
{
    internal class PaymentClient
    {
        private SqlConnection _sqlConnection = null;
        private SqlCommand _sqlCommand = null;
        private SqlDataAdapter _da = null;
        private DataSet _ds = null;
        private Helper _helper = null;
        internal PaymentClient(ref Helper helper)
        {
            this._helper = helper;
            this._sqlConnection = new SqlConnection(this._helper.ConnectionString);
        }

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
        internal dynamic GetBankAccounts(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_BANK_ACCOUNTS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = onlyActive;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.BANK_ACCOUNTS;
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
        internal dynamic GetPaymentGateways(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_PAYMENT_GATEWAYS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = onlyActive;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.PAYMENT_GATEWAYS;
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
        internal dynamic GetOnlinePaymentGateways(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_ONLINE_PAYMENT_GATEWAYS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = onlyActive;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.ONLINE_PAYMENT_GATEWAYS;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to fetch OnliePaymentGateways. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch OnliePaymentGateways. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic GetPaymentStatuses(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_PAYMENT_STATUSES, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = onlyActive;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.PAYMENT_STATUSES;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to fetch OnliePaymentGateways. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch OnliePaymentGateways. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }


        public dynamic CreatePayment(int productId, int accountId, int employeeId, int invoiceId, int billingModeId,
            int paymentGatewayId, float paymentAmount, int bankAccountId, DateTime depositeDate, DateTime dueDate,
            string comments, bool isTDSApplicable, int tdsPercentage, string chequeNumber, string attachments, string transactionNumber,
            string clientAccountNumber, string clientAccountName, string clientBankName, string clientBankBranch,
            int onlinePaymentGatewayId, string paymentGatewayReferenceId,
            Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {

                this._sqlCommand = new SqlCommand(StoredProcedure.CREATE_PAYMENT, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_ID, SqlDbType.Int).Value = accountId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PRODUCT_ID, SqlDbType.Int).Value = productId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMPLOYEE_ID, SqlDbType.Int).Value = employeeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.INVOICEID, SqlDbType.Int).Value = invoiceId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.BILLING_MODE_ID, SqlDbType.TinyInt).Value = billingModeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PAYMENT_GATEWAY_ID, SqlDbType.TinyInt).Value = paymentGatewayId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PAYMENT_AMOUNT, SqlDbType.Float).Value = paymentAmount;
                this._sqlCommand.Parameters.Add(ProcedureParameter.BANK_ACCOUNT_ID, SqlDbType.Float).Value = bankAccountId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.DEPOSIT_DATE, SqlDbType.DateTime).Value = depositeDate;
                this._sqlCommand.Parameters.Add(ProcedureParameter.DUE_DATE, SqlDbType.DateTime).Value = dueDate;
                //this._sqlCommand.Parameters.Add(ProcedureParameter.ACTIVATE_PERCENTAGE, SqlDbType.Int).Value = activatePercentage;
                //this._sqlCommand.Parameters.Add(ProcedureParameter.ACTIVATION_AMOUNT, SqlDbType.Int).Value = activationAmount;
                this._sqlCommand.Parameters.Add(ProcedureParameter.COMMENTS, SqlDbType.VarChar, -1).Value = comments;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_TDS_APPLICABLE, SqlDbType.Bit).Value = isTDSApplicable;
                this._sqlCommand.Parameters.Add(ProcedureParameter.TDS_Percentage, SqlDbType.Int).Value = tdsPercentage;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ATTACHMENTS, SqlDbType.VarChar, 1000).Value = attachments;
                this._sqlCommand.Parameters.Add(ProcedureParameter.CHEQUE_NUMBER, SqlDbType.VarChar, 64).Value = chequeNumber;
                this._sqlCommand.Parameters.Add(ProcedureParameter.TRANSACTION_NUMBER, SqlDbType.VarChar, 50).Value = transactionNumber;
                this._sqlCommand.Parameters.Add(ProcedureParameter.CLIENT_ACCOUNT_NUMBER, SqlDbType.VarChar, 32).Value = clientAccountNumber;
                this._sqlCommand.Parameters.Add(ProcedureParameter.CLIENT_ACCOUNT_NAME, SqlDbType.VarChar, 128).Value = clientAccountName;
                this._sqlCommand.Parameters.Add(ProcedureParameter.CLIENT_BANK_NAME, SqlDbType.VarChar, 64).Value = clientBankName;
                this._sqlCommand.Parameters.Add(ProcedureParameter.CLIENT_BANK_BRANCH, SqlDbType.VarChar, 64).Value = clientBankBranch;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ONLINE_PAYMENT_GATEWAY_ID, SqlDbType.TinyInt).Value = onlinePaymentGatewayId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PAYMENT_GATEWAY_REFERENCE_ID, SqlDbType.VarChar, 64).Value = paymentGatewayReferenceId;
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
                Logger.Error(string.Format("Unable to generate payments. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to generate payments. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        internal dynamic GetPayments(byte productId, int accountId, string mobile, string email, int paymentStatus,
            string number, byte billingMode, int pageNumber, byte limit, DateTime fromDateTime, DateTime toDateTime, string accountName,
            Dictionary<string, TablePreferences> tablePreferences = null, bool isdownload = false)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_PAYMENTS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.PRODUCT_ID, SqlDbType.TinyInt).Value = productId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_ID, SqlDbType.Int).Value = accountId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_NAME, SqlDbType.VarChar, 128).Value = accountName;
                this._sqlCommand.Parameters.Add(ProcedureParameter.MOBILE, SqlDbType.VarChar, 15).Value = mobile;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMAIL, SqlDbType.VarChar, 126).Value = email;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PAYMENT_STATUS, SqlDbType.TinyInt).Value = paymentStatus;
                this._sqlCommand.Parameters.Add(ProcedureParameter.NUMBER, SqlDbType.VarChar, 32).Value = number;
                this._sqlCommand.Parameters.Add(ProcedureParameter.BILLING_MODE_ID, SqlDbType.Int).Value = billingMode;
                this._sqlCommand.Parameters.Add(ProcedureParameter.FROM_DATE_TIME, SqlDbType.DateTime).Value = fromDateTime;
                this._sqlCommand.Parameters.Add(ProcedureParameter.TO_DATE_TIME, SqlDbType.DateTime).Value = toDateTime;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ISDOWNLOAD, SqlDbType.Bit).Value = isdownload;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PAGE_NUMBER, SqlDbType.Int).Value = pageNumber;
                this._sqlCommand.Parameters.Add(ProcedureParameter.LIMIT, SqlDbType.TinyInt).Value = limit;
                this._sqlCommand.Parameters.Add(ProcedureParameter.COUNT, SqlDbType.Int).Direction = ParameterDirection.Output;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.PAYMENTS;
                JObject jobj = new JObject();
                if (isdownload == false)
                {
                    this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                    this._helper.ParseDataSet(this._ds, tablePreferences);
                    jobj = this._helper.GetResponse();
                }
                else
                {
                    ExportToExcel.ExportDsToExcelSheet(_ds, "Payments");
                }
                return jobj;
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to fetch OnliePaymentGateways. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch OnliePaymentGateways. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }


        internal dynamic GetPaymentDetails(byte productId, int orderId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_PAYMENT_DETAILS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.ORDER_ID, SqlDbType.Int).Value = orderId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PRODUCT_ID, SqlDbType.TinyInt).Value = productId;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.PAYMENT_DETAILS;
                if (this._ds.Tables.Count > 1)
                    this._ds.Tables[1].TableName = Label.DISTINCT_GATEWAYS;
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
        internal dynamic VerifyPaymentStatuses(long orderId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.VERIFY_PAYMENT_STATUSES, this._sqlConnection);
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
                Logger.Error(string.Format("Unable to verify  VerifyPaymentStatus. {0}", e.ToString()));
                throw new QuotationException(string.Format("nable to verify  VerifyPaymentStatus. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        internal dynamic InitiateRazorpayTransaction(int productId, int productUserId, string userName, string mobile, string email, float rawAmount, float tax, float fee, string orderId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.INITIATE_RAZORPAY_TRANSACTION, this._sqlConnection);                
                this._sqlCommand.Parameters.Add(ProcedureParameter.PRODUCT_ID, SqlDbType.TinyInt).Value = productId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.PRODUCT_USER_ID, SqlDbType.Int).Value = productUserId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.NAME, SqlDbType.VarChar).Value = userName;
                this._sqlCommand.Parameters.Add(ProcedureParameter.MOBILE, SqlDbType.VarChar).Value = mobile;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMAIL, SqlDbType.VarChar).Value = email;
                this._sqlCommand.Parameters.Add(ProcedureParameter.RAW_AMOUNT, SqlDbType.Float).Value = rawAmount;
                this._sqlCommand.Parameters.Add(ProcedureParameter.TAX, SqlDbType.Float).Value = tax;
                this._sqlCommand.Parameters.Add(ProcedureParameter.FEE, SqlDbType.Float).Value = fee;
                this._sqlCommand.Parameters.Add(ProcedureParameter.RAZORPAY_ORDER_ID, SqlDbType.VarChar).Value = orderId;
                
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
                Logger.Error(string.Format("Unable to Initiate Razorpay transaction logging {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to Initiate Razorpay transaction logging. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        internal dynamic UpdateRazorpayResponse(int Id, string orderId, string paymentId, string signature, int status, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.UPDATE_RAZORPAY_RESPONSE, this._sqlConnection);                
                this._sqlCommand.Parameters.Add(ProcedureParameter.TABLE_ID, SqlDbType.BigInt).Value = Id;
                this._sqlCommand.Parameters.Add(ProcedureParameter.RAZORPAY_ORDER_ID, SqlDbType.VarChar).Value = orderId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.RAZORPAY_PAYMENT_ID, SqlDbType.VarChar).Value = paymentId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.RAZORPAY_SIGNATURE, SqlDbType.VarChar).Value = signature;
                this._sqlCommand.Parameters.Add(ProcedureParameter.RAZORPAY_STATUS, SqlDbType.TinyInt).Value = status;
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
                Logger.Error(string.Format("Unable to save Razorpay response {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to save Razorpay response. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        #endregion

        internal dynamic PaymentStatuses(Dictionary<string, TablePreferences> tablePreferences)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.Get_And_Update_Payment_Statuses, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.Mode, SqlDbType.TinyInt).Value = 1;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.PAYMENT_STATUSES;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to fetch payment statuses. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch payment statuses. {0}", e.Message));
            }
            finally 
            {
                this.Clean();
            }
        }

        internal dynamic UpdatePaymentStatus(int adminId, long invoiceId, byte statusId, string comment, Dictionary<string, TablePreferences> tablePreferences)
        {
            //ExecuteNonQuery();
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.Get_And_Update_Payment_Statuses, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.Mode, SqlDbType.TinyInt).Value = 2;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ADMIN_ID, SqlDbType.Int).Value = adminId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.INVOICEID, SqlDbType.BigInt).Value = invoiceId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.STATUS_ID, SqlDbType.TinyInt).Value = statusId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.COMMENTS, SqlDbType.VarChar, 500).Value = comment;

                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                //this._da = new SqlDataAdapter(this._sqlCommand);
               // this._da.Fill(this._ds = new DataSet());
                this.  _sqlConnection.Open();
                this._sqlCommand.ExecuteNonQuery();
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                //if (this._ds.tables.count > 0)
                //    this._ds.Tables[0].TableName = Label.PAYMENT_STATUSES;
                this._ds = new DataSet();
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to fetch payment statuses. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to fetch payment statuses. {0}", e.Message));
            }
            finally
            {
                this.Clean();
                this._sqlConnection.Close();
            }
        }
    }
}


