using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using Newtonsoft.Json.Linq;
using OrdersManagement.Exceptions;

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
        internal static DataTable ToDataTable(this List<Model.ServiceProperty> serviceProperties)
        {
            if (serviceProperties == null || serviceProperties.Count == 0)
                throw new Exceptions.ServiceException("Atleast one ServiceProperty Should Be Supplied");
            DataTable table = new DataTable();
            table.Columns.Add(Label.DISPLAY_NAME, typeof(string));
            table.Columns.Add(Label.META_DATA_CODE, typeof(string));
            table.Columns.Add(Label.IS_REQUIRED, typeof(bool));
            table.Columns.Add(Label.INPUT_TYPE_ID, typeof(byte));
            table.Columns.Add(Label.INPUT_DATA_TYPE_ID, typeof(byte));
            table.Columns.Add(Label.DEFAULT_VALUE, typeof(string));
            table.Columns.Add(Label.INCLUDE_IN_ORDER_AMOUNT, typeof(bool));
            byte index = 1;
            foreach (Model.ServiceProperty serviceProperty in serviceProperties)
            {
                if (serviceProperty.DisplayName == null || serviceProperty.DisplayName.Length == 0)
                    throw new Exceptions.ServiceException(string.Format("Invalid DisplayName at Row {0}", (index)));
                if (serviceProperty.MetaDataCode == null || serviceProperty.MetaDataCode.Length == 0)
                    throw new Exceptions.ServiceException(string.Format("Invalid MetaDataCode at Row {0}", (index)));
                table.Rows.Add(serviceProperty.DisplayName, serviceProperty.MetaDataCode, serviceProperty.IsRequired, serviceProperty.InputTypeId > 0 ? serviceProperty.InputTypeId : Convert.ToByte(serviceProperty.InputType),
                    serviceProperty.DataTypeId > 0 ? serviceProperty.DataTypeId : Convert.ToByte(serviceProperty.DataType), serviceProperty.DefaultValue, serviceProperty.IncludeInOrderAmount);
                ++index;
            }
            return table;
        }
        internal static DataTable ToDataTable(this Model.ServiceProperty serviceProperty)
        {
            DataTable table = new DataTable();
            table.Columns.Add(Label.DISPLAY_NAME, typeof(string));
            table.Columns.Add(Label.META_DATA_CODE, typeof(string));
            table.Columns.Add(Label.INPUT_TYPE_ID, typeof(byte));
            table.Columns.Add(Label.INPUT_DATA_TYPE_ID, typeof(byte));
            table.Columns.Add(Label.IS_REQUIRED, typeof(bool));
            table.Columns.Add(Label.IS_ACTIVE, typeof(bool));
            table.Columns.Add(Label.DEFAULT_VALUE, typeof(string));
            table.Columns.Add(Label.INCLUDE_IN_ORDER_AMOUNT, typeof(bool));
            table.Columns.Add(Label.INPUT_PROPERTY, typeof(string));
            if (serviceProperty.DisplayName == null || serviceProperty.DisplayName.Length == 0)
                throw new Exceptions.ServiceException(string.Format("Invalid DisplayName"));
            if (serviceProperty.MetaDataCode == null || serviceProperty.MetaDataCode.Length == 0)
                throw new Exceptions.ServiceException(string.Format("Invalid MetaDataCode"));
            table.Rows.Add(serviceProperty.DisplayName, serviceProperty.MetaDataCode, Convert.ToByte(serviceProperty.InputTypeId),
                Convert.ToByte(serviceProperty.DataTypeId), serviceProperty.IsRequired,serviceProperty.IsActive, serviceProperty.DefaultValue, serviceProperty.IncludeInOrderAmount,serviceProperty.InputProperty);
            return table;
        }

        internal static DataTable ToDataTable(this List<Model.ServicePropertyFields> servicePropertyFields)
        {
            DataTable table = new DataTable();
            table.Columns.Add(Label.FIELDID, typeof(Int16));
            table.Columns.Add(Label.INPUT_TYPE_ID, typeof(byte));
            table.Columns.Add(Label.META_DATA_CODE, typeof(string));
            table.Columns.Add(Label.MINLENGTH, typeof(Byte));
            table.Columns.Add(Label.MAXLENGTH, typeof(Int16));
            table.Columns.Add(Label.Is_Allow_Special_Chars, typeof(bool));
            table.Columns.Add(Label.OPTIONS, typeof(string));
            byte index = 1;
            foreach (Model.ServicePropertyFields servicePropertyField in servicePropertyFields)
            {
                if (servicePropertyField.MetaDataCode == null || servicePropertyField.MetaDataCode.Length == 0)
                    throw new Exceptions.ServiceException(string.Format("Invalid Metacode at Row {0}", index));
                table.Rows.Add(servicePropertyField.FieldId, servicePropertyField.InputTypeId, servicePropertyField.MetaDataCode, servicePropertyField.MinLength, servicePropertyField.MaxLength,
                   servicePropertyField.IsAllowSpecialChars, servicePropertyField.Options);

                ++index;
            }
            return table;
        }

        internal static DataTable ToQuotationServicesDataTable(this List<Model.QuotationServices> quotationServices)
        {
            DataTable table = new DataTable();
            table.Columns.Add(Label.ID, typeof(int));
            table.Columns.Add(Label.META_DATA_CODE, typeof(string));
            table.Columns.Add(Label.SERVICE_ID, typeof(short));
            table.Columns.Add(Label.EXTRA_CHARGES, typeof(string));
            table.Columns.Add(Label.OCCURANCE, typeof(string));

            foreach (Model.QuotationServices servicePropertyField in quotationServices)
            {
                table.Rows.Add(servicePropertyField.Id, servicePropertyField.MetaDataCode, servicePropertyField.ServiceId, servicePropertyField.ExtraCharges, servicePropertyField.Occurance);

            }
            return table;
        }
        internal static DataTable ToQuotationServicePropertiesDataTable(this List<Model.QuotationServiceProperties> quotationServices)
        {
            DataTable table = new DataTable();

            table.Columns.Add(Label.META_DATA_CODE, typeof(string));
            table.Columns.Add(Label.SERVICE_ID, typeof(short));
            table.Columns.Add(Label.SERVICE_PROPERTY_ID, typeof(short));
            table.Columns.Add(Label.VALUE, typeof(string));
            table.Columns.Add(Label.QUOTATION_SERVICE_ID, typeof(int));

            foreach (Model.QuotationServiceProperties servicePropertyField in quotationServices)
            {
                table.Rows.Add(servicePropertyField.MetaDataCode, servicePropertyField.ServiceId, servicePropertyField.ServicePropertiId, servicePropertyField.Value, servicePropertyField.QuotationServiceId);

            }
            return table;
        }

        internal static string ToExtraChargesString(this JToken extraChargesOfService, string serviceName)
        {
            JArray extraChargesArray = extraChargesOfService as JArray;
            JObject extraJobject = new JObject();

            if (extraChargesArray != null)
            {
                foreach (JToken child in extraChargesArray.Children())
                {
                    try
                    {
                        JObject childObject = child as JObject;
                        extraJobject.Add(new JProperty(childObject.SelectToken(Label.DESCRIPTION).ToString(), childObject.SelectToken(Label.AMOUNT).ToString()));

                    }
                    catch (Exception e)
                    {
                        throw new QuotationException(string.Format("Duplicate key name for {0} cannot be added to Service {1}", Label.EXTRA_CHARGES, serviceName));
                    }

                }
            }
            else
            {

                return string.Empty;
            }

            return extraJobject.ToString();
        }
    }
}
