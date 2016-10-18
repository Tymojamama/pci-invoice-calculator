using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceCalculation
{
    public class GeneratorInvoice
    {
        private readonly Generator _generator;
        private readonly CRM.Model.Engagement _engagement;
        private readonly Model.ProductType _engagementProductType;
        private readonly DateTime _engagementEffectiveDate;
        private readonly DateTime _engagementTerminationDate;
        private readonly int _engagementTierLevel;
        private readonly decimal _planAssetValue;
        private readonly bool _isNewEngagement;
        private readonly bool _isTerminatedEngagement;

        private decimal _annualFee = 0m;
        private decimal _invoiceFee = 0m;
        private decimal _invoiceCredit = 0m;

        public GeneratorInvoice(CRM.Model.Engagement engagement, Generator generator)
        {
            this._generator = generator;
            this._engagement = engagement;
            this._engagementProductType = engagement.GetProductTypeDetail();
            this._engagementEffectiveDate = (DateTime)engagement.EffectiveDate;
            this._engagementTerminationDate = engagement.ContractTerminationDate;
            this._engagementTierLevel = (int)engagement.Tier;
            this._planAssetValue = engagement.GetAssetsForInvoice(generator.BillingDate, generator.PlanEngagements, generator.PlanAccounts, generator.PlanAssets);
            this._isNewEngagement = engagement.IsNewOnBillingDate(generator.BillingDate);
            this._isTerminatedEngagement = engagement.IsTerminatedOnBillingDate(generator.BillingDate);

            if (this._engagement.Name == "Lexicon Management Group, Inc. 3(21) CC 1601")
            {

            }

            this.calculateInvoiceFeesAndCredits();
        }

        public List<Model.Invoice> GetInvoices()
        {
            var result = new List<Model.Invoice>();

            var main = this.getInvoiceMain();
            if (main != null && main.StartDate < this._engagement.ContractTerminationDate)
            {
                result.Add(main);
            }

            if (this._isNewEngagement && this._engagementProductType.BillingSchedule == "In Advance")
            {
                result.Add(this.getInvoiceNextBillingPeriod());
            }

            if (this.isBillingTypeSwitch())
            {
                if (this._engagementTerminationDate == DateTime.MaxValue)
                {
                    result.Add(this.getInvoiceBillingTypeSwitch());
                }
                else if (this._engagementTerminationDate >= this.getPreviousBillingDate())
                {
                    result.Add(this.getInvoiceBillingTypeSwitch());
                }
            }

            return result;
        }

        private Model.Invoice getInvoiceBillingTypeSwitch()
        {
            var previousBillingDate = this.getPreviousBillingDate();
            var newInvoice = this.getInvoiceMain(Model.BillingType.InArrears);
            newInvoice.IsMainInvoice = false;
            newInvoice.StartDate = DateTime.SpecifyKind(this._engagement.GetInvoicePeriodStartDate(previousBillingDate, this._isNewEngagement, Model.BillingType.InAdvanced), DateTimeKind.Utc).AddHours(12);
            newInvoice.EndDate = DateTime.SpecifyKind(this._engagement.GetInvoicePeriodEndDate(previousBillingDate, this._isNewEngagement, Model.BillingType.InAdvanced), DateTimeKind.Utc).AddHours(12);
            newInvoice.EarnedOn = newInvoice.EndDate;
            newInvoice.BillingType = Model.BillingType.InArrears;
            return newInvoice;
        }

        private DateTime getPreviousBillingDate()
        {
            if (this._engagementProductType.BillingFrequency == "Quarterly")
            {
                return this._generator.BillingDate.AddMonths(-3);
            }
            else if (this._engagementProductType.BillingFrequency == "Monthly")
            {
                return this._generator.BillingDate.AddMonths(-1);
            }
            else
            {
                return this._generator.BillingDate.AddMonths(-12);
            }
        }

        private bool isBillingTypeSwitch()
        {
            var isBillingTypeSwitch = false;
            var previousBillingDate = this.getPreviousBillingDate();

            if (this._engagementProductType.BillingSchedule != "In Advance")
            {
                return false;
            }

            if (this._engagementProductType.BillingFrequency == "Quarterly")
            {
                var feeSchedules = Data.FeeSchedule.GetFeeSchedules(this._engagementProductType, this._engagementEffectiveDate, this._generator.BillingDate);
                var previousFeeSchedules = Data.FeeSchedule.GetFeeSchedules(this._engagementProductType, this._engagementEffectiveDate, previousBillingDate);
                if (feeSchedules.Count == 0 || previousFeeSchedules.Count == 0)
                {

                }
                else if (feeSchedules[0].BillingType != previousFeeSchedules[0].BillingType)
                {
                    isBillingTypeSwitch = true;
                }
            }
            else if (this._engagementProductType.BillingFrequency == "Monthly")
            {
                var feeSchedules = Data.FeeSchedule.GetFeeSchedules(this._engagementProductType, this._engagementEffectiveDate, this._generator.BillingDate);
                var previousFeeSchedules = Data.FeeSchedule.GetFeeSchedules(this._engagementProductType, this._engagementEffectiveDate, previousBillingDate);
                if (feeSchedules.Count == 0 || previousFeeSchedules.Count == 0)
                {

                }
                else if (feeSchedules[0].BillingType != previousFeeSchedules[0].BillingType)
                {
                    isBillingTypeSwitch = true;
                }
            }

            return isBillingTypeSwitch;
        }

        private Model.Invoice modifyForInArrearsTermination(Model.Invoice invoice)
        {
            var typicalInvoiceFee = Calculator.CalculateInvoiceFee(this._annualFee, this._engagementProductType, this._generator.BillingDate, this._engagementEffectiveDate, false, this._isNewEngagement, this._engagementEffectiveDate);
            var invoiceCredit = Calculator.CalculateInvoiceCredit(this._isTerminatedEngagement, typicalInvoiceFee, this._engagementTerminationDate.AddDays(-1), this._engagementProductType);
            invoice.InvoiceFee = typicalInvoiceFee - invoiceCredit;
            invoice.InvoiceCredit = 0;
            return invoice;
        }

        private Model.Invoice getInvoiceMain()
        {
            var invoice = this.createInvoiceFromEngagement();
            invoice.IsMainInvoice = true;
            invoice.AnnualFee = this._annualFee;
            invoice.InvoiceFee = this._invoiceFee;
            invoice.InvoiceCredit = this._invoiceCredit;
            invoice.TotalPlanAssetsUsed = this._planAssetValue;
            invoice.BillingType = Calculator.GetInvoiceBillingType(this._engagementProductType, this._engagementEffectiveDate, this._generator.BillingDate, this._isNewEngagement);

            if (invoice.BillingType == Model.BillingType.InArrears && this._isTerminatedEngagement)
            {
                invoice = this.modifyForInArrearsTermination(invoice);
            }

            if (this.isInvalidInvoicePeriodForEngagement(invoice))
            {
                return null;
            }

            return invoice;
        }

        private Model.Invoice getInvoiceMain(Model.BillingType billingType)
        {
            var invoice = this.createInvoiceFromEngagement();
            invoice.IsMainInvoice = true;
            invoice.AnnualFee = this._annualFee;
            invoice.InvoiceFee = this._invoiceFee;
            invoice.InvoiceCredit = this._invoiceCredit;
            invoice.TotalPlanAssetsUsed = this._planAssetValue;
            invoice.BillingType = billingType;

            if (invoice.BillingType == billingType && this._isTerminatedEngagement)
            {
                invoice = this.modifyForInArrearsTermination(invoice);
            }

            if (this.isInvalidInvoicePeriodForEngagement(invoice))
            {
                return null;
            }

            return invoice;
        }

        /// <summary>
        /// Verifies that there should be an invoice given the invoice period, and
        /// modifies vendor search invoice fee where applicable.
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        /// <remarks>Kind of an awkward method. Needs refactoring.</remarks>
        private bool isInvalidInvoicePeriodForEngagement(Model.Invoice invoice)
        {
            // Vendor Search
            if (this._engagement.ProductType == 11)
            {
                if (this._engagement.StateCode == "Active")
                {
                    if (invoice.StartDate.Date <= this._engagement.EffectiveDate.Date && invoice.EndDate.Date > this._engagement.EffectiveDate.Date)
                    {
                        invoice.InvoiceFee = invoice.AnnualFee / 2m;
                        return false;
                    }
                    else if (invoice.StartDate.Date <= this._engagement.EffectiveDate.Date.AddMonths(5) && invoice.EndDate.Date > this._engagement.EffectiveDate.Date.AddMonths(5))
                    {
                        invoice.InvoiceFee = invoice.AnnualFee / 2m;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private void calculateInvoiceFeesAndCredits()
        {
            var erisaVendorProductTypes = new int[] { 2, 5, 8, 11, 13, 14 };
            if (this._planAssetValue != 0 || erisaVendorProductTypes.Contains(this._engagement.ProductType))
            {
                this._annualFee = Calculator.CalculateAnnualFee(this._engagementProductType, this._generator.BillingDate, this._engagementEffectiveDate, this._planAssetValue, this._engagementTierLevel);
                this._invoiceFee = Calculator.CalculateInvoiceFee(this._annualFee, this._engagementProductType, this._generator.BillingDate, this._engagementEffectiveDate, this._isTerminatedEngagement, this._isNewEngagement, this._engagementEffectiveDate);
                this._invoiceFee = this._invoiceFee + this._engagement.FixedProjectFeeForInvoicePeriod(this._generator.BillingDate, this._isNewEngagement);
                this._invoiceFee = this._invoiceFee - (this._engagement.AnnualFeeOffset / 4m);

                if (this._isTerminatedEngagement)
                {
                    var typicalInvoiceFee = Calculator.CalculateInvoiceFee(this._annualFee, this._engagementProductType, this._generator.BillingDate, this._engagementEffectiveDate, false, this._isNewEngagement, this._engagementEffectiveDate);
                    this._invoiceCredit = Calculator.CalculateInvoiceCredit(this._isTerminatedEngagement, typicalInvoiceFee, this._engagementTerminationDate, this._engagementProductType);
                }
            }
        }

        private int[] getErisaVendorProducTypes()
        {
            return new int[] { 2, 5, 8, 11, 13, 14 };
        }

        private Model.Invoice getInvoiceNextBillingPeriod()
        {
            var newInvoice = this.createInvoiceFromEngagement();
            newInvoice.TotalPlanAssetsUsed = this._planAssetValue;
            newInvoice.BillingType = Calculator.GetInvoiceBillingType(this._engagementProductType, this._engagementEffectiveDate, this._generator.BillingDate, false);
            if (this._planAssetValue != 0 || this.getErisaVendorProducTypes().Contains(this._engagement.ProductType))
            {
                newInvoice.AnnualFee = Calculator.CalculateAnnualFee(this._engagementProductType, this._generator.BillingDate, this._engagementEffectiveDate, this._planAssetValue, this._engagementTierLevel);
                newInvoice.InvoiceFee = Calculator.CalculateInvoiceFee(this._annualFee, this._engagementProductType, this._generator.BillingDate, this._engagementEffectiveDate, this._isTerminatedEngagement, false, this._engagementEffectiveDate);
                newInvoice.InvoiceFee = newInvoice.InvoiceFee + this._engagement.FixedProjectFeeForInvoicePeriod(this._generator.BillingDate, false);
                newInvoice.InvoiceCredit = Calculator.CalculateInvoiceCredit(this._isTerminatedEngagement, this._invoiceFee, this._engagementTerminationDate, this._engagementProductType);
            }

            return newInvoice;
        }

        private Model.Invoice createInvoiceFromEngagement()
        {
            var invoice = new Model.Invoice();
            invoice.Name = this._generator.BillingDate.ToString("MM/dd/yyyy") + " invoice for " + this._engagement.Name;
            invoice.BilledOn = this._generator.BillingDate.AddHours(12);
            invoice.StartDate = DateTime.SpecifyKind(this._engagement.GetInvoicePeriodStartDate(this._generator.BillingDate, this._isNewEngagement), DateTimeKind.Utc).AddHours(12);
            invoice.EndDate = DateTime.SpecifyKind(this._engagement.GetInvoicePeriodEndDate(this._generator.BillingDate, this._isNewEngagement), DateTimeKind.Utc).AddHours(12);
            invoice.EarnedOn = invoice.EndDate;
            invoice.DaysToPay = this._engagement.GetDaysToPay();
            invoice.GeneralLedgerAccountId = this._engagement.GetGeneralLedgerAccountId();
            invoice.EngagementId = this._engagement.Id;
            invoice.ClientId = this._engagement.ClientId;
            return invoice;
        }
    }
}
