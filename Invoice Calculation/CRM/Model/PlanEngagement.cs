using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvoiceCalculation.CRM.Model
{
    public class PlanEngagement : EntityBase
    {
        public PlanEngagement() : base("new_new_plan_new_project") { }
        public PlanEngagement(Microsoft.Crm.Sdk.DynamicEntity e) : base(e) { }

        public Guid Id
        {
            get { return base.GetPropertyValue<Guid>("new_new_plan_new_projectid", PropertyType.Key, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_new_plan_new_projectid", PropertyType.Key, value); }
        }

        public Guid PlanId
        {
            get { return base.GetPropertyValue<Guid>("new_planid", PropertyType.UniqueIdentifier, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_planid", PropertyType.UniqueIdentifier, value); }
        }

        public Guid EngagementId
        {
            get { return base.GetPropertyValue<Guid>("new_projectid", PropertyType.UniqueIdentifier, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_projectid", PropertyType.UniqueIdentifier, value); }
        }
    }
}
