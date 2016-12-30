using System;
using System.Collections.Generic;
using System.Text;

namespace InvoiceCalculation.CRM.Exceptions
{
    public class VspDataLayerException : ApplicationException
    {
        private const String _message = "An exception has occurred during a data request.";

        internal VspDataLayerException()
            : base(_message)
        {
        }

        internal VspDataLayerException(String message)
            : base(String.IsNullOrEmpty(message) ? _message : message)
        {
        }

        internal VspDataLayerException(Exception innerException)
            : base(_message, innerException)
        {
        }

        internal VspDataLayerException(String message, Exception innerException)
            : base(String.IsNullOrEmpty(message) ? _message : message, innerException)
        {
        }
    }

    public class TicketExpiredException : VspDataLayerException
    {
        private const string _message = "The authentication ticket presented to the CRM service has expired.";
        internal TicketExpiredException()
            : base(_message)
        {
        }
    }

    public class InvalidTicketException : VspDataLayerException
    {
        private const string _message = "A valid ticket must be presented with the data request.";
        internal InvalidTicketException()
            : base(_message)
        {
        }
    }

    public class InvalidEntityException : VspDataLayerException
    {
        private const string _message = "An invalid entity was presented.";
        internal InvalidEntityException()
            : base(_message)
        {
        }
    }

    public class InvalidCredentialsException : VspDataLayerException
    {
        private const string _message = "Invalid credentials were supplied to the CRM service.";
        internal InvalidCredentialsException()
            : base(_message)
        {
        }
    }

    public class TooManyResultsException : VspDataLayerException
    {
        private const string _message = "Multiple results were found where a unique result was expected.";
        internal TooManyResultsException()
            : base(_message)
        {
        }
    }
}
