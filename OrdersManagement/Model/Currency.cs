using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    public enum Currency
    {
        [StringValue("Unknown")]
        UNKNOWN = 0,
        [StringValue("INR")]
        INR = 1,
        [StringValue("BHD")]
        BHD = 2,
        [StringValue("USD")]
        USD = 3
    }
}
