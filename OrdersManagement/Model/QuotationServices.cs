using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    class QuotationServices
    {
        #region PRIVATE VARIABLES

        private short id = 0;
        private int quotationId = 0;
        private short serviceId = 0;
        private string metaDataCode = string.Empty;
        private string extraCharges = string.Empty;
        private byte occurance = 0;

        #endregion

        #region PUBLIC PROPERTIES
        public short Id { get; set; }
        public int QuotationId { get; set; }
        public short ServiceId { get; set; }
        public string MetaDataCode { get; set; }
        public string ExtraCharges { get; set; }

        public byte Occurance { get; set; }
        /// <summary>
        #endregion
    }
}
