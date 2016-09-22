using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using InvoiceCalculation.CRM.Data;

namespace InvoiceCalculation.CRM.Model
{
    public class TaskTime : EntityBase
    {
        public TaskTime() : base("new_projecttasktime") { }
        public TaskTime(DynamicEntity e) : base(e) { }

        public Guid Id
        {
            get { return base.GetPropertyValue<Guid>("new_projecttasktimeid", PropertyType.Key, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_projecttasktimeid", PropertyType.Key, value); }
        }

        public Guid ComponentTaskId
        {
            get { return base.GetPropertyValue<Guid>("new_projecttaskid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_projecttaskid", PropertyType.Lookup, value); }
        }

        public String Name
        {
            get { return base.GetPropertyValue<String>("new_name", PropertyType.String, String.Empty); }
            set { base.SetPropertyValue<String>("new_name", PropertyType.String, value); }
        }

        public DateTime StartTime
        {
            get { return base.GetPropertyValue<DateTime>("new_starttime", PropertyType.DateTime, DateTime.MinValue); }
            set { base.SetPropertyValue<DateTime>("new_starttime", PropertyType.DateTime, value); }
        }

        public DateTime EndTime
        {
            get { return base.GetPropertyValue<DateTime>("new_endtime", PropertyType.DateTime, DateTime.MaxValue); }
            set { base.SetPropertyValue<DateTime>("new_endtime", PropertyType.DateTime, value); }
        }

        public bool? TaskComplete
        {
            get { return base.GetPropertyValue<bool?>("new_taskcomplete", PropertyType.Bit, null); }
            set { base.SetPropertyValue<bool?>("new_taskcomplete", PropertyType.Bit, value); }
        }

        public bool IsWithinDateRange(DateTime startTime, DateTime endTime)
        {
            var after = false;
            var before = false;

            if (this.StartTime <= endTime)
            {
                before = true;
            }

            if (this.StartTime >= startTime)
            {
                after = true;
            }

            return before && after;
        }
    }
}
