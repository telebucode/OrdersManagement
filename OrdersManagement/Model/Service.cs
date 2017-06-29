using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    public class Service
    {
        #region PRIVATE VARIABLES

        private string _displayName = string.Empty;
        private string _code = string.Empty;
        private bool _areMultipleAllowed = false;
        //private List<ServiceProperty> _properties = new List<ServiceProperty>();
        private Dictionary<string, ServiceProperty> _properties = new Dictionary<string, ServiceProperty>();

        #endregion

        #region PUBLIC PROPERTIES            

        /// <summary>
        /// Gets Or Sets the DisplayName of the Service to display in the UI.
        /// </summary>
        public string DisplayName { get { return this._displayName; } set { this._displayName = value; } }
        /// <summary>
        /// Gets Or Sets the Code linked to this service to populate in the MetaData.
        /// </summary>
        public string Code { get { return this._code; } set { this._code = value; } }
        /// <summary>
        /// Gets Or Sets the value indicating whether this Service support Multiple Entries
        /// </summary>
        public bool AreMultipleAllowed { get { return this._areMultipleAllowed; } set { this._areMultipleAllowed = value; } }
        /// <summary>
        /// Gets Or Sets the list of properties assosiated with this Service.
        /// </summary>
        public Dictionary<string, ServiceProperty> Properties { get { return this._properties; } set { this._properties = value; } }

        #endregion
    }
}
