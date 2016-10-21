using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using InvoiceCalculation.CRM.Data;

namespace InvoiceCalculation.CRM.Model
{
    public class GlaInvoiceTeamSplit : EntityBase
    {
        public GlaInvoiceTeamSplit() : base("new_glainvoiceteamsplit") { }
        public GlaInvoiceTeamSplit(DynamicEntity e) : base(e) { }

        public Guid Id
        {
            get { return base.GetPropertyValue<Guid>("new_glainvoiceteamsplitid", PropertyType.Key, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_glainvoiceteamsplitid", PropertyType.Key, value); }
        }

        public Guid GeneralLedgerAccountId
        {
            get { return base.GetPropertyValue<Guid>("new_generalledgeraccountid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_generalledgeraccountid", PropertyType.Lookup, value); }
        }

        public Guid InvoiceId
        {
            get { return base.GetPropertyValue<Guid>("new_invoiceid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_invoiceid", PropertyType.Lookup, value); }
        }

        public Guid EngagementId
        {
            get { return base.GetPropertyValue<Guid>("new_engagementid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_engagementid", PropertyType.Lookup, value); }
        }

        public Guid AccountId
        {
            get { return base.GetPropertyValue<Guid>("new_accountid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_accountid", PropertyType.Lookup, value); }
        }

        public Guid Team1
        {
            get { return base.GetPropertyValue<Guid>("new_team1", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_team1", PropertyType.Lookup, value); }
        }

        public Guid Team2
        {
            get { return base.GetPropertyValue<Guid>("new_team2", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_team2", PropertyType.Lookup, value); }
        }

        public Guid Team3
        {
            get { return base.GetPropertyValue<Guid>("new_team3", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_team3", PropertyType.Lookup, value); }
        }

        public Guid Team4
        {
            get { return base.GetPropertyValue<Guid>("new_team4", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_team4", PropertyType.Lookup, value); }
        }

        public Guid Team5
        {
            get { return base.GetPropertyValue<Guid>("new_team5", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_team5", PropertyType.Lookup, value); }
        }

        public String Name
        {
            get { return base.GetPropertyValue<String>("new_name", PropertyType.String, String.Empty); }
            set { base.SetPropertyValue<String>("new_name", PropertyType.String, value); }
        }

        public DateTime StartDate
        {
            get { return base.GetPropertyValue<DateTime>("new_startdate", PropertyType.DateTime, DateTime.MinValue); }
            set { base.SetPropertyValue<DateTime>("new_startdate", PropertyType.DateTime, value); }
        }

        public DateTime EndDate
        {
            get { return base.GetPropertyValue<DateTime>("new_enddate", PropertyType.DateTime, DateTime.MaxValue); }
            set { base.SetPropertyValue<DateTime>("new_enddate", PropertyType.DateTime, value); }
        }

        public int ProductType
        {
            get { return base.GetPropertyValue<int>("new_producttype", PropertyType.Picklist, -1); }
            set { base.SetPropertyValue<int>("new_producttype", PropertyType.Picklist, value); }
        }

        public bool UseDefaultSplit
        {
            get { return base.GetPropertyValue<bool>("new_usedefaultsplit", PropertyType.Bit, false); }
            set { base.SetPropertyValue<bool>("new_usedefaultsplit", PropertyType.Bit, value); }
        }

        public decimal Split1
        {
            get { return base.GetPropertyValue<decimal>("new_split1", PropertyType.Decimal, 0m); }
            set { base.SetPropertyValue<decimal>("new_split1", PropertyType.Decimal, value); }
        }

        public decimal Split2
        {
            get { return base.GetPropertyValue<decimal>("new_split2", PropertyType.Decimal, 0m); }
            set { base.SetPropertyValue<decimal>("new_split2", PropertyType.Decimal, value); }
        }

        public decimal Split3
        {
            get { return base.GetPropertyValue<decimal>("new_split3", PropertyType.Decimal, 0m); }
            set { base.SetPropertyValue<decimal>("new_split3", PropertyType.Decimal, value); }
        }

        public decimal Split4
        {
            get { return base.GetPropertyValue<decimal>("new_split4", PropertyType.Decimal, 0m); }
            set { base.SetPropertyValue<decimal>("new_split4", PropertyType.Decimal, value); }
        }

        public decimal Split5
        {
            get { return base.GetPropertyValue<decimal>("new_split5", PropertyType.Decimal, 0m); }
            set { base.SetPropertyValue<decimal>("new_split5", PropertyType.Decimal, value); }
        }
    }
}
