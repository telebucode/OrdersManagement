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

namespace OrdersManagement.Core
{
    internal class InvoiceClient
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
        internal InvoiceClient(ref Helper helper)
        {
            this._helper = helper;
            this._sqlConnection = new SqlConnection(this._helper.ConnectionString);
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
        internal dynamic GetStatuses(bool onlyActive = false, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_INVOICE_STATUSES, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = onlyActive;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.INVOICE_STATUSES;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch(Exception e)
            {
                Logger.Error(string.Format("Unable to fetch InvoiceStatuses. {0}", e.ToString()));
                throw new InvoiceException(string.Format("Unable to fetch QuotationStatuses. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        #endregion

    }
}
