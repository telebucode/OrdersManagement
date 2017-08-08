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
        public dynamic GetServices(short serviceId = 0, bool includeServiceProperties = false, bool onlyActive = true, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.GetServices(serviceId: serviceId, includeServiceProperties: includeServiceProperties, onlyActive: onlyActive, tablePreferences: tablePreferences);
        }
        /// <summary>
        /// Creates a new Service
        /// </summary>
        /// <param name="displayName">DisplayName of the Service.</param>
        /// <param name="metaDataCode">MetaDataCode to use while constructing MetaData.</param>
        /// <param name="areMultipleEntriesAllowed">Indicates whether this Service Supports Multiple Entries</param>
        /// <param name="tablePreferences">See Model.TablePreferences For Details.</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic CreateService(string displayName, string metaDataCode, bool areMultipleEntriesAllowed, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.CreateService(displayName: displayName, metaDataCode: metaDataCode, areMultipleEntriesAllowed: areMultipleEntriesAllowed);
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
        public dynamic UpdateService(byte serviceId, string displayName, string metaDataCode, bool areMultipleEntriesAllowed, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.UpdateService(serviceId, displayName, metaDataCode, areMultipleEntriesAllowed);
        }
        /// <summary>
        /// Deletes a Service
        /// </summary>
        /// <param name="serviceId">Id of the Service to Delete</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic DeleteService(byte serviceId)
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
        public dynamic CreateServiceProperties(byte serviceId, List<ServiceProperty> serviceProperties)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.CreateServiceProperties(serviceId, serviceProperties);
        }
        /// <summary>
        /// Updates a specific ServiceProperty
        /// </summary>
        /// <param name="serviceProperty">ServiceProperty Object</param>
        /// <returns>JSon/Xml Object Depending on the ResponseFormat Set while Initiating the Client Object.</returns>
        public dynamic UpdateServiceProperty(ServiceProperty serviceProperty)
        {
            ServiceClient serviceClient = new ServiceClient(ref this._helper);
            return serviceClient.UpdateServiceProperty(serviceProperty);
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
        public dynamic GetQuotations(int quotationId = 0, string quotationNumber = "", int accountId = 0, int employeeId = 0, int ownerShipId = 0, byte statusId = 0, sbyte channelId = 0, string ipAddress = "", byte billingModeId = 0, Nullable<DateTime> fromDateTime = null, Nullable<DateTime> toDateTime = null, int pageNumber = 1, byte limit = 20, Dictionary<string, TablePreferences> tablePreferences = null)
        {
            QuotationClient client = new QuotationClient(ref this._helper);
            return client.Search(quotationId, quotationNumber, accountId, employeeId, ownerShipId, statusId, channelId, ipAddress, billingModeId, fromDateTime, toDateTime, pageNumber, limit, tablePreferences);
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