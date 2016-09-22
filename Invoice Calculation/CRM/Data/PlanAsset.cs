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
    public static class PlanAsset
    {
        private static string _tableName = "new_planasset";

        public static List<Model.PlanAsset> Retrieve()
        {
            var request = Globals.GetRetrieveMultipleRequest(_tableName);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.PlanAsset>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var planAsset = new CRM.Model.PlanAsset(dynamicEntity);
                result.Add(planAsset);
            }

            return result;
        }

        public static Model.PlanAsset Retrieve(Guid id)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_planassetid", ConditionOperator.Equal, id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.PlanAsset>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var planAsset = new CRM.Model.PlanAsset(dynamicEntity);
                result.Add(planAsset);
            }

            return result.Find(x => x.Id == id);
        }
    }
}
