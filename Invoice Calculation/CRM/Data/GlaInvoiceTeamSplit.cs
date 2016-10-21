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
    public static class GlaInvoiceTeamSplit
    {
        private static string _tableName = "new_glainvoiceteamsplit";

        public static List<Model.GlaInvoiceTeamSplit> Retrieve()
        {
            var request = Globals.GetRetrieveMultipleRequest(_tableName);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.GlaInvoiceTeamSplit>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var split = new CRM.Model.GlaInvoiceTeamSplit(dynamicEntity);
                result.Add(split);
            }

            return result;
        }

        public static Model.GlaInvoiceTeamSplit Retrieve(Guid id)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_glainvoiceteamsplitid", ConditionOperator.Equal, id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.GlaInvoiceTeamSplit>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var split = new CRM.Model.GlaInvoiceTeamSplit(dynamicEntity);
                result.Add(split);
            }

            return result.Find(x => x.Id == id);
        }

        /// <summary>
        /// Creates a new record if ID is empty and updates existing records.
        /// </summary>
        /// <param name="invoice"></param>
        public static void Save(Model.GlaInvoiceTeamSplit split, bool isNew = false)
        {
            try
            {
                if (split.Id == Guid.Empty || isNew == true)
                {
                    split.Id = Guid.NewGuid();
                    Globals.CrmServiceBroker.Service.Create(split.GetDynamicEntity());
                }
                else
                {
                    Globals.CrmServiceBroker.Service.Update(split.GetDynamicEntity());
                }
            }
            catch (SoapException soapEx)
            {
                Console.WriteLine("SoapException: " + soapEx.Detail);
                Console.WriteLine("SoapException: " + soapEx.Message);
            }
        }
    }
}
