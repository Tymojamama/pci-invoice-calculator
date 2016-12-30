using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data = InvoiceCalculation.Data;
using Models = InvoiceCalculation.Model;

namespace InvoiceCalculation.Test.FeeSchedule
{
    public enum UnitTestType
    {
        NewEngagement,
        TerminatedEngagement,
        ExistingEngagement
    }

    public class UnitTest
    {
        public DateTime BillingDate;
        public DateTime ClientFeeScheduleDate;
        public DateTime? EngagementStartDate;
        public DateTime? TerminationDate;
        public Decimal PlanAssetValue;
        public Decimal ExpectedAnnualFee;
        public Decimal ExpectedInvoiceFee;
        public Decimal? ExpectedCredit;
        public int TierLevel;
        public bool IsActive;
        public Model.ProductType ProductType;
        public UnitTestType UnitTestType;

        public UnitTest()
        {

        }
    }
}
