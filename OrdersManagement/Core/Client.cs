using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using OrdersManagement.Model;
using OrdersManagement.Exceptions;

namespace OrdersManagement.Core
{
    public class Client
    {
        #region PRIVATE VARIABLES

        private string _connectionString = string.Empty;
        //private List<Model.Service> _services = new List<Model.Service>();
        private Dictionary<string, Service> _services = new Dictionary<string, Service>();
        private Dictionary<byte, PropertyInputType> _inputTypes = new Dictionary<byte, PropertyInputType>();
        private Dictionary<byte, PropertyDataType> _inputDataTypes = new Dictionary<byte, PropertyDataType>();
        private SqlConnection _sqlConnection = null;
        private SqlCommand _sqlCommand = null;
        private SqlDataAdapter _da = null;
        private DataSet _ds = null;
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
            this._helper = new Helper(this);
            this._helper.ResponseFormat = responseFormat;            
            this.Initialize();
        }
        /// <summary>
        /// Initializes a Client Object and tries to read the connection string from application config file with connection string name 'OrdersDbConnectionString'
        /// </summary>
        public Client(ResponseFormat responseFormat = ResponseFormat.JSON)
        {
            if (System.Configuration.ConfigurationManager.ConnectionStrings["OrdersDbConnectionString"] == null)
                throw new ClientInitializationException("OrderDbConnectionString not found in application config file.");
            this._connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["OrdersDbConnectionString"].ConnectionString;
            this._helper = new Helper(this);
            this._helper.ResponseFormat = responseFormat;
            this.Initialize();
        }
        
        #endregion

        #region PRIVATE METHODS

        private void Initialize()
        {
            this.InitializeSqlVariables();
            this.LoadServices();
            this.LoadInputTypes();
            this.LoadInputDataTypes();
            this.LoadServiceProperties();
        }
        private void InitializeSqlVariables()
        {
            this._sqlConnection = new SqlConnection(this._connectionString);
        }

