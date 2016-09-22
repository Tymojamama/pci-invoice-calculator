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
    public static class Invoice
    {
        private static string _tableName = "new_customerinvoice";

        public static List<Model.Invoice> Retrieve()
        {
            var request = Globals.GetRetrieveMultipleRequest(_tableName);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.Invoice>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var invoice = new CRM.Model.Invoice(dynamicEntity);
                result.Add(invoice);
            }

            return result;
        }

        public static Model.Invoice Retrieve(Guid id)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_customerinvoiceid", ConditionOperator.Equal, id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.Invoice>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var invoice = new CRM.Model.Invoice(dynamicEntity);
                result.Add(invoice);
            }

            return result.Find(x => x.Id == id);
        }

        /// <summary>
        /// Retrieves a list of Invoices with a specified billing date
        /// </summary>
        /// <param name="billingDate"></param>
        /// <returns></returns>
        public static List<Model.Invoice> Retrieve(DateTime billingDate)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_billedon", ConditionOperator.On, billingDate);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.Invoice>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var invoice = new CRM.Model.Invoice(dynamicEntity);
                result.Add(invoice);
            }

            return result;
        }

        /// <summary>
        /// Creates a new record if ID is empty and updates existing records.
        /// </summary>
        /// <param name="invoice"></param>
        public static void Save(Model.Invoice invoice)
        {
            try
            {
                if (invoice.Id == Guid.Empty)
                {
                    invoice.Id = Guid.NewGuid();
                    Globals.CrmServiceBroker.Service.Create(invoice.GetDynamicEntity());
                }
                else
                {
                    Globals.CrmServiceBroker.Service.Update(invoice.GetDynamicEntity());
                }
            }
            catch(SoapException soapEx)
            {
                Console.WriteLine("SoapException: " + soapEx.Detail);
                Console.WriteLine("SoapException: " + soapEx.Message);
            }
        }
    }
}
