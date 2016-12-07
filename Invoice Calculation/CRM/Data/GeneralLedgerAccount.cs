using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;
using SdkTypeProxy = Microsoft.Crm.SdkTypeProxy;
using Model = InvoiceCalculation.CRM.Model;

namespace InvoiceCalculation.CRM.Data
{
    public static class GeneralLedgerAccount
    {
        private static string _tableName = "new_generalledgeraccount";

        public static List<Model.GeneralLedgerAccount> Retrieve()
        {
            var request = Globals.GetRetrieveMultipleRequest(_tableName);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.GeneralLedgerAccount>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var gla = new CRM.Model.GeneralLedgerAccount(dynamicEntity);
                result.Add(gla);
            }

            return result;
        }

        public static Model.GeneralLedgerAccount Retrieve(Guid id)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_generalledgeraccountid", ConditionOperator.Equal, id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.GeneralLedgerAccount>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var gla = new CRM.Model.GeneralLedgerAccount(dynamicEntity);
                result.Add(gla);
            }

            return result.Find(x => x.Id == id);
        }
    }
}
