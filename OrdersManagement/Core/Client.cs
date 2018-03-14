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
    public class Client
    {
        #region PRIVATE VARIABLES

        private string _connectionString = string.Empty;
        private Dictionary<string, Service> _services = new Dictionary<string, Service>();
        private Dictionary<byte, PropertyInputType> _inputTypes = new Dictionary<byte, PropertyInputType>();
        private Dictionary<byte, PropertyDataType> _inputDataTypes = new Dictionary<byte, PropertyDataType>();
        private Helper _helper = null;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Initializes a Client Object.
        /// </summary>
        /// <param name="connectionString">Connection String To Orders Database</param>
        public Client(string connectionString, ResponseFormat responseFormat = ResponseFormat.JSON)
        {
            if (connectionString == null || connectionString.Trim().Length == 0)
                throw new ClientInitializationException("ConnectionString should not be empty.");
            this._connectionString = connectionString;
            this._helper = new Helper(this, responseFormat);
            Logger.InitializeLogger();
        }
        /// <summary>
        /// Initializes a Client Object and tries to read the connection string from application config file with connection string name 'OrdersDbConnectionString'
        /// </summary>
        public Client(ResponseFormat responseFormat = ResponseFormat.JSON)
        {
            if (System.Configuration.ConfigurationManager.ConnectionStrings[Label.CONNECTION_STRING_NAME] == null)
                throw new ClientInitializationException(string.Format("{0} not found in application config file.", Label.CONNECTION_STRING_NAME));
            this._connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[Label.CONNECTION_STRING_NAME].ConnectionString;
            this._helper = new Helper(this, responseFormat);
            Logger.InitializeLogger();
        }

        #endregion

        #region PUBLIC METHODS

        #region SERVICE RELATED

        /// <summary>
        /// Gets The List Of Services
        /// </summary>
        /// <param name="serviceId">If greater than zero, will get corresponding Service Details. Otherwise All Services</param>
        /// <param name="includeServiceProperties">Indicates whether include ServiceProperties in Response.</param>
        /// <param name="onlyActive">Indicates whether to fetch only active services or all the services irrespective of their status.</param>
        /// <param name="tablePreferences">See Model.TablePreferences for details</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic GetServices(byte productId = 0, short serviceId = 0, bool includeServiceProperties = false, bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.GetServices(productId: productId, serviceId: serviceId, includeServiceProperties: includeServiceProperties, onlyActive: onlyActive, tablePreferences: tablePreferences);
        }

        public dynamic GetServiceProperties(short serviceId = 0, bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.GetServiceProperties(serviceId: serviceId, onlyActive: onlyActive, tablePreferences: tablePreferences);
        }
        /// <summary>
        /// Creates a new Service
        /// </summary>
        /// <param name="displayName">DisplayName of the Service.</param>
        /// <param name="metaDataCode">MetaDataCode to use while constructing MetaData.</param>
        /// <param name="areMultipleEntriesAllowed">Indicates whether this Service Supports Multiple Entries</param>
        /// <param name="tablePreferences">See Model.TablePreferences For Details.</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic CreateService(byte productId, string displayName, string metaDataCode, bool areMultipleEntriesAllowed, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.CreateService(productId: productId, displayName: displayName, metaDataCode: metaDataCode, areMultipleEntriesAllowed: areMultipleEntriesAllowed);
        }
        /// <summary>
        /// Updates a Service
        /// </summary>
        /// <param name="serviceId">Id Of Service to Update</param>
        /// <param name="displayName">DisplayName of the Service.</param>
        /// <param name="metaDataCode">MetaDataCode to use while constructing MetaData.</param>
        /// <param name="areMultipleEntriesAllowed">Indicates whether this Service Supports Multiple Entries</param>
        /// <param name="tablePreferences">See Model.TablePreferences For Details.</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic UpdateService(Int16 serviceId, string displayName, string metaDataCode, bool areMultipleEntriesAllowed, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.UpdateService(serviceId, displayName, metaDataCode, areMultipleEntriesAllowed);
        }
        /// <summary>
        /// Deletes a Service
        /// </summary>
        /// <param name="serviceId">Id of the Service to Delete</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic DeleteService(Int16 serviceId)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.DeleteService(serviceId);
        }
        /// <summary>
        /// Creates Properties for a Service
        /// </summary>
        /// <param name="serviceId">Id of the Service to which these Properties should be mapped.</param>
        /// <param name="serviceProperties">ServiceProperties list</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic CreateServiceProperties(Int16 serviceId, List<ServiceProperty> serviceProperties, List<ServicePropertyFields> servicePropertyFields)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.CreateServiceProperties(serviceId, serviceProperties, servicePropertyFields);
        }
        /// <summary>
        /// Updates a specific ServiceProperty
        /// </summary>
        /// <param name="serviceProperty">ServiceProperty Object</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic UpdateServiceProperty(ServiceProperty serviceProperty, List<ServicePropertyFields> servicePropertFields)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.UpdateServiceProperty(serviceProperty, servicePropertFields);
        }
        /// <summary>
        /// Deletes a Specific ServiceProperty
        /// </summary>
        /// <param name="servicePropertyId">Id of the ServiceProperty to delete.</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic DeleteServiceProperty(int servicePropertyId)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.DeleteServiceProperty(servicePropertyId);
        }
        /// <summary>
        /// Gets InputTypes from InputTypes Table
        /// </summary>
        /// <param name="onlyActive">Indicates whether to fetch only active InputTypes or all InputTypes irrespective of their Status</param>
        /// <param name="tablePreferences">See Model.TablePreferences for details</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic GetInputTypes(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.GetInputTypes(onlyActive: onlyActive, tablePreferences: tablePreferences);
        }
        /// <summary>
        /// Gets InputDataTypes from InputDataTypes Table
        /// </summary>
        /// <param name="onlyActive">Indicates whether to fetch only active InputDataTypes or all InputTypes irrespective of their Status</param>
        /// <param name="tablePreferences">See Model.TablePreferences for details</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic GetInputDataTypes(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.GetInputDataTypes(onlyActive: onlyActive, tablePreferences: tablePreferences);
        }

        #endregion // SERVICE RELATED

        #region QUOTATION RELATED
        /// <summary>
        /// Gets the QuotationStatuses
        /// </summary>
        /// <param name="onlyActive">Indicates whether to fetch only active statuses or all statuses irrespective of the status</param>
        /// <param name="tablePreferences">See Model.TablePreferences for details</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic GetQuotationStatuses(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            QuotationClient client = new QuotationClient(ref this._helper);
            return client.GetStatuses(onlyActive, tablePreferences);
        }
        /// <summary>
        /// Used to Get The Channels through which Quotation can be Raised.
        /// </summary>
        /// <param name="onlyActive">Indicates whether to fetch only active statuses or all statuses irrespective of the status</param>
        /// <param name="tablePreferences">See Model.TablePreferences for details</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic GetQuotationChannels(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            QuotationClient client = new QuotationClient(ref this._helper);
            return client.GetChannels(onlyActive, tablePreferences);
        }
        /// <summary>
        /// Used to Search Quotations
        /// </summary>
        /// <param name="quotationId"></param>
        /// <param name="quotationNumber"></param>
        /// <param name="accountId"></param>
        /// <param name="employeeId">EmployeeId by whoom the Quotation Was Raised</param>
        /// <param name="ownerShipId">EmployeeId who owns the accounts mapped with the quotation result set</param>
        /// <param name="statusId"></param>
        /// <param name="channelId"></param>
        /// <param name="ipAddress"></param>
        /// <param name="billingModeId"></param>
        /// <param name="fromDateTime"></param>
        /// <param name="toDateTime"></param>
        /// <param name="pageNumber">Page Number. Default is 1</param>
        /// <param name="limit">Records per page. Default is 20</param>
        /// <param name="tablePreferences">See Model.TablePreferences for details</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic GetQuotations(byte productId = 0, int quotationId = 0, string quotationNumber = "", int accountId = 0, 
            int employeeId = 0, int ownerShipId = 0, byte statusId = 0, sbyte channelId = 0, string ipAddress = "", 
            byte billingModeId = 0, Nullable<DateTime> fromDateTime = null, Nullable<DateTime> toDateTime = null,
            int pageNumber = 1, byte limit = 20, string mobile = "", string email = "",string accountName = "", 
            Dictionary<string, TablePreferences> tablePreferences = null)
        {
            QuotationClient client = new QuotationClient(ref this._helper);
            return client.Search(productId, quotationId, quotationNumber, accountId, employeeId, ownerShipId, statusId, channelId,
                ipAddress, billingModeId, fromDateTime, toDateTime, pageNumber, limit, mobile, email,accountName, tablePreferences);
        }
        /// <summary>
        /// Creates a Quotation
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="employeeId"></param>
        /// <param name="channelId"></param>
        /// <param name="metaData"></param>
        /// <param name="ipAddress"></param>
        /// <param name="stateId"></param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic CreateQuotation(byte productId, int accountId, int employeeId, byte channelId, string metaData, string ipAddress, int stateId)
        {
            QuotationClient client = new QuotationClient(ref this._helper);

            return client.Create(productId, accountId, employeeId, channelId, metaData, ipAddress, stateId);
        }
        /// <summary>
        /// Updates an existing Quotation
        /// </summary>
        /// <param name="quotationId">Id of Quotations Table / PostPaidQuotations Table</param>
        /// <param name="employeeId">EmployeeId who is updating</param>
        /// <param name="metaData">Quotations services metadata</param>
        /// <param name="ipAddress">IpAddress from which request came</param>
        /// <param name="stateId">StateId of the account holder</param>
        /// <returns></returns>
        public dynamic UpdateQuotation(int quotationId, int employeeId, string metaData, string ipAddress, int stateId)
        {
            QuotationClient client = new QuotationClient(ref this._helper);
            return client.Update(quotationId, employeeId, metaData, ipAddress, stateId);
        }
        public dynamic DeleteQuotation(int quotationId, bool isPostPaidQuotation)
        {
            QuotationClient client = new QuotationClient(ref this._helper);
            return client.Delete(quotationId, isPostPaidQuotation);
        }
        public dynamic GetQuotationDetails(int quotationId, bool isPostPaidQuotation)
        {
            QuotationClient client = new QuotationClient(ref this._helper);
            return client.GetQuotationDetails(quotationId, isPostPaidQuotation);
        }

        public dynamic ViewQuotation(int quotationId, bool isPostPaidQuotation)
        {
            QuotationClient client = new QuotationClient(ref this._helper);
            return client.ViewQuotation(quotationId, isPostPaidQuotation);
        }
        public dynamic DownloadQuotation(int quotationId, bool isPostPaidQuotation)
        {
            QuotationClient client = new QuotationClient(ref this._helper);
            return client.DownloadQuotation(quotationId, isPostPaidQuotation);
        }
        public dynamic GetQuotationServices(int quotationId,byte billingModeId, bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            QuotationClient client = new QuotationClient(ref this._helper);
            return client.GetQuotationServices(quotationId:quotationId,billingModeId:billingModeId,onlyActive:onlyActive,tablePreferences: tablePreferences);
        }

        public dynamic GetQuotationServiceProperties(int quotationId, byte billingModeId, bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            QuotationClient client = new QuotationClient(ref this._helper);
            return client.GetQuotationServiceProperties(quotationId: quotationId, billingModeId: billingModeId, onlyActive: onlyActive, tablePreferences: tablePreferences);
        }

        #endregion

        #region INVOICE RELATED
        /// <summary>
        /// Gets InvoiceStatuses
        /// </summary>
        /// <param name="onlyActive">Indicates whether to fetch only active statuses or all statuses irrespective of the status</param>
        /// <param name="tablePreferences">See Model.TablePreferences for details</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic GetInvoiceStatuses(bool onlyActive = false, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            InvoiceClient client = new InvoiceClient(ref this._helper);
            return client.GetStatuses(onlyActive, tablePreferences);
        }
        public dynamic CreateInvoice(int quotationId, byte billingModeId, int employeeId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            InvoiceClient client = new InvoiceClient(ref this._helper);
            return client.Create(quotationId: quotationId, billingModeId: billingModeId, employeeId: employeeId, tablePreferences: tablePreferences);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="quotationId"></param>
        /// <param name="isPostPaidQuotation"></param>
        /// <returns></returns>
        public dynamic ViewInvoice(int quotationId, bool isPostPaidQuotation)
        {
            InvoiceClient client = new InvoiceClient(ref this._helper);
            return client.ViewInvoice(quotationId, isPostPaidQuotation);
        }
        public dynamic DownloadInvoice(int quotationId, bool isPostPaidQuotation)
        {
            InvoiceClient client = new InvoiceClient(ref this._helper);
            return client.DownloadInvoice(quotationId, isPostPaidQuotation);
        }

        public dynamic GetInvoices(byte productId = 0, int invoiceId = 0, string quotationNumber = "", int accountId = 0, int employeeId = 0, int ownerShipId = 0, byte statusId = 0, sbyte channelId = 0, string ipAddress = "", byte billingModeId = 0, Nullable<DateTime> fromDateTime = null, Nullable<DateTime> toDateTime = null, int pageNumber = 1, byte limit = 20, string mobile = "", string email = "", string accountName = "", Dictionary<string, TablePreferences> tablePreferences = null)
        {
            InvoiceClient client = new InvoiceClient(ref this._helper);
            return client.Search(productId, invoiceId, quotationNumber, accountId, employeeId, ownerShipId, statusId, channelId,
                ipAddress, billingModeId, fromDateTime, toDateTime, pageNumber, limit, mobile, email,accountName, tablePreferences);
        }

        #endregion

        #region Payments RELATED

        public dynamic GetBankAccounts(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            PaymentClient paymentsClient = new PaymentClient(ref this._helper);
            return paymentsClient.GetBankAccounts(onlyActive: onlyActive, tablePreferences: tablePreferences);
        }
        public dynamic GetPaymentGateways(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            PaymentClient paymentsClient = new PaymentClient(ref this._helper);
            return paymentsClient.GetPaymentGateways(onlyActive: onlyActive, tablePreferences: tablePreferences);
        }
        public dynamic CreatePayment(int productId, int accountId, int employeeId, int invoiceId, int billingModeId,
            int paymentGatewayId, float paymentAmount, int bankAccountId, DateTime depositeDate, int activatePercentage,
            string comments, bool isTDSApplicable, int tdsPercentage, string chequeNumber, string attachments, string transactionNumber,
            string clientAccountNumber, string clientAccountName, string clientBankName, string clientBankBranch, int onlinePaymentGatewayId,
            string paymentGatewayReferenceId)
        {
            PaymentClient paymentsClient = new PaymentClient(ref this._helper);
            return paymentsClient.CreatePayment(productId: productId, accountId: accountId, employeeId: employeeId,
                invoiceId: invoiceId, billingModeId: billingModeId, paymentGatewayId: paymentGatewayId, paymentAmount: paymentAmount,
                bankAccountId: bankAccountId, depositeDate: depositeDate, activatePercentage: activatePercentage,
                comments: comments, isTDSApplicable: isTDSApplicable, tdsPercentage: tdsPercentage, chequeNumber: chequeNumber,
                attachments: attachments, transactionNumber: transactionNumber, clientAccountNumber: clientAccountNumber,
                clientAccountName: clientAccountName, clientBankName: clientBankName, clientBankBranch: clientBankBranch, onlinePaymentGatewayId: onlinePaymentGatewayId, paymentGatewayReferenceId: paymentGatewayReferenceId);
        }
        public dynamic GetOnlinePaymentGateways(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            PaymentClient paymentsClient = new PaymentClient(ref this._helper);
            return paymentsClient.GetOnlinePaymentGateways(onlyActive: onlyActive, tablePreferences: tablePreferences);
        }
        public dynamic GetPaymentStatuses(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            PaymentClient paymentsClient = new PaymentClient(ref this._helper);
            return paymentsClient.GetPaymentStatuses(onlyActive: onlyActive, tablePreferences: tablePreferences);
        }


        public dynamic GetPayments(byte productId, int accountId, string mobile, string email, int paymentStatus, string number, byte billingMode, DateTime fromDateTime, DateTime toDateTime, string accountName, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            PaymentClient paymentsClient = new PaymentClient(ref this._helper);
            return paymentsClient.GetPayments(productId: productId, accountId: accountId, mobile: mobile, email: email,
                paymentStatus: paymentStatus, number: number, billingMode: billingMode, fromDateTime: fromDateTime, toDateTime: toDateTime,
                accountName: accountName, tablePreferences: tablePreferences);
        }

        public dynamic GetPaymentDetails(byte productId, int orderId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            PaymentClient paymentsClient = new PaymentClient(ref this._helper);
            return paymentsClient.GetPaymentDetails(productId: productId, orderId: orderId, tablePreferences: tablePreferences);
        }
        public dynamic VerifyPaymentStatuses(long orderId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            PaymentClient paymentsClient = new PaymentClient(ref this._helper);
            return paymentsClient.VerifyPaymentStatuses(orderId: orderId, tablePreferences: tablePreferences);
        }

        #endregion

        #region Orders Related

        public dynamic GetOrderSummary(int quotationId, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            OrdersClient ordersClient = new OrdersClient(ref this._helper);
            return ordersClient.GetOrderSummary(quotationId: quotationId, tablePreferences: tablePreferences);
        }
        public dynamic GetOrderStatuses(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            OrdersClient ordersClient = new OrdersClient(ref this._helper);
            return ordersClient.GetOrderStatuses(onlyActive: onlyActive, tablePreferences: tablePreferences);
        }

        public dynamic GetOrders(byte productId, int accountId, string mobile, string email, int orderStatus, string number, byte billingMode, DateTime fromDateTime, DateTime toDateTime, string accountName, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            OrdersClient paymentsClient = new OrdersClient(ref this._helper);
            return paymentsClient.GetOrders(productId: productId, accountId: accountId, mobile: mobile, email: email,
                orderStatus: orderStatus, number: number, billingMode: billingMode, fromDateTime: fromDateTime, toDateTime: toDateTime,
                accountName:accountName,tablePreferences: tablePreferences);
        }
        public dynamic ActivateOrder(string activationUrl,string metaData,Dictionary<string, TablePreferences> tablePreferences = null)
        {
            OrdersClient ordersClient = new OrdersClient(ref this._helper);
            return ordersClient.ActivateOrder(activationUrl:activationUrl,metaData:metaData, tablePreferences: tablePreferences);
        }
        

        #endregion

        // Products Related
        #region
        public dynamic GetProducts(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            ProductsClient serviceClient = new ProductsClient(ref this._helper);
            return serviceClient.GetProducts(onlyActive: onlyActive, tablePreferences: tablePreferences);
        }
        #endregion

        #region GENERIC

        public dynamic GetBillingModes(bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            GenericClient client = new GenericClient(ref this._helper);
            return client.GetBillingModes(onlyActive, tablePreferences);
        }

        #endregion

        #endregion // PUBLIC METHODS

        #region INTERNAL PROPERTIES

        internal string ConnectionString { get { return this._connectionString; } set { this._connectionString = value; } }

        #endregion

    }
}