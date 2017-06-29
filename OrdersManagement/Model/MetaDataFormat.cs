using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    public enum MetaDataFormat
    {
        [StringValue("Json")]
        JSON = 1,
        [StringValue("Xml")]
        XML = 2
    }
}
