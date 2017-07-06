using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement.Model
{
    public class TablePreferences
    {
        public TablePreferences(string tableName, string childXmlElementNameForRows, bool columnValuesAsAttributes)
        {
            this.TableName = tableName;
            this.ChildXmlElementNameForRows = childXmlElementNameForRows;
            this.ColumnValuesAsAttributes = columnValuesAsAttributes;
        }
        public string TableName { get; set; }
        public string ChildXmlElementNameForRows { get; set; }
        public bool ColumnValuesAsAttributes { get; set; }
    }
}
