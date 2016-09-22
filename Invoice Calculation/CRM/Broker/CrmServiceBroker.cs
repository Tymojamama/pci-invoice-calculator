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

        /*
        var password = new System.Security.SecureString();
        foreach (Char c in "PCItri13!".ToCharArray())
        {
            password.AppendChar(c);
        }
        password.MakeReadOnly();

        var settings = new CRM.CrmServiceSettings ()
        {
            OrganizationName = "TestEnvironment",
            DeploymentType = 2,
            DomainName = "pension1",
            Password = password,
            Username = "tricension",
            ServiceUrl = "https://dev.pension-consultants.com/MSCRMServices/2007/SPLA/CrmDiscoveryService.asmx"
        };

        CRM.Globals.Initialize(settings);

        var request = new CRM.Model.AuthenticationRequest()
        {
            CrmTicket = CRM.Globals.CrmServiceSettings.ServiceUrl,
            DomainName = CRM.Globals.CrmServiceSettings.DomainName,
            OrganizationName = CRM.Globals.CrmServiceSettings.OrganizationName,
            Password = CRM.Globals.CrmServiceSettings.Password,
            Username = CRM.Globals.CrmServiceSettings.Username
        };

        var planLogic = new CRM.Data.PlanAccount(request);
        var plans = planLogic.Retrieve();
        */
    }
}
