using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement
{
    internal class StringValueAttribute : System.Attribute
    {
        private string _value = string.Empty;
        internal StringValueAttribute(string value)
        {
            this._value = value;
        }
        internal string Value
        {
            get { return this._value; }
        }
    }
}
