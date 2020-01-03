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

        internal dynamic Search(int productId = 0, int invoiceId = 0, string quotationNumber = "", int accountId = 0, int employeeId = 0,
            int ownerShipId = 0, byte statusId = 0, sbyte channelId = 0, string ipAddress = "", byte billingModeId = 0,
            Nullable<DateTime> fromDateTime = null, Nullable<DateTime> toDateTime = null, int pageNumber = 1, byte limit = 20, 
            string mobile = "", string email = "",string accountName = "",Dictionary<string, TablePreferences> tablePreferences = null, bool isdownload = false,bool isProformaInvoice=false)
        {
            try
            {
                if(isProformaInvoice==false)
                    this._sqlCommand = new SqlCommand(StoredProcedure.GET_INVOICES, this._sqlConnection);
                else
                    this._sqlCommand = new SqlCommand(StoredProcedure.GET_PROFORMA_INVOICES, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.PRODUCT_ID, SqlDbType.Int).Value = productId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.INVOICEID, SqlDbType.Int).Value = invoiceId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_NUMBER, SqlDbType.VarChar, 20).Value = quotationNumber;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_ID, SqlDbType.Int).Value = accountId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_NAME, SqlDbType.VarChar, 128).Value = accountName;
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
                this._sqlCommand.Parameters.Add(ProcedureParameter.COUNT, SqlDbType.Int).Direction = ParameterDirection.Output;
                this._sqlCommand.Parameters.Add(ProcedureParameter.LIMIT, SqlDbType.TinyInt).Value = limit;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ISDOWNLOAD, SqlDbType.Bit).Value = isdownload;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.INVOICES;
                JObject jobj = new JObject();
                if (isdownload == true)
                {
                    ExportToExcel.ExportDsToExcelSheet(_ds, "Invoices");                   
                }
                else
                {
                    this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                    this._helper.ParseDataSet(this._ds, tablePreferences);                   
                    jobj = this._helper.GetResponse();;
                }
                return jobj;
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
        internal dynamic Create(int quotationId, byte billingModeId, int employeeId,bool isProformaInvoice, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.CREATE_INVOICE, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.BILLING_MODE_ID, SqlDbType.TinyInt).Value = billingModeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMPLOYEE_ID, SqlDbType.Int).Value = employeeId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ISPROFORMAINVOICE, SqlDbType.Bit).Value = isProformaInvoice;
                this._sqlCommand.Parameters.Add(ProcedureParameter.INVOICEID, SqlDbType.BigInt).Direction = ParameterDirection.Output;
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

        //cancel Invoice 
        internal dynamic CancelInvoice(int quotationId, int adminId)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.CANCEL_INVOICE, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ADMIN_ID, SqlDbType.Int).Value = adminId;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                //this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to cancel Invoice. {0}", e.ToString()));
                throw new InvoiceException(string.Format("Unable to cancel Invoice. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        internal dynamic GetInvoiceAccountDetails(int invoiceId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_INVOICE_ACCOUNT_DETAILS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.INVOICEID, SqlDbType.BigInt).Value = invoiceId;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 0)
                    this._ds.Tables[0].TableName = Label.INVOICE_ACCOUNT_DETAILS;
                this._ds.Tables.Add(this._helper.ConvertOutputParametersToDataTable(this._sqlCommand.Parameters));
                this._helper.ParseDataSet(this._ds, tablePreferences);
                return this._helper.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Unable to fetch Invoice Account Details. {0}", e.ToString()));
                throw new InvoiceException(string.Format("Unable to fetch Invoice Account Details. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }

        internal dynamic UpdateInvoice(int invoiceId, string mobile, string email, string address, string GSTIN, string companyName, int stateId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.UPDATE_INVOICE, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.INVOICEID, SqlDbType.BigInt).Value = invoiceId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.MOBILE, SqlDbType.VarChar, 15).Value = mobile;
                this._sqlCommand.Parameters.Add(ProcedureParameter.EMAIL, SqlDbType.VarChar, 200).Value = email;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ADDRESS, SqlDbType.NVarChar, 1000).Value = address;
                this._sqlCommand.Parameters.Add(ProcedureParameter.GSTIN, SqlDbType.VarChar, 15).Value = GSTIN;
                this._sqlCommand.Parameters.Add(ProcedureParameter.COMPANY, SqlDbType.NVarChar, 200).Value = companyName;
                this._sqlCommand.Parameters.Add(ProcedureParameter.STATE_ID, SqlDbType.Int).Value = stateId;
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
                Logger.Error(string.Format("Unable to Update Invoice. {0}", e.ToString()));
                throw new InvoiceException(string.Format("Unable to Update Invoice. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
        }
        internal dynamic ViewInvoice(int quotationId, bool isPostPaidQuotation, bool isProformaInvoice)
        {
            DataSet tempDataSet = new DataSet();
            string entityName = string.Empty;
            string quotationData = string.Empty;
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.VIEW_OR_DOWNLOAD_INVOICE, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_POSTPAID_QUOTATION, SqlDbType.Bit).Value = isPostPaidQuotation;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ISPROFORMAINVOICE, SqlDbType.Bit).Value = isProformaInvoice;
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

        internal dynamic GenerateTaxInvoice(int invoiceId,int adminId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            DataSet tempDataSet = new DataSet();
            string entityName = string.Empty;
            string invoiceData = string.Empty;
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GENERATE_TAX_INVOICE, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.INVOICEID, SqlDbType.Int).Value = invoiceId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ADMIN_ID, SqlDbType.Int).Value = adminId;                                
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
            catch (Exception e)
            {
                invoiceData = "";
                Logger.Error(string.Format("Unable to get Generate Sale Invoice. {0}", e.ToString()));
                throw new QuotationException(string.Format("Unable to Generate Sale Invoice. {0}", e.Message));
            }
            finally
            {
                this.Clean();
            }
            
        }
        internal dynamic DownloadInvoice(int quotationId, bool isPostPaidQuotation, bool isProformaInvoice)
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
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_POSTPAID_QUOTATION, SqlDbType.Bit).Value = isPostPaidQuotation;
                this._sqlCommand.Parameters.Add(ProcedureParameter.ISPROFORMAINVOICE, SqlDbType.Bit).Value = isProformaInvoice;
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
