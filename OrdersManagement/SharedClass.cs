using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement
{
    internal static class SharedClass
    {
        internal static Dictionary<string, Model.Service> Services = new Dictionary<string, Model.Service>();
        internal static Dictionary<byte, Model.PropertyInputType> InputTypes = new Dictionary<byte, Model.PropertyInputType>();
        internal static Dictionary<byte, Model.PropertyDataType> InputDataTypes = new Dictionary<byte, Model.PropertyDataType>();
        internal static bool InputTypesLoaded { get { return InputTypes.Count > 0; } }
        internal static bool InputDataTypesLoaded { get { return InputDataTypes.Count > 0; } }
        internal static bool ServiceLoaded { get { return Services.Count > 0; } }
    }
}
