using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;
using SdkTypeProxy = Microsoft.Crm.SdkTypeProxy;
using InvoiceCalculation.CRM.Model;
using InvoiceCalculation.CRM.Exceptions;
using InvoiceCalculation.CRM.Broker;

namespace InvoiceCalculation.CRM.Data
{
    public abstract class DataObjectBase<TDataObject, TDataKey> where TDataObject : EntityBase
    {
        protected String[] ColumnSet { get; set; }
        protected String EntityName { get; set; }

        protected DataObjectBase(string entityName, string[] columnSet)
        {
            this.EntityName = entityName;
            if (columnSet != null) { ColumnSet = columnSet; }
        }

        protected DynamicEntity GetUniqueResult(List<DynamicEntity> list)
        {
            if (list.Count > 1)
                throw new TooManyResultsException();
            else if (list.Count == 0)
                return null;
            return list.FirstOrDefault();
        }
    }

    public abstract class ServiceObjectBase<TDataObject, TDataKey> : DataObjectBase<TDataObject, TDataKey> where TDataObject : EntityBase
    {
        protected IAuthenticationRequest _authRequest;

        public ServiceObjectBase(IAuthenticationRequest authRequest, String entityName, String[] columnSet)
            : base(entityName, columnSet)
        {
            _authRequest = authRequest;
        }

        public Guid Create(TDataObject dataObject)
        {
            return ServiceBroker.GetServiceInstance(_authRequest).Create(dataObject.GetDynamicEntity());
        }

        public void Update(TDataObject dataObject)
        {
            ServiceBroker.GetServiceInstance(_authRequest).Update(dataObject.GetDynamicEntity());
        }

        public void Delete(TDataObject dataObject)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid dataObjectId)
        {
            ServiceBroker.GetServiceInstance(_authRequest).Delete(base.EntityName, dataObjectId);
        }

        public BusinessEntity Retrieve(Guid dataObjectId)
        {
            return ServiceBroker.GetServiceInstance(_authRequest).Retrieve(base.EntityName, dataObjectId, new ColumnSet(base.ColumnSet));
        }

        public BusinessEntity Retrieve(Guid dataObjectId, String[] columnSet)
        {
            ColumnSetBase cs;
            if (columnSet == null || columnSet.Length == 0)
                cs = new AllColumns();
            else
                cs = new ColumnSet(columnSet);
            return ServiceBroker.GetServiceInstance(_authRequest).Retrieve(base.EntityName, dataObjectId, cs);
        }

        public Object Execute(SdkTypeProxy.Request obj)
        {
            return ServiceBroker.GetServiceInstance(_authRequest).Execute(obj);
        }

        public List<DynamicEntity> RetrieveMultiple(FilterExpression filterExpression, OrderExpression orderExpression)
        {
            return RetrieveMultiple(filterExpression, orderExpression, base.ColumnSet);
        }

        public List<DynamicEntity> RetrieveMultiple(FilterExpression filterExpression)
        {
            return RetrieveMultiple(filterExpression, null);
        }

        public List<DynamicEntity> RetrieveMultiple(FilterExpression filterExpression, OrderExpression orderExpression, String[] columnSet)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = base.EntityName,
                Criteria = filterExpression,
                ColumnSet = new ColumnSet(columnSet)
            };
            if (orderExpression != null)
                query.Orders.Add(orderExpression);

            var request = new SdkTypeProxy.RetrieveMultipleRequest()
            {
                ReturnDynamicEntities = true,
                Query = query
            };

            SdkTypeProxy.CrmService service = ServiceBroker.GetServiceInstance(_authRequest);
            SdkTypeProxy.RetrieveMultipleResponse retrieveMultipleResponse = (SdkTypeProxy.RetrieveMultipleResponse)service.Execute(request);
            return retrieveMultipleResponse.BusinessEntityCollection.BusinessEntities.Select(e => (DynamicEntity)e).ToList();
        }

        public List<DynamicEntity> RetrieveMultiple(QueryExpression queryExpression)        {
            if (queryExpression == null)
                throw new ArgumentNullException("The 'queryExpression' argument cannot be null.");

            var request = new SdkTypeProxy.RetrieveMultipleRequest()
            {
                ReturnDynamicEntities = true,
                Query = queryExpression
            };

            var service = ServiceBroker.GetServiceInstance(_authRequest);
            return ((SdkTypeProxy.RetrieveMultipleResponse)service.Execute(request)).BusinessEntityCollection.BusinessEntities.Select(e => (DynamicEntity)e).ToList();
        }

        public XDocument Fetch(string fetchXml)
        {
            var service = ServiceBroker.GetServiceInstance(_authRequest);
            string resultXml = service.Fetch(fetchXml);

            return XDocument.Parse(resultXml);
        }

    }
}
