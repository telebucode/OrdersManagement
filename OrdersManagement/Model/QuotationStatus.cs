using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    /// <summary>
    /// Represets the Status of the Quotation
    /// </summary>
    public enum QuotationStatus
    {
        [StringValue("Unknown")]
        UNKNOWN = 0,
        [StringValue("Created")]
        CREATED = 1,
        [StringValue("Invoice Generated")]
        INVOICE_GENERATED = 2
    }
}
