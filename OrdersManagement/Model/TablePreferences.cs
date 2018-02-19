using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    public class TablePreferences
    {
        public TablePreferences(string RootName, string childElementNameForRows = "",
                    bool columnValuesAsXmlAttributes = true, bool singleRowAsSingleEntity = true)
        {
            this.RootName = RootName;
            this.ChildElementNameForRows = childElementNameForRows;
            this.ColumnValuesAsXmlAttributes = columnValuesAsXmlAttributes;
            this.SingleRowAsSingleEntity = singleRowAsSingleEntity;
        }
        public string RootName { get; set; }
        public string ChildElementNameForRows { get; set; }
        public bool ColumnValuesAsXmlAttributes { get; set; }
        public bool SingleRowAsSingleEntity { get; set; }

        
    }
}
