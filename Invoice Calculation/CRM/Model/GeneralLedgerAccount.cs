using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using InvoiceCalculation.CRM.Data;

namespace InvoiceCalculation.CRM.Model
{
    public class GeneralLedgerAccount : EntityBase
    {
        public GeneralLedgerAccount() : base("new_generalledgeraccount") { }
        public GeneralLedgerAccount(DynamicEntity e) : base(e) { }

        public Guid Id
        {
            get { return base.GetPropertyValue<Guid>("new_generalledgeraccountid", PropertyType.Key, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_generalledgeraccountid", PropertyType.Key, value); }
        }

        public String Name
        {
            get { return base.GetPropertyValue<String>("new_name", PropertyType.String, String.Empty); }
            set { base.SetPropertyValue<String>("new_name", PropertyType.String, value); }
        }

        public String QuickBooksAccountNumber
        {
            get { return base.GetPropertyValue<String>("new_quickbooksaccountnumber", PropertyType.String, String.Empty); }
            set { base.SetPropertyValue<String>("new_quickbooksaccountnumber", PropertyType.String, value); }
        }
    }
}
