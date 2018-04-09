using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using OrdersManagement.Model;
using OrdersManagement.Exceptions;
using Newtonsoft.Json.Linq;

namespace OrdersManagement.Core
{
    public class ActivationClient
    {
        private SqlConnection _sqlConnection = null;
        private SqlCommand _sqlCommand = null;
        private SqlDataAdapter _da = null;
        private DataSet _ds = null;
        private Helper _helper = null;
        private string _connectionString = string.Empty;
        internal ActivationClient(ref Helper helper)
        {
            this._helper = helper;
            this._sqlConnection = new SqlConnection(this._helper.ConnectionString);
        }

        public dynamic GetRequestObjectForActivation(int quotationId, bool isPostPaidQuotation, byte activationPercentage)
        {
            dynamic activationObject = new JObject();
            dynamic servicesObject;
            string serviceName = string.Empty;
            bool areMultipleEntriesAllowed = false;
            int quotationServiceId = 0;
            DataRow[] drServiceProperies;

            try
            {
                this._sqlCommand = new SqlCommand(StoredProcedure.GET_QUOTATION_DETAILS, this._sqlConnection);
                this._sqlCommand.Parameters.Add(ProcedureParameter.QUOTATION_ID, SqlDbType.Int).Value = quotationId;
                this._sqlCommand.Parameters.Add(ProcedureParameter.IS_POSTPAID_QUOTATION, SqlDbType.Bit).Value = isPostPaidQuotation;
                this._helper.PopulateCommonOutputParameters(ref this._sqlCommand);
                this._da = new SqlDataAdapter(this._sqlCommand);
                this._da.Fill(this._ds = new DataSet());
                if (!this._sqlCommand.IsSuccess())
                    return this.ErrorResponse();
                if (this._ds.Tables.Count > 2)
                {
                    this._ds.Tables[0].TableName = Label.QUOTATION;
                    this._ds.Tables[1].TableName = Label.QUOTATION_SERVICES;
                    this._ds.Tables[2].TableName = Label.QUOTATION_SERVICE_PROPERTIES;
                }
                else if (this._ds.Tables.Count > 1)
                {
                    this._ds.Tables[0].TableName = Label.QUOTATION;
                    this._ds.Tables[1].TableName = Label.QUOTATION_SERVICES;

                }
                else
                {
                    this._ds.Tables[0].TableName = Label.QUOTATION;
                }
                if (this._ds.Tables[Label.QUOTATION_SERVICES].Rows.Count != 0)
                {
                    //dtServicesUniqueTable = this._ds.Tables[Label.QUOTATION_SERVICES].DefaultView.ToTable(true, "ServiceId");


                    for (int quotationServices = 0; quotationServices < this._ds.Tables[Label.QUOTATION_SERVICES].Rows.Count; quotationServices++)
                    {
                        serviceName = Convert.ToString(this._ds.Tables[Label.QUOTATION_SERVICES].Rows[quotationServices]["MetaDataCode"]);
                        areMultipleEntriesAllowed = Convert.ToBoolean(this._ds.Tables[Label.QUOTATION_SERVICES].Rows[quotationServices]["AreMultipleEntriesAllowed"]);
                        quotationServiceId = Convert.ToInt32(this._ds.Tables[Label.QUOTATION_SERVICES].Rows[quotationServices]["Id"]);
                        activationObject.Add(new JProperty(serviceName, new JObject()));
                        activationObject.SelectToken(serviceName).Add(new JProperty(Label.ARE_MULTIPLE_ENTRIES_ALLOWED, areMultipleEntriesAllowed));
                        if (!activationObject.ContainsKey(serviceName))
                        {
                            if (areMultipleEntriesAllowed)
                            {
                                activationObject.SelectToken(serviceName).Add(new JProperty(Label.DATA, new JArray()));
                            }
                            else
                            {
                                activationObject.SelectToken(serviceName).Add(new JProperty(Label.DATA, new JObject()));
                            }
                        }
                        drServiceProperies = this._ds.Tables[Label.QUOTATION_SERVICE_PROPERTIES].Select(Label.QUOTATION_SERVICE_ID + "=" + quotationServiceId);
                        foreach (DataRow drServiceProperty in drServiceProperies)
                        {
                            activationObject.SelectToken(serviceName).Add(new JProperty(drServiceProperty["MetaDataCode"].ToString(), drServiceProperty["Value"]));
                        }
                    }

                }

            }
            catch (Exception ex)
            {

            }

            return activationObject;
        }

        private dynamic ErrorResponse(string message = null)
        {
            this._helper.CreateProperty(Label.SUCCESS, false);
            if (message != null)
                this._helper.CreateProperty(Label.MESSAGE, message);
            else
                this._helper.CreateProperty(Label.MESSAGE, this._sqlCommand.GetMessage());
            return this._helper.GetResponse();
        }

    }
}
