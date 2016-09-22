using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using InvoiceCalculation.CRM.Data;

namespace InvoiceCalculation.CRM.Model
{
    public class PlanAsset : EntityBase
    {
        public PlanAsset() : base("new_planasset") { }
        public PlanAsset(DynamicEntity e) : base(e) { }

        public Guid Id
        {
            get { return base.GetPropertyValue<Guid>("new_planassetid", PropertyType.Key, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_planassetid", PropertyType.Key, value); }
        }

        public Guid PlanId
        {
            get { return base.GetPropertyValue<Guid>("new_planid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_planid", PropertyType.Lookup, value); }
        }

        public decimal AssetValue
        {
            get { return base.GetPropertyValue<decimal>("new_assetvalue", PropertyType.Money, 0m); }
            set { base.SetPropertyValue<decimal>("new_assetvalue", PropertyType.Money, value); }
        }

        public DateTime AssetValueAsOf
        {
            get { return base.GetPropertyValue<DateTime>("new_assetvalueasof", PropertyType.DateTime, DateTime.MaxValue); }
            set { base.SetPropertyValue<DateTime>("new_assetvalueasof", PropertyType.DateTime, value); }
        }
    }
}
