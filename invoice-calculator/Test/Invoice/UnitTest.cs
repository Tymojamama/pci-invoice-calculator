using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data = InvoiceCalculation.Data;
using Models = InvoiceCalculation.Model;

namespace InvoiceCalculation.Test.Invoice
{
    public class UnitTest
    {
	    public Guid EngagementId;
	    public DateTime BilledOn;
	    public DateTime EarnedOn;
	    public DateTime StartDate;
	    public DateTime EndDate;
	    public int DaysToPay;
	    public int BillingType;
	    public decimal AnnualFee;
	    public decimal CalculatedFee;
        public decimal TotalPlanAssetsUsed;

        public UnitTest()
        {

        }
    }
}
