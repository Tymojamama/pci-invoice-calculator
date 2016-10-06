using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceCalculation.Model
{
    public enum BillingType
    {
        InArrears = 1,
        InAdvanced = 2
    }

    public class Invoice
    {
        public string Name;
        public decimal AnnualFee;
        public decimal InvoiceFee;
        public decimal InvoiceCredit;
        public decimal TotalPlanAssetsUsed;
        public DateTime BilledOn;
        public DateTime EarnedOn;
        public DateTime StartDate;
        public DateTime EndDate;
        public BillingType BillingType;
        public int DaysToPay;
        public Guid EngagementId;
        public Guid ClientId;
        public Guid GeneralLedgerAccountId;
        public List<InvoiceLineItem> LineItems = new List<InvoiceLineItem>();

        public Invoice()
        {

        }
    }
}
