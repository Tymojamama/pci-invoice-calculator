using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceCalculation.Model
{
    public enum LineItemType
    {
        Fee = 1,
        Credit = 2
    }

    public class InvoiceLineItem
    {
        public Guid InvoiceId;
        public string Name;
        public string Description = "";
        public LineItemType LineItemType;
        public decimal Amount;
        public decimal PercentOfInvoice;
        public DateTime StartDate;
        public DateTime EndDate;
        public Guid GeneralLedgerAccountId;
        public Guid TeamId;
    }
}
