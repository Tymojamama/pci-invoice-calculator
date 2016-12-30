using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvoiceCalculation.CRM.Model
{
    public class Account : EntityBase
    {
        public Account() : base("account") { }
        public Account(Microsoft.Crm.Sdk.DynamicEntity e) : base(e) { }

        public Guid Id
        {
            get { return base.GetPropertyValue<Guid>("accountid", PropertyType.Key, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("accountid", PropertyType.Key, value); }
        }

        public new String Name
        {
            get { return base.GetPropertyValue<String>("name", PropertyType.String, ""); }
            set { base.SetPropertyValue<String>("name", PropertyType.String, value); }
        }

        public String Phone
        {
            get { return base.GetPropertyValue<String>("telephone1", PropertyType.String, ""); }
            set { base.SetPropertyValue<String>("telephone1", PropertyType.String, value); }
        }

        public String Fax
        {
            get { return base.GetPropertyValue<String>("fax", PropertyType.String, ""); }
            set { base.SetPropertyValue<String>("fax", PropertyType.String, value); }
        }

        public String WebsiteUrl
        {
            get { return base.GetPropertyValue<String>("websiteurl", PropertyType.String, ""); }
            set { base.SetPropertyValue<String>("websiteurl", PropertyType.String, value); }
        }

        public String Address
        {
            get { return base.GetPropertyValue<String>("address2_line1", PropertyType.String, ""); }
            set { base.SetPropertyValue<String>("address2_line1", PropertyType.String, value); }
        }

        public String Address2
        {
            get { return base.GetPropertyValue<String>("address2_line2", PropertyType.String, ""); }
            set { base.SetPropertyValue<String>("address2_line2", PropertyType.String, value); }
        }

        public String City
        {
            get { return base.GetPropertyValue<String>("address2_city", PropertyType.String, ""); }
            set { base.SetPropertyValue<String>("address2_city", PropertyType.String, value); }
        }

        public String State
        {
            get { return base.GetPropertyValue<String>("address2_stateorprovince", PropertyType.String, ""); }
            set { base.SetPropertyValue<String>("address2_stateorprovince", PropertyType.String, value); }
        }

        public String PostalCode
        {
            get { return base.GetPropertyValue<String>("address2_postalcode", PropertyType.String, ""); }
            set { base.SetPropertyValue<String>("address2_postalcode", PropertyType.String, value); }
        }

        public String CategoryCode
        {
            get { return base.GetPropertyValue<String>("accountcategorycode", PropertyType.String, ""); }
            set { base.SetPropertyValue<String>("accountcategorycode", PropertyType.String, value); }
        }

        public DateTime? LastUpdated
        {
            get
            {
                String crmDateString = base.GetPropertyValue<String>("vsp_lastupdated", PropertyType.String, String.Empty);
                return base.StringDateToNullable(crmDateString);
            }
            set
            {
                if (value.HasValue)
                    base.SetPropertyValue<DateTime>("vsp_lastupdated", PropertyType.DateTime, value.Value);

                //String crmDateString = String.Empty;
                //if (value.HasValue) { crmDateString = value.Value.ToShortDateString(); }
                //base.SetPropertyValue<String>("vsp_lastupdated", PropertyType.String, crmDateString);
            }
        }

        public DateTime BeganRecordkeeping { get; set; }
        public String ParentCompany { get; set; }
        public Int32 QualifiedEmployees { get; set; }
    }
}
