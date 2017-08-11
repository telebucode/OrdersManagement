using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersManagement
{
    internal static class StoredProcedure
    {
        #region SERVICES RELATED
        internal const string GET_SERVICES = "GetServices";
        internal const string GET_SERVICE_PROPERTIES = "GetServiceProperties";
        internal const string GET_INPUT_TYPES = "GetInputTypes";
        internal const string GET_INPUT_DATA_TYPES = "GetInputDataTypes";
        internal const string CREATE_SERVICE = "CreateService";
        internal const string UPDATE_SERVICE = "UpdateService";
        internal const string DELETE_SERVICE = "DeleteService";
        internal const string CREATE_SERVICE_PROPERTIES = "CreateServiceProperties";
        internal const string UPDATE_SERVICE_PROPERTY = "UpdateServiceProperty";
        internal const string DELETE_SERVICE_PROPERTY = "DeleteServiceProperty";
        #endregion
        #region QUOTATION RELATED
        internal const string GET_QUOTATION_STATUSES = "GetQuotationStatuses";
        internal const string GET_QUOTATIONS = "GetQuotations";
        internal const string GET_QUOTATION_CHANNELS = "GetQuotatioChannels";
        internal const string CREATE_QUOTATION = "CreateQuotation";
        internal const string UPDATE_QUOTATION = "UpdateQuotation";
        #endregion
        internal const string GET_BILLING_MODES = "GetBillingModes";
    }
}
