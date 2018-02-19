using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;

namespace OrdersManagement.Model
{
    public class Service
    {
        #region PRIVATE VARIABLES

        private Int16 _id = 0;
        private byte _productId = 0;
        private string _displayName = string.Empty;
        private string _metaDataCode = string.Empty;
        private bool _areMultipleEntriesAllowed = false;
        private bool _isActive = true;
        //private List<ServiceProperty> _properties = new List<ServiceProperty>();
        private Dictionary<string, ServiceProperty> _properties = new Dictionary<string, ServiceProperty>();

        #endregion

        #region PUBLIC PROPERTIES
        /// <summary>
        /// Gets Or Sets the Id associated with this service in Services Table.
        /// </summary>
        public Int16 Id { get { return this._id; } set { this._id = value; } }
        /// Gets Or Sets the ID asscoiated with the product for the particular service
        public byte ProductId { get { return this._productId; } set { this._productId = value; } }        /// <summary>
        /// Gets Or Sets the DisplayName of the Service to display in the UI.
        /// </summary>
        public string DisplayName { get { return this._displayName; } set { this._displayName = value; } }
        /// <summary>
        /// Gets Or Sets the Code linked to this service to populate in the MetaData.
        /// </summary>
        public string MetaDataCode { get { return this._metaDataCode; } set { this._metaDataCode = value; } }
        /// <summary>
        /// Gets Or Sets the value indicating whether this Service support Multiple Entries
        /// </summary>
        public bool AreMultipleEntriesAllowed { get { return this._areMultipleEntriesAllowed; } set { this._areMultipleEntriesAllowed = value; } }
        /// <summary>
        /// Gets Or Sets the value indicating whether this service is active or not.
        /// </summary>
        public bool IsActive { get { return this._isActive; } set { this._isActive = value; } }
        /// <summary>
        /// Gets Or Sets the list of properties assosiated with this Service.
        /// </summary>
        public Dictionary<string, ServiceProperty> Properties { get { return this._properties; } set { this._properties = value; } }

        #endregion

        #region PUBLIC METHODS
        /// <summary>
        /// Create a Service
        /// </summary>
        /// <param name="responseFormat">Indicates In which format the response object should be.</param>
        /// <returns>JSon/Xml Object Depending on the responseFormat Parameter</returns>
        public dynamic Create(ResponseFormat responseFormat = ResponseFormat.JSON)
        {
            Core.Client client = new Core.Client(responseFormat);
            return client.CreateService(this._productId, this._displayName, this._metaDataCode, this._areMultipleEntriesAllowed);
        }
        /// <summary>
        /// Updates a Service
        /// </summary>
        /// <param name="responseFormat">Indicates In which format the response object should be.</param>
        /// <returns>JSon/Xml Object Depending on the responseFormat Parameter</returns>
        public dynamic Update(ResponseFormat responseFormat = ResponseFormat.JSON)
        {
            Core.Client client = new Core.Client(responseFormat);
            return client.UpdateService(this._id, this._displayName, this._metaDataCode, this._areMultipleEntriesAllowed);
        }
        /// <summary>
        /// Deletes a Service
        /// </summary>
        /// <param name="responseFormat">Indicates In which format the response object should be.</param>
        /// <returns>JSon/Xml Object Depending on the responseFormat Parameter</returns>
        public dynamic Delete(ResponseFormat responseFormat = ResponseFormat.JSON)
        {
            Core.Client client = new Core.Client(responseFormat);
            return client.DeleteService(this._id);
        }

        #endregion

    }
}
