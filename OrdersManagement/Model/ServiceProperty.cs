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

        private short _id = 0;
        private string _displayName = string.Empty;
        private string _metaDataCode = string.Empty;
        private bool _isRequired = true;
        private dynamic _defaultValue = null;
        private PropertyInputType _inputType = PropertyInputType.UNKNOWN;
        private byte _inputTypeId = 0;
        private byte _dataTypeId = 0;
        private PropertyDataType _dataType = PropertyDataType.UNKNOWN;
        private bool _isActive = true;
        private bool _includeInOrderAmount = false;

        #endregion

        #region PUBLIC PROPERTIES
        /// <summary>
        /// Gets Or Sets the Id associated with this property in ServiceProperties Table.
        /// </summary>
        public short Id { get { return this._id; } set { this._id = value; } }
        /// <summary>
        /// Gets Or Sets the DisplayName of the Property
        /// </summary>
        public string DisplayName { get { return this._displayName; } set { this._displayName = value; } }
        /// <summary>
        /// Gets Or Sets the Code to be used
        /// </summary>
        public string MetaDataCode { get { return this._metaDataCode; } set { this._metaDataCode = value; } }
        /// <summary>
        /// Gets Or Sets the value indicating whether this Property is required or not. Default is true.
        /// </summary>
        public bool IsRequired { get { return this._isRequired; } set { this._isRequired = value; } }
        /// <summary>
        /// Gets Or Sets the Default Value of this Property
        /// </summary>
        public dynamic DefaultValue { get { return this._defaultValue; } set { this._defaultValue = value; } }
        /// <summary>
        /// Gets Or Sets the Input Type for this Service Property. See InputType enum for details.
        /// </summary>
        public PropertyInputType InputType { get { return this._inputType; } set { this._inputType = value; } }
        public byte InputTypeId { get { return this._inputTypeId; } set { this._inputTypeId = value; } }
        public byte DataTypeId { get { return this._dataTypeId; } set { this._dataTypeId = value; } }
        /// <summary>
        /// Gets Or Sets the Input Data Type (What type of data should be accepted in the Input Field). See InputDataType enum for details.
        /// </summary>
        public PropertyDataType DataType { get { return this._dataType; } set { this._dataType = value; } }
        /// <summary>
        /// Gets Or Sets the value indicating whether this property is Active or not.
        /// </summary>
        public bool IsActive { get { return this._isActive; } set { this._isActive = value; } }
        /// <summary>
        /// Gets Or Sets the valud indicating whether this valud should include in Order Amount.
        /// </summary>
        public bool IncludeInOrderAmount { get { return this._includeInOrderAmount; } set { this._includeInOrderAmount = value; } }

        #endregion
    }
}