        private void LoadServices(bool isOnlyActive = true)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_SERVICES, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = isOnlyActive;
                this._helper.PopulateOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter();
                this._da.SelectCommand = this._sqlCommand;
                this._ds = new DataSet();
                this._da.Fill(this._ds);
                if (!this._sqlCommand.IsSuccess())
                    throw new ClientInitializationException(string.Format("Procedure Returned False. {0}", this._sqlCommand.GetMessage()));
                if (this._ds.Tables.Count > 0 && this._ds.Tables[0].Rows.Count > 0)
                    this.PopulateServicesList(this._ds.Tables[0]);
                else
                    throw new ClientInitializationException("No service found.");
            }
            catch (SqlException e)
            {
                throw new ClientInitializationException(string.Format("Could not load service list from database. Reason : {0}", e.Message));
            }
            finally
            {
                this._ds = null;
                this._da = null;
            }
        }
        private void LoadServiceProperties(byte serviceId = 0, bool isOnlyActive = true)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_SERVICE_PROPERTIES, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.SERVICE_ID, SqlDbType.TinyInt).Value = serviceId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = isOnlyActive;
                this._helper.PopulateOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter();
                this._da.SelectCommand = this._sqlCommand;
                this._ds = new DataSet();
                this._da.Fill(this._ds);
                if (!this._sqlCommand.IsSuccess())
                    throw new ClientInitializationException(string.Format("Procedure Returned False. {0}", this._sqlCommand.GetMessage()));
                if (this._ds.Tables.Count > 0 && this._ds.Tables[0].Rows.Count > 0)
                    this.PopulateServiceProperties(this._ds.Tables[0]);
                else
                    throw new ClientInitializationException("No service properties found.");
            }
            catch (SqlException e)
            {
                throw new ClientInitializationException(string.Format("Could not load service properties from database. Reason : {0}", e.Message));
            }
            finally
            {
                this._ds = null;
                this._da = null;
            }
        }
        private void LoadInputTypes(bool isOnlyActive = true)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_INPUT_TYPES, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = isOnlyActive;
                this._helper.PopulateOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter();
                this._da.SelectCommand = this._sqlCommand;
                this._ds = new DataSet();
                this._da.Fill(this._ds);
                if (!this._sqlCommand.IsSuccess())
                    throw new ClientInitializationException(string.Format("Procedure Returned False. {0}", this._sqlCommand.GetMessage()));
                if (this._ds.Tables.Count > 0 && this._ds.Tables[0].Rows.Count > 0)
                {
                    this._inputTypes.Clear();
                    foreach (DataRow inputTypeRow in _ds.Tables[0].Rows)
                    {
                        if (inputTypeRow[Label.TYPE].ToString().Equals(Label.TEXT_BOX, StringComparison.CurrentCultureIgnoreCase))
                            this._inputTypes.Add(Convert.ToByte(inputTypeRow[Label.ID]), PropertyInputType.TEXT_BOX);
                        else if (inputTypeRow[Label.TYPE].ToString().Equals(Label.TEXT_AREA, StringComparison.CurrentCultureIgnoreCase))
                            this._inputTypes.Add(Convert.ToByte(inputTypeRow[Label.ID]), PropertyInputType.TEXT_AREA);
                        else
                            throw new ClientInitializationException(string.Format("Input Type is not defined for {0} in Orders Library.", inputTypeRow[Label.TYPE]));
                    }
                }
                else
                    throw new ClientInitializationException("No Input Types found.");
            }
            catch (SqlException e)
            {
                throw new ClientInitializationException(string.Format("Could not load Input Types from database. Reason : {0}", e.Message));
            }
            finally
            {
                this._ds = null;
                this._da = null;
            }
        }
        private void LoadInputDataTypes(bool isOnlyActive = true)
        {
            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_INPUT_DATA_TYPES, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_ONLY_ACTIVE, SqlDbType.Bit).Value = isOnlyActive;
                this._helper.PopulateOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter();
                this._da.SelectCommand = this._sqlCommand;
                this._ds = new DataSet();
                this._da.Fill(this._ds);
                if (!this._sqlCommand.IsSuccess())
                    throw new ClientInitializationException(string.Format("Procedure Returned False. {0}", this._sqlCommand.GetMessage()));
                if (this._ds.Tables.Count > 0 && this._ds.Tables[0].Rows.Count > 0)
                {
                    this._inputDataTypes.Clear();
                    foreach (DataRow inputDataTypeRow in _ds.Tables[0].Rows)
                    {
                        if (inputDataTypeRow[Label.DATA_TYPE].ToString().Equals(Label.INT, StringComparison.CurrentCultureIgnoreCase))
                            this._inputDataTypes.Add(Convert.ToByte(inputDataTypeRow[Label.ID]), PropertyDataType.INT);
                        else if (inputDataTypeRow[Label.DATA_TYPE].ToString().Equals(Label.FLOAT, StringComparison.CurrentCultureIgnoreCase))
                            this._inputDataTypes.Add(Convert.ToByte(inputDataTypeRow[Label.ID]), PropertyDataType.FLOAT);
                        else if (inputDataTypeRow[Label.DATA_TYPE].ToString().Equals(Label.STRING, StringComparison.CurrentCultureIgnoreCase))
                            this._inputDataTypes.Add(Convert.ToByte(inputDataTypeRow[Label.ID]), PropertyDataType.STRING);
                        else if (inputDataTypeRow[Label.DATA_TYPE].ToString().Equals(Label.DATE_TIME, StringComparison.CurrentCultureIgnoreCase))
                            this._inputDataTypes.Add(Convert.ToByte(inputDataTypeRow[Label.ID]), PropertyDataType.DATETIME);
                        else
                            throw new ClientInitializationException(string.Format("Input Data Type is not defined for {0} in Orders Library.", inputDataTypeRow[Label.DATA_TYPE]));
                    }
                }
                else
                    throw new ClientInitializationException("No Input Data Types found.");
            }
            catch (SqlException e)
            {
                throw new ClientInitializationException(string.Format("Could not load Input Data Types from database. Reason : {0}", e.Message));
            }
            finally
            {
                this._ds = null;
                this._da = null;
            }
        }
        private void PopulateServicesList(DataTable servicesTable)
        {
            this._services.Clear();
            foreach (DataRow service in servicesTable.Rows)
            {
                Service serviceObj = new Service();
                serviceObj.Id = Convert.ToByte(service[Label.ID]);
                serviceObj.DisplayName = service[Label.DISPLAY_NAME].ToString();
                serviceObj.MetaDataCode = service[Label.META_DATA_CODE].ToString();
                if (!service[Label.ARE_MULTIPLE_ENTRIES_ALLOWED].IsDbNull())
                    serviceObj.AreMultipleAllowed = Convert.ToBoolean(service[Label.ARE_MULTIPLE_ENTRIES_ALLOWED].ToString());
                serviceObj.IsActive = Convert.ToBoolean(service[Label.IS_ACTIVE].ToString());
                this._services.Add(serviceObj.MetaDataCode, serviceObj);
            }
        }
        private void PopulateServiceProperties(DataTable servicePropertiesTable)
        {
            foreach (KeyValuePair<string, Service> service in this._services)
            {
                service.Value.Properties.Clear();
                foreach (DataRow servicePropertyRow in servicePropertiesTable.Select(string.Format("ServiceId = {0}", service.Value.Id)))
                {
                    ServiceProperty serviceProperty = new ServiceProperty();
                    serviceProperty.Id = Convert.ToSByte(servicePropertyRow[Label.ID]);
                    serviceProperty.DisplayName = servicePropertyRow[Label.DISPLAY_NAME].ToString();
                    serviceProperty.MetaDataCode = servicePropertyRow[Label.META_DATA_CODE].ToString();
                    serviceProperty.IsRequired = Convert.ToBoolean(servicePropertyRow[Label.IS_REQUIRED].ToString());
                    if(!servicePropertyRow[Label.DEFAULT_VALUE].IsDbNull())
                        serviceProperty.DefaultValue = servicePropertyRow[Label.DEFAULT_VALUE].ToString();
                    serviceProperty.IsActive = Convert.ToBoolean(servicePropertyRow[Label.IS_ACTIVE].ToString());
                    serviceProperty.IncludeInOrderAmount = Convert.ToBoolean(servicePropertyRow[Label.INCLUDE_IN_ORDER_AMOUNT].ToString());
                    if (!this._inputTypes.ContainsKey(Convert.ToByte(servicePropertyRow[Label.INPUT_TYPE_ID])))
                        throw new ClientInitializationException(string.Format("Input Type Id {0} not Supported", servicePropertyRow[Label.INPUT_TYPE_ID]));
                    else
                        serviceProperty.InputType = this._inputTypes[Convert.ToByte(servicePropertyRow[Label.INPUT_TYPE_ID])];
                    if (!this._inputDataTypes.ContainsKey(Convert.ToByte(servicePropertyRow[Label.INPUT_DATA_TYPE_ID])))
                        throw new ClientInitializationException(string.Format("Input Data Type Id {0} not Supported", servicePropertyRow[Label.INPUT_DATA_TYPE_ID]));
                    else
                        serviceProperty.DataType = this._inputDataTypes[Convert.ToByte(servicePropertyRow[Label.INPUT_DATA_TYPE_ID])];
                    service.Value.Properties.Add(servicePropertyRow[Label.META_DATA_CODE].ToString(), serviceProperty);
                }
            }
        }        

        #endregion

        #region PUBLIC METHODS
        
        public dynamic GetServices(bool includeServiceProperties = false, bool isOnlyActive = true)
        {
            return this._helper.GetServices(includeServiceProperties, isOnlyActive);
        }

        #endregion

        #region PROPERTIES

        internal Dictionary<string, Service> Services { get { return this._services; } }

        #endregion

    }
}