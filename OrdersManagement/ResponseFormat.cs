using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement
{
    public enum ResponseFormat
    {
        [StringValue("JSON")]
        JSON = 1,
        [StringValue("XML")]
        XML = 2,
        [StringValue("OBJECT")]
        OBJECT = 3
    }
}
