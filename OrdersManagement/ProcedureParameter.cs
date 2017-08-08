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

        internal const string QUOTATION_ID = "@QuotationId";
        internal const string QUOTATION_NUMBER = "@QuotationNumber";
        internal const string ACCOUNT_ID = "@AccountId";
        internal const string EMPLOYEE_ID = "@EmployeeId";
        internal const string OWNERSHIP_ID = "@OwnerShipId";
        internal const string STATUS_ID = "@StatusId";
        internal const string CHANNEL_ID = "@ChannelId";
        internal const string IP_ADDRESS = "@IpAddress";
        internal const string BILLING_MODE_ID = "@BillingModeId";
        internal const string FROM_DATE_TIME = "@FromDateTime";
        internal const string TO_DATE_TIME = "@ToDateTime";
        internal const string PAGE_NUMBER = "@PageNumber";
        internal const string LIMIT = "@Limit";
        internal const string META_DATA = "@MetaData";
        internal const string STATE_ID = "@StateId";
        internal const string ORDER_AMOUNT = "@OrderAmount";

        internal const string SUCCESS = "@Success";
        internal const string MESSAGE = "@Message";
    }
}
