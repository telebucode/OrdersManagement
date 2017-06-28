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
            private List<ServiceProperty> _properties = new List<ServiceProperty>();
        #endregion
        #region PUBLIC PROPERTIES
            public string DisplayName { get { return this._displayName; } set { this._displayName = value; } }
            public string Code { get { return this._code; } set { this._code = value; } }
            public bool AreMultipleAllowed { get { return this._areMultipleAllowed; } set { this._areMultipleAllowed = value; } }
            public List<ServiceProperty> Properties { get { return this._properties; } set { this._properties = value; } }
        #endregion
    }
}
