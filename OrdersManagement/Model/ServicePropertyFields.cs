using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    public class ServicePropertyFields
    {

        #region Private Variables

        private short fieldId = 0;
        private string metaDataCode = string.Empty;
        private byte minLength = 0;
        private Int16 maxLength = 0;
        private byte inputTypeId = 0;
        private byte inputDataTypeId = 0;
        private bool isAllowSpecialChars = false;
        private string options = string.Empty;

        #endregion


        #region Public Properties

        public short FieldId { get; set; }
        public string MetaDataCode { get; set; }
        public byte MinLength { get; set; }
        public Int16 MaxLength { get; set; }

        public byte InputTypeId { get; set; }

        public byte InputDataTypeId { get; set; }

        public bool IsAllowSpecialChars { get; set; }

        public string Options { get; set; }


        #endregion
    }
}
