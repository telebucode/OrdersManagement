using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    public enum PropertyDataType
    {
        [StringValue("Unknown")]
        UNKNOWN = 0,
        [StringValue("Int")]
        INT = 1,
        [StringValue("Float")]
        FLOAT = 2,
        [StringValue("String")]
        STRING = 3,
        [StringValue("DateTime")]
        DATETIME = 4
    }
}
