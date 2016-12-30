using System;
using System.Linq;
using System.Net;
using System.Security;
using System.Web.Services.Protocols;
using Microsoft.Crm.Sdk;
using InvoiceCalculation.CRM.Model;
using InvoiceCalculation.CRM.Exceptions;
using InvoiceCalculation.CRM.Service;

namespace InvoiceCalculation.CRM.Broker
{
    internal class TokenBroker : IDisposable
    {
        private CrmDiscoveryService _disco = new CrmDiscoveryService() { Url = Globals.CrmServiceSettings.ServiceUrl };

        public Token RequestToken(IAuthenticationRequest tokenRequest)
        {
            // Check that we're accessing a valid organization from the CrmDiscoveryService Web service.
            if (String.IsNullOrEmpty(tokenRequest.OrganizationName))
            {
                OrganizationDetail orgInfo = GetOrganizationDetail();
                tokenRequest.OrganizationName = orgInfo.OrganizationName;
            }

            // Retrieve a CrmTicket from the CrmDiscoveryService Web service.
            RetrieveCrmTicketResponse crmTicketResponse = GetCrmTicketResponse(tokenRequest);
            if (crmTicketResponse == null)
            {
                // user not found, try default credentials
                IAuthenticationRequest defaultTicketRequest = GetDefaultTicketRequest();
                crmTicketResponse = GetCrmTicketResponse(defaultTicketRequest);
            }

            // user not found at all, throw exception
            if (crmTicketResponse == null)
            {
                InvalidCredentialsException ex = new InvalidCredentialsException();
                ex.Data.Add("Username", tokenRequest.DomainName + @"\" + tokenRequest.Username);
                throw ex;
            }

            CrmAuthenticationToken crmToken = GetCrmAuthenticationToken(tokenRequest, crmTicketResponse);
            return new Token(crmToken, crmTicketResponse.OrganizationDetail.CrmServiceUrl);
        }

        private void AddExceptionData(Exception ex, IAuthenticationRequest tokenRequest)
        {
            if (ex == null)
            {
                return;
            }

            if (!ex.Data.Contains("Organization"))
            {
                ex.Data.Add("Organization", Globals.CrmServiceSettings.OrganizationName);
            }

            if (!ex.Data.Contains("ServiceUrl"))
            {
                ex.Data.Add("ServiceUrl", Globals.CrmServiceSettings.ServiceUrl);
            }

            if (tokenRequest != null)
            {
                if (!ex.Data.Contains("DomainName"))
                {
                    ex.Data.Add("DomainName", Globals.CrmServiceSettings.DomainName);
                }

                if (!ex.Data.Contains("Username"))
                {
                    ex.Data.Add("Username", tokenRequest.Username);
                }
            }
        }

        private IAuthenticationRequest GetDefaultTicketRequest()
        {
            return new TokenRequest()
            {
                DomainName = Globals.CrmServiceSettings.DomainName,
                Password = Globals.CrmServiceSettings.Password,
                Username = Globals.CrmServiceSettings.Username,
                OrganizationName = Globals.CrmServiceSettings.OrganizationName
            };
        }

        private CrmAuthenticationToken GetCrmAuthenticationToken(IAuthenticationRequest tokenRequest, RetrieveCrmTicketResponse ticket)
        {
            Int32 authenticationType;

#if DEBUG_AuthOverride
            authenticationType = Globals.CrmServiceSettings.DeploymentType;
#else
            if (String.IsNullOrEmpty(tokenRequest.DomainName))
            {
                // this is a CRM user, don't authenticate with their credentials
                authenticationType = Globals.CrmServiceSettings.DeploymentType;
            }
            else
            {
                // this is a domain user, authenticate via AD
                authenticationType = AuthenticationType.AD;
            }
#endif

            CrmAuthenticationToken sdkToken = new CrmAuthenticationToken()
            {
                AuthenticationType = authenticationType,
                OrganizationName = ticket.OrganizationDetail.OrganizationName,
                CrmTicket = ticket.CrmTicket
            };
            return sdkToken;
        }

        private RetrieveCrmTicketResponse GetCrmTicketResponse(IAuthenticationRequest tokenRequest)
        {
            // Retrieve a CrmTicket from the CrmDiscoveryService Web service.
            RetrieveCrmTicketRequest crmTicketRequest = new RetrieveCrmTicketRequest();
            crmTicketRequest.OrganizationName = tokenRequest.OrganizationName;
            crmTicketRequest.UserId = tokenRequest.DomainName + @"\" + tokenRequest.Username;
            crmTicketRequest.Password = Globals.UnwrapSecureString(tokenRequest.Password);

            RetrieveCrmTicketResponse crmTicketResponse = null;
            try
            {
                crmTicketResponse = (RetrieveCrmTicketResponse)_disco.Execute(crmTicketRequest);
            }
            catch (SoapException se)
            {
                // examine SoapException for authentication failure
            }
            catch (Exception ex)
            {
                AddExceptionData(ex, tokenRequest);
                throw;
            }
            return crmTicketResponse;
        }

        internal OrganizationDetail GetOrganizationDetail()
        {
            // Retrieve a list of available organizations from the CrmDiscoveryService Web service.

            RetrieveOrganizationsRequest orgRequest = new RetrieveOrganizationsRequest
            {
                UserId = Globals.CrmServiceSettings.DomainName + @"\" + Globals.CrmServiceSettings.Username,
                Password = Globals.UnwrapSecureString(Globals.CrmServiceSettings.Password)
            };

            // Make request to organization service using global credentials.

            if (Globals.CrmServiceSettings.DeploymentType == 0)
            {
                // _disco.PreAuthenticate = true;
                //_disco.UnsafeAuthenticatedConnectionSharing = true;
                _disco.Credentials = new NetworkCredential(Globals.CrmServiceSettings.Username, orgRequest.Password, Globals.CrmServiceSettings.DomainName);
            }

            RetrieveOrganizationsResponse orgResponse = (RetrieveOrganizationsResponse)_disco.Execute(orgRequest);

            // Find the target organization.
            OrganizationDetail orgInfo = orgResponse.OrganizationDetails.FirstOrDefault(p => p.OrganizationName.ToLower() == Globals.CrmServiceSettings.OrganizationName.ToLower());

            // Check whether a matching organization was not found.
            if (orgInfo == null)
            {
                ApplicationException ex = new ApplicationException("The specified organization was not found.");
                ex.Data.Add("Organization", Globals.CrmServiceSettings.OrganizationName);
                throw ex;
            }
            return orgInfo;
        }

        private class TokenRequest : IAuthenticationRequest
        {
            public string Username { get; set; }
            public SecureString Password { get; set; }
            public string DomainName { get; set; }
            public bool WasRefreshed { get; set; }
            public string CrmTicket { get; set; }
            public string OrganizationName { get; set; }
        }

        public class Token
        {
            private CrmAuthenticationToken _crmToken;
            private String _serviceUrl;

            public Token(CrmAuthenticationToken crmToken, String serviceUrl)
            {
                _crmToken = crmToken;
                _serviceUrl = serviceUrl;
            }

            public CrmAuthenticationToken CrmToken
            {
                get
                {
                    return _crmToken;
                }
            }

            public String ServiceUrl
            {
                get
                {
                    return _serviceUrl;
                }
            }

        }

        public void Dispose()
        {
            if (_disco != null)
                _disco.Dispose();
        }
    }
}
