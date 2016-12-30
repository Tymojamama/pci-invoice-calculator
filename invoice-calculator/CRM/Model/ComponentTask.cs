using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk;
using InvoiceCalculation.CRM.Data;

namespace InvoiceCalculation.CRM.Model
{
    public class ComponentTask : EntityBase
    {
        public ComponentTask() : base("new_projecttask") { }
        public ComponentTask(DynamicEntity e) : base(e) { }

        public Guid Id
        {
            get { return base.GetPropertyValue<Guid>("new_projecttaskid", PropertyType.Key, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_projecttaskid", PropertyType.Key, value); }
        }

        public Guid EngagementId
        {
            get { return base.GetPropertyValue<Guid>("new_projectid", PropertyType.Lookup, Guid.Empty); }
            set { base.SetPropertyValue<Guid>("new_projectid", PropertyType.Lookup, value); }
        }

        public String Name
        {
            get { return base.GetPropertyValue<String>("new_name", PropertyType.String, String.Empty); }
            set { base.SetPropertyValue<String>("new_name", PropertyType.String, value); }
        }

        public String TaskCode
        {
            get { return base.GetPropertyValue<String>("new_projectserviceidname", PropertyType.String, String.Empty); }
            set { base.SetPropertyValue<String>("new_projectserviceidname", PropertyType.String, value); }
        }

        public int StateCode
        {
            get { return base.GetPropertyValue<int>("statecode", PropertyType.Number, 0); }
            set { base.SetPropertyValue<int>("statecode", PropertyType.Number, value); }
        }

        public List<TaskTime> GetAllTaskTimes()
        {
            return Data.TaskTime.Retrieve(this);
        }

        public DateTime? CompletedOn()
        {
            var taskTimes = this.GetAllTaskTimes();
            if (taskTimes.Count == 0)
            {
                return null;
            }

            var completedTaskTimes = taskTimes.FindAll(x => x.TaskComplete == true);
            if (completedTaskTimes.Count == 0)
            {
                return null;
            }

            return completedTaskTimes
                .OrderByDescending(x => x.EndTime)
                .ToList()
                .FirstOrDefault()
                .EndTime;
        }
    }
}
