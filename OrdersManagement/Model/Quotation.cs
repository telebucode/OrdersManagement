using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        #endregion
    }
}
