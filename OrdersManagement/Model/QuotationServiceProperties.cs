using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    class QuotationServiceProperties
    {
        #region PRIVATE VARIABLES

        private int id = 0;
        private int quotationId = 0;
        private short serviceId = 0;
        private short servicePropertiId = 0;
        private int quotationServiceId = 0;
        private string value = string.Empty;
        private string metaDataCode = string.Empty;

        #endregion

        #region PUBLIC PROPERTIES
        public int Id { get; set; }
        public int QuotationId { get; set; }

        public short ServiceId { get; set; }
        public short ServicePropertiId { get; set; }

        public int QuotationServiceId { get; set; }
        public string Value { get; set; }
        public string MetaDataCode { get; set; }

        /// <summary>
        #endregion
    }
}
