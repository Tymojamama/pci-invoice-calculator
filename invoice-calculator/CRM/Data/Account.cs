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
    public static class Account
    {
        private static string _tableName = "account";

        public static List<Model.Account> Retrieve()
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("customertypecode", ConditionOperator.Equal, 200000);
            criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.Account>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var account = new CRM.Model.Account(dynamicEntity);
                result.Add(account);
            }

            return result;
        }

        public static Model.Account Retrieve(Guid id)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("accountid", ConditionOperator.Equal, id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.Account>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var account = new CRM.Model.Account(dynamicEntity);
                result.Add(account);
            }

            return result.Find(x => x.Id == id);
        }
    }
}
