using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement
{
    internal static class ProcedureParameter
    {
        internal const string IS_ONLY_ACTIVE = "@IsOnlyActive";
        internal const string SERVICE_ID = "@ServiceId";
        internal const string DISPLAY_NAME = "@DisplayName";
        internal const string META_DATA_CODE = "@MetaDataCode";
        internal const string ARE_MULTIPLE_ENTRIES_ALLOWED = "@AreMultipleEntriesAllowed";
        internal const string INCLUDE_SERVICE_PROPERTIES = "@IncludeServiceProperties";
        internal const string SERVICE_PROPERTIES = "@ServiceProperties";
        internal const string SERVICE_PROPERTY_DETAILS = "@ServicePropertyDetails";
        internal const string SERVICE_PROPERTY_ID = "@ServicePropertyId";

        internal const string SUCCESS = "@Success";
        internal const string MESSAGE = "@Message";
    }
}
