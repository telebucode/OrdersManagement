using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace OrdersManagement
{
    internal static class ExtensionMethods
    {
        internal static bool IsDbNull(this object input)
        {
            return input.Equals(DBNull.Value) ? true : false;
        }
        internal static bool IsSuccess(this System.Data.SqlClient.SqlCommand sqlCommand)
        {
            if (!sqlCommand.Parameters.Contains(ProcedureParameter.SUCCESS))
                throw new KeyNotFoundException("Success output parameter is not found in the sqlcommand parameters list.");
            return Convert.ToBoolean(sqlCommand.Parameters[ProcedureParameter.SUCCESS].Value);
        }
        internal static string GetMessage(this System.Data.SqlClient.SqlCommand sqlCommand)
        {
            return sqlCommand.Parameters[ProcedureParameter.MESSAGE].IsDbNull() ? "Null Message" : sqlCommand.Parameters[ProcedureParameter.MESSAGE].Value.ToString();
        }
        internal static string GetStringValue(this Model.PropertyInputType input)
        {
            StringValueAttribute[] stringValueAttributes = (StringValueAttribute[])input.GetType().GetField(input.ToString()).GetCustomAttributes(typeof(StringValueAttribute), false);
            return stringValueAttributes.Length > 0 ? stringValueAttributes[0].Value : string.Empty;
        }
        internal static string GetStringValue(this Model.PropertyDataType input)
        {
            StringValueAttribute[] stringValueAttributes = (StringValueAttribute[])input.GetType().GetField(input.ToString()).GetCustomAttributes(typeof(StringValueAttribute), false);
            return stringValueAttributes.Length > 0 ? stringValueAttributes[0].Value : string.Empty;
        }
        internal static string GetStringValue(this Model.MetaDataFormat input)
        {
            StringValueAttribute[] stringValueAttributes = (StringValueAttribute[])input.GetType().GetField(input.ToString()).GetCustomAttributes(typeof(StringValueAttribute), false);
            return stringValueAttributes.Length > 0 ? stringValueAttributes[0].Value : string.Empty;
        }
        internal static string GetSingleRowName(this Model.TablePreferences tablePreferences)
        {
            if (tablePreferences.ChildElementNameForRows.Length > 0)
                return tablePreferences.ChildElementNameForRows;
            else if (tablePreferences.RootName.Length > 0)
                return tablePreferences.RootName;
            else
                return string.Empty;
        }
    }
}
