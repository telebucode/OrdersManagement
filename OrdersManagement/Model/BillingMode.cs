using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    /// <summary>
    /// Represents Quotation/Invoice BillingMode
    /// </summary>
    public enum BillingMode
    {
        [StringValue("Unknown")]
        UNKNOWN = 0,
        [StringValue("PrePaid")]
        PREPAID = 1,
        [StringValue("PostPaid")]
        POSTPAID = 2
    }
}
