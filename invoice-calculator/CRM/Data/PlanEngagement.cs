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
    public static class PlanEngagement
    {
        private static string _tableName = "new_new_plan_new_project";

        public static List<Model.PlanEngagement> Retrieve()
        {
            var request = Globals.GetRetrieveMultipleRequest(_tableName);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.PlanEngagement>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var planEngagement = new CRM.Model.PlanEngagement(dynamicEntity);
                result.Add(planEngagement);
            }

            return result;
        }

        public static Model.PlanEngagement Retrieve(Guid id)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_new_plan_new_projectid", ConditionOperator.Equal, id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.PlanEngagement>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var planEngagement = new CRM.Model.PlanEngagement(dynamicEntity);
                result.Add(planEngagement);
            }

            return result.Find(x => x.Id == id);
        }
    }
}
