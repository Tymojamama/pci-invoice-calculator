using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;
using SdkTypeProxy = Microsoft.Crm.SdkTypeProxy;

namespace InvoiceCalculation.CRM.Broker
{
    public class CrmServiceBroker
    {
        private SdkTypeProxy.CrmService _service = null;

        private void _init()
        {
            var token = new CrmAuthenticationToken();
            token.AuthenticationType = 0;
            token.OrganizationName = "testenvironment";

            var service = new SdkTypeProxy.CrmService();
            service.Url = "http://pci-app/mscrmservices/2007/crmservice.asmx";
            service.CrmAuthenticationTokenValue = token;
            service.Credentials = System.Net.CredentialCache.DefaultCredentials;

            _service = service;
        }

        public SdkTypeProxy.CrmService Service
        {
            get
            {
                if (this._service == null)
                {
                    _init();
                }

                return _service;
            }
            set
            {
                _service = value;
            }
        }

        public SdkTypeProxy.RetrieveMultipleResponse ExecuteRetrieveMultipleRequest(SdkTypeProxy.Request request)
        {
            return (SdkTypeProxy.RetrieveMultipleResponse)this.Service.Execute(request);
        }
    }
}
