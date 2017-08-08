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
        internal dynamic Create(int accountId, byte channelId, string metaData, string ipAddress, int stateId, decimal orderAmount, int employeeId)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.CREATE_QUOTATION, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_ID, SqlDbType.Int).Value = accountId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMPLOYEE_ID, SqlDbType.Int).Value = employeeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.CHANNEL_ID, SqlDbType.TinyInt).Value = channelId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.META_DATA, SqlDbType.VarChar, 2000).Value = metaData;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IP_ADDRESS, SqlDbType.VarChar, 50).Value = ipAddress;
                this._sqlCommand.Parameters.Add(ProcedureParameter.STATE_ID, SqlDbType.Int).Value = stateId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ORDER_AMOUNT, SqlDbType.Decimal).Value = orderAmount;
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
        #endregion
    }
}
