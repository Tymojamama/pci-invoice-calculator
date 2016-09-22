using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;
using SdkTypeProxy = Microsoft.Crm.SdkTypeProxy;
using Model = InvoiceCalculation.CRM.Model;

namespace InvoiceCalculation.CRM.Data
{
    public static class TaskTime
    {
        private static string _tableName = "new_projecttasktime";

        public static List<Model.TaskTime> Retrieve()
        {
            var request = Globals.GetRetrieveMultipleRequest(_tableName);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.TaskTime>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var taskTime = new CRM.Model.TaskTime(dynamicEntity);
                result.Add(taskTime);
            }

            return result;
        }

        public static Model.TaskTime Retrieve(Guid id)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_projecttasktimeid", ConditionOperator.Equal, id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.TaskTime>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var taskTime = new CRM.Model.TaskTime(dynamicEntity);
                result.Add(taskTime);
            }

            return result.Find(x => x.Id == id);
        }

        public static List<Model.TaskTime> Retrieve(Model.ComponentTask componentTask)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_projecttaskid", ConditionOperator.Equal, componentTask.Id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.TaskTime>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var taskTime = new CRM.Model.TaskTime(dynamicEntity);
                result.Add(taskTime);
            }

            return result.FindAll(x => x.ComponentTaskId == componentTask.Id);
        }
    }
}
