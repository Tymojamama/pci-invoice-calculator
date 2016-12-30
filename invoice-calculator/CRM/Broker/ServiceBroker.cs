using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.SdkTypeProxy;
using InvoiceCalculation.CRM.Service;

namespace InvoiceCalculation.CRM.Broker
{
    internal class ServiceBroker
    {
        private static Dictionary<String, CrmServiceWrapper> _crmServices = new Dictionary<string, CrmServiceWrapper>();
        private static System.Timers.Timer _timer = new System.Timers.Timer(1800000D);
        private static String _genericCrmTicket = null;

        static ServiceBroker()
        {
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            _timer.AutoReset = true;
            _timer.Start();
        }

        internal static CrmService GetServiceInstance(IAuthenticationRequest authRequest)
        {
            if (_genericCrmTicket != null && _crmServices.ContainsKey(_genericCrmTicket))
            {
                if (_crmServices[_genericCrmTicket].ExpirationDate < DateTime.Now)
                    RemoveServiceInstance(_genericCrmTicket);
                else
                    return _crmServices[_genericCrmTicket].CrmService;
            }

            // create new service instance
            CrmServiceWrapper serviceWrapper = new CrmServiceWrapper(CreateServiceInstance(authRequest));
            if (!String.IsNullOrEmpty(serviceWrapper.CrmService.CrmAuthenticationTokenValue.CrmTicket))
            {
                _crmServices.Add(serviceWrapper.CrmService.CrmAuthenticationTokenValue.CrmTicket, serviceWrapper);
                _genericCrmTicket = serviceWrapper.CrmService.CrmAuthenticationTokenValue.CrmTicket;
            }
            return serviceWrapper.CrmService;
        }

        private static void DisposeExpiredServices()
        {
            bool locked = false;

            DateTime now = DateTime.Now;
            List<String> toRemove = new List<String>();

            try
            {
                foreach (KeyValuePair<String, CrmServiceWrapper> c in _crmServices)
                {
                    if (c.Value.ExpirationDate > now) { continue; }
                    toRemove.Add(c.Key);
                }
            }
            catch (Exception)
            {
                // don't care
            }

            foreach (String c in toRemove)
            {
                locked = System.Threading.Monitor.TryEnter(_crmServices[c].CrmService, 5000);
                if (!locked) { continue; }

                try
                {
                    _crmServices[c].CrmService.Dispose();
                }
                catch (Exception)
                {
                    // don't care
                }
                locked = false;
                _crmServices.Remove(c);
            }
        }

        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            DisposeExpiredServices();
        }

        private static void RemoveServiceInstance(String crmTicket)
        {
            _crmServices[crmTicket].CrmService.Dispose();
            _crmServices.Remove(crmTicket);
        }

        private static CrmService CreateServiceInstance(IAuthenticationRequest authRequest)
        {
            //took out discovery service as the backward compatible discovery service does not work well with ADFS.  

            CrmService serviceClient;
            using (TokenBroker tokenBroker = new TokenBroker())
            {
                OrganizationDetail orgInfo = tokenBroker.GetOrganizationDetail();
                serviceClient = new CrmService()
                {
                    Url = orgInfo.CrmServiceUrl//,.ServiceUrl,
                    //CrmAuthenticationTokenValue = orgInfo.CrmToken
                };

                switch (Globals.CrmServiceSettings.DeploymentType)
                {
                    case AuthenticationType.AD:
                        CrmAuthenticationToken ADToken = new CrmAuthenticationToken();
                        ADToken.AuthenticationType = 0; //ActiveDirectory
                        ADToken.OrganizationName = orgInfo.OrganizationName;
                        serviceClient.CrmAuthenticationTokenValue = ADToken;
                        serviceClient.UnsafeAuthenticatedConnectionSharing = true;
                        serviceClient.PreAuthenticate = true;


                        //serviceClient.UseDefaultCredentials = true;
                        //serviceClient.Credentials = System.Net.CredentialCache.DefaultCredentials;
                        serviceClient.Credentials = new System.Net.NetworkCredential(authRequest.Username, authRequest.Password, authRequest.DomainName);
                        //serviceClient.CrmAuthenticationTokenValue.CallerId = new Guid("97945673-0ac2-e111-9748-000423c7d319");
                        //serviceClient.PreAuthenticate = true;
                        break;
                    default:
                        TokenBroker.Token token = tokenBroker.RequestToken(authRequest);
                        serviceClient.CrmAuthenticationTokenValue = token.CrmToken;
                        break;
                }
            }

            authRequest.WasRefreshed = true;
            return serviceClient;
        }

        private class CrmServiceWrapper
        {
            private DateTime _expirationDate = DateTime.Now.AddMinutes(30D);

            internal CrmService CrmService { get; set; }
            internal DateTime ExpirationDate
            {
                get
                {
                    return _expirationDate;
                }
            }

            internal CrmServiceWrapper(CrmService crmService)
            {
                this.CrmService = crmService;
            }
        }
    }
}
