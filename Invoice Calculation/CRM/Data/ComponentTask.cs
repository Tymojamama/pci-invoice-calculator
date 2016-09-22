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
    public static class ComponentTask
    {
        private static string _tableName = "new_projecttask";

        public static List<Model.ComponentTask> Retrieve()
        {
            var request = Globals.GetRetrieveMultipleRequest(_tableName);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.ComponentTask>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var componentTask = new CRM.Model.ComponentTask(dynamicEntity);
                result.Add(componentTask);
            }

            return result;
        }

        public static Model.ComponentTask Retrieve(Guid id)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_projecttaskid", ConditionOperator.Equal, id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.ComponentTask>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var componentTask = new CRM.Model.ComponentTask(dynamicEntity);
                result.Add(componentTask);
            }

            return result.Find(x => x.Id == id);
        }

        public static List<Model.ComponentTask> Retrieve(Model.Engagement engagement)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_projectid", ConditionOperator.Equal, engagement.Id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.ComponentTask>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var componentTask = new CRM.Model.ComponentTask(dynamicEntity);
                result.Add(componentTask);
            }

            return result.FindAll(x => x.EngagementId == engagement.Id);
        }
    }
}
