using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace OrdersManagement.Model
{
    public class Quotation
    {
        #region PRIVATE VARIABLES

        private long _id = long.MinValue;
        private long _accountId = long.MinValue;
        private int _raisedBy = int.MinValue;
        private string _identifier = string.Empty;
        private QuotationStatus _status = QuotationStatus.UNKNOWN;
        private float _amountWithOutTax = 0;
        private float _tax = 0;
        private float _totalAmount = 0;
        private Currency _currency = Currency.UNKNOWN;
        private string _ip = string.Empty;
        private Nullable<DateTime> _createdTime = null;
        private Nullable<DateTime> _updatedTime = null;
        private string _email = string.Empty;
        private string _mobile = string.Empty;
        private BillingMode _billingMode = BillingMode.PREPAID;
        private bool _isBillGenerated = false;
        private List<Service> _services = new List<Service>();
        private MetaDataFormat _format = MetaDataFormat.JSON;
        private JObject _jsonObj = null;
        private JArray _jsonArray = null;
        private XmlDocument _xmlDocument = null;
        private XmlElement _rootElement = null;        

        #endregion

        #region CONSTRUCTORS

        public Quotation(MetaDataFormat format)
        {
            this._format = format;
            this.InitializeMetaDataVariables();
        }
        public Quotation()
        {
            this.InitializeMetaDataVariables();
        }

        #endregion

        #region PRIVATE METHODS
        private void InitializeMetaDataVariables()
        { 
            if(this._format.Equals(MetaDataFormat.JSON))
            {
                this._jsonArray = new JArray();
                this._jsonObj = new JObject();
            }
            else if(this._format.Equals(MetaDataFormat.XML))
            {
                this._xmlDocument = new XmlDocument();
                this._rootElement = this._xmlDocument.CreateElement("Services");
            }
        }
        #endregion


        #region PUBLIC PROPERTIES

        /// <summary>
        /// Gets Or Sets the Id of the Quotation in Data Base. (Slno in the table)
        /// </summary>
        public long Id { get { return this._id; } set { this._id = value; } }
        /// <summary>
        /// Gets Or Sets the AccountId to which this Quotation belongs to.
        /// </summary>
        public long AccountId { get { return this._accountId; } set { this._accountId = value; } }
        /// <summary>
        /// Gets Or Sets the adminId by which this quotation is raised.
        /// </summary>
        public int RaisedBy { get { return this._raisedBy; } set { this._raisedBy = value; } }
        /// <summary>
        /// Gets Or Sets the Alphanumeric Number of this quotation
        /// </summary>
        public string Identifier { get { return this._identifier; } set { this._identifier = value; } }
        /// <summary>
        /// Gets Or Sets the Quotation Status
        /// </summary>
        public QuotationStatus Status { get { return this._status; } set { this._status = value; } }
        /// <summary>
        /// Gets Or Sets the Amount of this Quotation without Tax.
        /// </summary>
        public float AmountWithOutTax { get { return this._amountWithOutTax; } set { this._amountWithOutTax = value; } }
        /// <summary>
        /// Gets Or Sets the Tax applied for this Quotation.
        /// </summary>
        public float Tax { get { return this._tax; } set { this._tax = value; } }
        /// <summary>
        /// Gets Or Sets the Total Amount for this Quotation (Including Tax).
        /// </summary>
        public float TotalAmount { get { return this._totalAmount; } set { this._totalAmount = value; } }
        /// <summary>
        /// Gets Or Sets the Currency Enum for this Quotation.
        /// </summary>
        public Currency Currency { get { return this._currency; } set { this._currency = value; } }
        /// <summary>
        /// Gets Or Sets the IP from which the Quotation raised.
        /// </summary>
        public string Ip { get { return this._ip; } set { this._ip = value; } }
        /// <summary>
        /// Gets Or Sets the Quotation CreatedTime in DataBase.
        /// </summary>
        public Nullable<DateTime> CreatedTime { get { return this._createdTime; } set { this._createdTime = value; } }
        /// <summary>
        /// Gets Or Sets the Last Updated Time of this Quotation.
        /// </summary>
        public Nullable<DateTime> UpdatedTime { get { return this._updatedTime; } set { this._updatedTime = value; } }
        /// <summary>
        /// Gets Or Sets the BillingMode to this Quotation.
        /// </summary>
        public BillingMode BillingMode { get { return this._billingMode; } set { this._billingMode = value; } }
        /// <summary>
        /// Gets Or Sets the flag indicating whether the bill is generated, if Quotation BillingMode Is PostPaid.
        /// </summary>
        public bool IsBillGenerated { get { return this._isBillGenerated; } set { this._isBillGenerated = value; } }
        /// <summary>
        /// Gets Or Sets the list of Services associated with this quotation
        /// </summary>
        public List<Service> Services { get { return this._services; } set { this._services = value; } }
        /// <summary>
        /// Gets Or Sets the Services MetaData Format. Default is JSON
        /// </summary>
        public MetaDataFormat Format { get { return this._format; } set { this._format = value; } }

        #endregion

        #region PUBLIC METHODS

        public void GenerateMetadata()
        {
            if(this._format.Equals(MetaDataFormat.JSON))
            {
                foreach(Service service in this._services)
                {
                    JObject serviceObject = new JObject();
                    if(service.AreMultipleAllowed)
                    {
                        this._jsonArray = new JArray();
                        //foreach(ServiceProperty property in service.Properties)
                        //{
                            
                        //}
                    }
                }
            }
        }
        public bool ParseMetaData(dynamic metaData)
        {
            if (this._format.Equals(MetaDataFormat.JSON))
            {
                this._jsonObj = metaData as JObject;
                return this.ParseJSon();
            }   
            else if (this.Format.Equals(MetaDataFormat.XML))
            {
                this._xmlDocument.LoadXml(metaData.ToString());
                return this.ParseXml();
            }
            return false;
        }
        #endregion
        #region PRIVATE METHODS
        /// <summary>
        /// Parses JSon MetaData
        /// </summary>
        /// <returns>bool value indicating parse process success.</returns>
        private bool ParseJSon()
        {
            bool isWellFormatted = false;
            JObject tempJObj = null;
            JArray tempJArray = null;
            string currentService = string.Empty;
            foreach(JProperty jsonProperty in this._jsonObj.Properties())
            {
                tempJArray = null;
                tempJObj = null;
                currentService = jsonProperty.Name;
                if (!SharedClass.Services.ContainsKey(currentService))
                    throw new KeyNotFoundException(string.Format("Service '{0}' Not Found in SharedClass", currentService));
                if (SharedClass.Services[currentService].AreMultipleAllowed)
                    tempJArray = this._jsonObj.SelectToken(currentService) as JArray;                
                foreach(JToken child in tempJArray.Children())
                {
                    tempJObj = child as JObject;

                }
            }
            return isWellFormatted;
        }
        private void ParseServicePropertyJSon(string serviceName, JObject serviceProperty)
        {
            foreach (KeyValuePair<string, ServiceProperty> servicePropertyEntry in SharedClass.Services[serviceName].Properties)
            {
                if (servicePropertyEntry.Value.IsRequired && (serviceProperty.SelectToken(servicePropertyEntry.Value.Code) == null
                        || serviceProperty.SelectToken(servicePropertyEntry.Value.Code).ToString().Trim().Length == 0))
                    throw new MissingFieldException(string.Format("Property {0} is marked as Required. But it is not found or empty in MetaData.",
                        servicePropertyEntry.Value.Code));
                if (serviceProperty.SelectToken(servicePropertyEntry.Value.Code).ToString().Trim().Length > 0)
                {
                    switch (servicePropertyEntry.Value.DataType)
                    {
                        case PropertyDataType.INT:
                            try
                            {
                                int.Parse(serviceProperty.SelectToken(servicePropertyEntry.Value.Code).ToString());
                            }
                            catch (Exception e)
                            {
                                throw new InvalidCastException(string.Format("Property {0} requires Int value. But '{1}' is not an Int value.",
                                    servicePropertyEntry.Value.Code, serviceProperty.SelectToken(servicePropertyEntry.Value.Code).ToString()));
                            }
                            break;
                        case PropertyDataType.FLOAT:
                            try
                            {
                                float.Parse(serviceProperty.SelectToken(servicePropertyEntry.Value.Code).ToString());
                            }
                            catch (Exception e)
                            {
                                throw new InvalidCastException(string.Format("Property {0} requires float value. But '{1}' is not a float value.",
                                    servicePropertyEntry.Value.Code, serviceProperty.SelectToken(servicePropertyEntry.Value.Code).ToString()));
                            }
                            break;
                        case PropertyDataType.DATETIME:
                            try
                            {
                                DateTime.Parse(serviceProperty.SelectToken(servicePropertyEntry.Value.Code).ToString());
                            }
                            catch (Exception e)
                            {
                                throw new InvalidCastException(string.Format("Property {0} requires DateTime value. But '{1}' is not a valid DateTime value.",
                                    servicePropertyEntry.Value.Code, serviceProperty.SelectToken(servicePropertyEntry.Value.Code).ToString()));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        private bool ParseXml()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
