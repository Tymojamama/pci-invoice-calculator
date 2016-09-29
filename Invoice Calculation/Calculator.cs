using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceCalculation
{
    public class Calculator
    {
        public static decimal CalculateAnnualFee(Model.ProductType productType, DateTime billingDate, DateTime clientFeeScheduleDate, decimal planAssetValue, int tierLevel = 0)
        {
            var annualFee = 0m;
            annualFee = Program.CalculateAnnualFee(productType, billingDate, clientFeeScheduleDate, planAssetValue, tierLevel);
            return annualFee;
        }

        public static decimal CalculateOriginalInvoiceFee(decimal annualFee, Model.ProductType productType, DateTime billingDate, DateTime clientFeeScheduleDate)
        {
            return Program.CalculateInvoiceFee(annualFee, productType, billingDate, clientFeeScheduleDate);
        }

        public static decimal CalculateInvoiceFee(decimal annualFee, Model.ProductType productType, DateTime billingDate, DateTime clientFeeScheduleDate, bool isTerminatedEngagement, bool isNewEngagement, DateTime engagementStartDate)
        {
            var invoiceFee = CalculateOriginalInvoiceFee(annualFee, productType, billingDate, clientFeeScheduleDate);

            if (isTerminatedEngagement && isNewEngagement)
            {
                throw new NotImplementedException();
            }
            else if (isTerminatedEngagement)
            {
                invoiceFee = 0m;
            }
            else if (isNewEngagement)
            {
                invoiceFee = Program.ProRataInvoiceFeeForNew(productType, invoiceFee, (DateTime)engagementStartDate);
            }

            return invoiceFee;
        }

        public static decimal CalculateInvoiceCredit(bool isTerminatedEngagement, decimal invoiceFee, DateTime? terminationDate, Model.ProductType productType)
        {
            var invoiceCredit = 0m;
            if (isTerminatedEngagement)
            {
                invoiceCredit = Program.CalculateCredit(invoiceFee, (DateTime)terminationDate, productType);
            }
            return invoiceCredit;
        }


        public static InvoiceCalculation.Model.BillingType GetInvoiceBillingType(Model.ProductType productType, DateTime clientFeeScheduleDate, DateTime billingDate, bool isNew)
        {
            var result = Model.BillingType.InAdvanced;
            var feeSchedules = Data.FeeSchedule
                .GetFeeSchedules(productType, clientFeeScheduleDate, billingDate)
                .OrderByDescending(x => x.EndDate ?? DateTime.MaxValue)
                .ThenByDescending(x => x.StartDate)
                .ToList();

            if (feeSchedules.Count == 0)
            {
                if (productType.BillingSchedule == "In Arrears")
                {
                    result = Model.BillingType.InArrears;
                }
                else if (productType.BillingSchedule == "In Advanced")
                {
                    result = Model.BillingType.InAdvanced;
                }
                else
                {
                    result = Model.BillingType.InAdvanced;
                    //throw new Exception("Product Type does not have a valid billing schedule");
                }

                if (isNew && result == Model.BillingType.InAdvanced)
                {
                    return Model.BillingType.InArrears;
                }

                return result;
            }

            result = feeSchedules[0].BillingType;

            if (isNew && result == Model.BillingType.InAdvanced)
            {
                return Model.BillingType.InArrears;
            }

            return result;
        }
    }
}
