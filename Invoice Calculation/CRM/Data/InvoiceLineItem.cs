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
    public static class InvoiceLineItem
    {
        private static string _tableName = "new_customerinvoicelineitem";

        public static List<Model.InvoiceLineItem> Retrieve()
        {
            var request = Globals.GetRetrieveMultipleRequest(_tableName);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.InvoiceLineItem>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var invoiceLineItem = new CRM.Model.InvoiceLineItem(dynamicEntity);
                result.Add(invoiceLineItem);
            }

            return result;
        }

        public static Model.InvoiceLineItem Retrieve(Guid id)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_customerinvoicelineitemid", ConditionOperator.Equal, id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.InvoiceLineItem>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var invoiceLineItem = new CRM.Model.InvoiceLineItem(dynamicEntity);
                result.Add(invoiceLineItem);
            }

            return result.Find(x => x.Id == id);
        }

        public static List<Model.InvoiceLineItem> Retrieve(Model.Invoice invoice)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("new_customerinvoiceid", ConditionOperator.Equal, invoice.Id);

            var request = Globals.GetRetrieveMultipleRequest(_tableName, criteria);
            var retrieveMultipleResponse = Globals.CrmServiceBroker.ExecuteRetrieveMultipleRequest(request);
            var businessEntityCollection = retrieveMultipleResponse.BusinessEntityCollection;

            var result = new List<Model.InvoiceLineItem>();
            foreach (var businessEntity in businessEntityCollection.BusinessEntities)
            {
                var dynamicEntity = (DynamicEntity)businessEntity;
                var invoiceLineItem = new CRM.Model.InvoiceLineItem(dynamicEntity);
                result.Add(invoiceLineItem);
            }

            return result.FindAll(x => x.CustomerInvoiceId == invoice.Id);
        }

        /// <summary>
        /// Creates a new record if ID is empty and updates existing records.
        /// </summary>
        /// <param name="invoiceLineItem"></param>
        public static void Save(Model.InvoiceLineItem invoiceLineItem)
        {
            try
            {
                if (invoiceLineItem.Id == Guid.Empty)
                {
                    invoiceLineItem.Id = Guid.NewGuid();
                    Globals.CrmServiceBroker.Service.Create(invoiceLineItem.GetDynamicEntity());
                }
                else
                {
                    Globals.CrmServiceBroker.Service.Update(invoiceLineItem.GetDynamicEntity());
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
