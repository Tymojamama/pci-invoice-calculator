using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using InvoiceCalculation.CRM.Data;

namespace InvoiceCalculation.CRM.Model
{
    public class InvoiceLineItem : EntityBase
    {
        public InvoiceLineItem() : base("new_customerinvoicelineitem") { }
        public InvoiceLineItem(DynamicEntity e) : base(e) { }
        public InvoiceLineItem(InvoiceCalculation.Model.InvoiceLineItem invoiceLineItem)
            : base("new_customerinvoicelineitem")
        {
            this.Id = Guid.Empty;
            this.Name = invoiceLineItem.Name;
            this.CustomerInvoiceId = invoiceLineItem.InvoiceId;
            this.GeneralLedgerAccountId = invoiceLineItem.GeneralLedgerAccountId;
            this.QuickbooksTeamClassId = invoiceLineItem.TeamId;
            this.Amount = invoiceLineItem.Amount;
            this.StartDate = invoiceLineItem.StartDate;
            this.EndDate = invoiceLineItem.EndDate;
            this.LineItemType = (int)invoiceLineItem.LineItemType;
        }

        public Guid Id
        {
            get { return base.GetPropertyValue<Guid>("new_customerinvoicelineitemid", PropertyType.Key, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_customerinvoicelineitemid", PropertyType.Key, value); }
        }

        public Guid CustomerInvoiceId
        {
            get { return base.GetPropertyValue<Guid>("new_customerinvoiceid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_customerinvoiceid", PropertyType.Lookup, value); }
        }

        public Guid GeneralLedgerAccountId
        {
            get { return base.GetPropertyValue<Guid>("new_generalledgeraccountid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_generalledgeraccountid", PropertyType.Lookup, value); }
        }

        public Guid QuickbooksTeamClassId
        {
            get { return base.GetPropertyValue<Guid>("new_quickbooksteamclass", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_quickbooksteamclass", PropertyType.Lookup, value); }
        }

        public string Name
        {
            get { return base.GetPropertyValue<string>("new_name", PropertyType.String, string.Empty); }
            set { base.SetPropertyValue<string>("new_name", PropertyType.String, value); }
        }

        public decimal Amount
        {
            get { return base.GetPropertyValue<decimal>("new_amount", PropertyType.Money, 0m); }
            set { base.SetPropertyValue<decimal>("new_amount", PropertyType.Money, value); }
        }

        public DateTime? StartDate
        {
            get { return base.GetPropertyValue<DateTime?>("new_startdate", PropertyType.DateTime, null); }
            set { base.SetPropertyValue<DateTime?>("new_startdate", PropertyType.DateTime, value); }
        }

        public DateTime? EndDate
        {
            get { return base.GetPropertyValue<DateTime?>("new_enddate", PropertyType.DateTime, null); }
            set { base.SetPropertyValue<DateTime?>("new_enddate", PropertyType.DateTime, value); }
        }

        public int LineItemType
        {
            get { return base.GetPropertyValue<int>("new_lineitemtype", PropertyType.Picklist, -1); }
            set { base.SetPropertyValue<int>("new_lineitemtype", PropertyType.Picklist, value); }
        }
    }
}
