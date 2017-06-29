using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    public class ServiceProperty
    {
        #region PRIVATE VARIABLES

        private string _displayName = string.Empty;
        private string _code = string.Empty;
        private bool _isRequired = true;
        private dynamic _defaultValue = null;
        private PropertyInputType _inputType = PropertyInputType.UNKNOWN;
        private PropertyDataType _dataType = PropertyDataType.UNKNOWN;

        #endregion

        #region PUBLIC PROPERTIES

        /// <summary>
        /// Gets Or Sets the DisplayName of the Property
        /// </summary>
        public string DisplayName { get { return this._displayName; } set { this._displayName = value; } }
        /// <summary>
        /// Gets Or Sets the Code to be used
        /// </summary>
        public string Code { get { return this._code; } set { this._code = value; } }
        /// <summary>
        /// Gets Or Sets the value indicating whether this Property is required or not. Default is true.
        /// </summary>
        public bool IsRequired { get { return this._isRequired; } set { this._isRequired = value; } }
        /// <summary>
        /// Gets Or Sets the Default Value of this Property
        /// </summary>
        public dynamic DefaultValue { get { return this._defaultValue; } set { this._defaultValue = value; } }
        public PropertyInputType InputType { get { return this._inputType; } set { this._inputType = value; } }
        public PropertyDataType DataType { get { return this._dataType; } set { this._dataType = value; } }

        #endregion
    }
}
