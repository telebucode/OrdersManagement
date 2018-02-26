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
using System.Web;
using SelectPdf;

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
        internal dynamic Create(int quotationId, byte billingModeId, int employeeId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.CREATE_INVOICE, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.BILLING_MODE_ID, SqlDbType.TinyInt).Value = billingModeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMPLOYEE_ID, SqlDbType.Int).Value = employeeId;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.INVOICES;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch(Exception e)
            {
                Logger.Error(string.Format("Unable to create Invoice. {0}", e.ToString()));
                throw new InvoiceException(string.Format("Unable to create Invoice. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic ViewInvoice(int quotationId, bool isPostPaidQuotation)
        {
            DataSet tempDataSet = new DataSet();
            string entityName = string.Empty;
            string quotationData = string.Empty;
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.VIEW_OR_DOWNLOAD_INVOICE, this._sqlConnection);
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
        internal dynamic DownloadInvoice(int quotationId, bool isPostPaidQuotation)
        {
            DataSet tempDataSet = new DataSet();
            string entityName = string.Empty;
            string quotationData = string.Empty;
            string filePath = string.Empty;
            string fileName = string.Empty;
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.VIEW_OR_DOWNLOAD_INVOICE, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.HTML, SqlDbType.VarChar, -1).Direction = ParameterDirection.Output;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();

                fileName = "Invoice_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                filePath = HttpContext.Current.Server.MapPath("/Files/Invoices") + '/' + fileName;
                if (ConvertHtmlToPdf(this._sqlCommand.Parameters[ProcedureParameter.HTML].IsDbNull() ? "Null Message" : this._sqlCommand.Parameters[ProcedureParameter.HTML].Value.ToString(), filePath))
                {
                    this._helper.CreateProperty("Success", true);
                    this._helper.CreateProperty("Message", "Success");
                    this._helper.CreateProperty("FilePath", "Files/Invoices/" + fileName);
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
        #endregion

    }
}
