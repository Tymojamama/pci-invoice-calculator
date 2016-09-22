using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using InvoiceCalculation.CRM.Data;

namespace InvoiceCalculation.CRM.Model
{
    public class Invoice : EntityBase
    {
        public Invoice() : base("new_customerinvoice") { }
        public Invoice(DynamicEntity e) : base(e) { }
        public Invoice(InvoiceCalculation.Model.Invoice invoice) : base("new_customerinvoice")
        {
            this.Id = Guid.Empty;
            this.Name = invoice.Name;
            this.ClientId = invoice.ClientId;
            this.EngagementId = invoice.EngagementId;
            //this.GeneralLedgerAccountId = invoice.GeneralLedgerAccountId;
            this.BilledOn = invoice.BilledOn.ToLocalTime();
            this.BillingType = (int)invoice.BillingType;
            this.DaysToPay = invoice.DaysToPay;
            this.EarnedOn = invoice.EarnedOn.ToLocalTime();
            this.StartDate = invoice.StartDate.ToLocalTime();
            this.EndDate = invoice.EndDate.ToLocalTime();
            this.AnnualFee = invoice.AnnualFee;
            this.InvoiceFee = invoice.InvoiceFee;
            this.TotalPlanAssetsUsed = invoice.TotalPlanAssetsUsed;
        }

        public Guid Id
        {
            get { return base.GetPropertyValue<Guid>("new_customerinvoiceid", PropertyType.Key, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_customerinvoiceid", PropertyType.Key, value); }
        }

        public Guid ClientId
        {
            get { return base.GetPropertyValue<Guid>("new_accountid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_accountid", PropertyType.Lookup, value); }
        }

        public Guid EngagementId
        {
            get { return base.GetPropertyValue<Guid>("new_engagementid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_engagementid", PropertyType.Lookup, value); }
        }

        public Guid GeneralLedgerAccountId
        {
            get { return base.GetPropertyValue<Guid>("new_generalledgeraccountid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_generalledgeraccountid", PropertyType.Lookup, value); }
        }

        public string Name
        {
            get { return base.GetPropertyValue<string>("new_name", PropertyType.String, string.Empty); }
            set { base.SetPropertyValue<string>("new_name", PropertyType.String, value); }
        }

        public decimal AnnualFee
        {
            get { return base.GetPropertyValue<decimal>("new_annualfee", PropertyType.Money, 0m); }
            set { base.SetPropertyValue<decimal>("new_annualfee", PropertyType.Money, value); }
        }

        public decimal InvoiceFee
        {
            get { return base.GetPropertyValue<decimal>("new_calculatedfee", PropertyType.Money, 0m); }
            set { base.SetPropertyValue<decimal>("new_calculatedfee", PropertyType.Money, value); }
        }

        public decimal TotalPlanAssetsUsed
        {
            get { return base.GetPropertyValue<decimal>("new_totalplanassetsused", PropertyType.Money, 0m); }
            set { base.SetPropertyValue<decimal>("new_totalplanassetsused", PropertyType.Money, value); }
        }

        public DateTime BilledOn
        {
            get { return base.GetPropertyValue<DateTime>("new_billedon", PropertyType.DateTime, DateTime.MaxValue); }
            set { base.SetPropertyValue<DateTime>("new_billedon", PropertyType.DateTime, value); }
        }

        public DateTime? EarnedOn
        {
            get { return base.GetPropertyValue<DateTime?>("new_earnedon", PropertyType.DateTime, null); }
            set { base.SetPropertyValue<DateTime?>("new_earnedon", PropertyType.DateTime, value); }
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

        public int BillingType
        {
            get { return base.GetPropertyValue<int>("new_billingtype", PropertyType.Picklist, -1); }
            set { base.SetPropertyValue<int>("new_billingtype", PropertyType.Picklist, value); }
        }

        public int? DaysToPay
        {
            get { return base.GetPropertyValue<int?>("new_daystopay", PropertyType.Number, null); }
            set { base.SetPropertyValue<int?>("new_daystopay", PropertyType.Number, value); }
        }

        public int StatusReason
        {
            get { return base.GetPropertyValue<int>("statuscode", PropertyType.Status, -1); }
            set { base.SetPropertyValue<int>("statuscode", PropertyType.Status, value); }
        }
    }
}
