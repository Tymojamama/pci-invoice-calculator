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

            this.calculateInvoiceFeesAndCredits();
        }

        public List<Model.Invoice> GetInvoices()
        {
            var result = new List<Model.Invoice>();
            
            var main = this.getInvoiceMain();

            var termAfter = this._engagementTerminationDate.Date > this.getPreviousBillingDate().Date;
            var termBefore = this._engagementTerminationDate.Date < this._generator.BillingDate.Date;
            var termPreviousBillingCycle = termBefore && termAfter;
            var isInAdvanced = main != null && main.BillingType == Model.BillingType.InAdvanced;

            if (main != null && main.StartDate < this._engagement.ContractTerminationDate)
            {
                result.Add(main);
            }
            else if (termPreviousBillingCycle && isInAdvanced)
            {
                var previousTermination = this.getInvoiceMain(this.getPreviousBillingDate().Date, Model.BillingType.InArrears);
                result.Add(previousTermination);
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
                else if (this._engagementTerminationDate.Date > this.getPreviousBillingDate().Date)
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
                var date = this._generator.BillingDate.AddMonths(-3);
                var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                return new DateTime(date.Year, date.Month, daysInMonth);
            }
            else if (this._engagementProductType.BillingFrequency == "Monthly")
            {
                var date = this._generator.BillingDate.AddMonths(-1);
                var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                return new DateTime(date.Year, date.Month, daysInMonth);
            }
            else
            {
                var date = this._generator.BillingDate.AddMonths(-12);
                var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                return new DateTime(date.Year, date.Month, daysInMonth);
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
            var terminationDate = this._engagementTerminationDate.Date;
            if (this._engagementProductType.BillingFrequency == "Quarterly")
            {
                var beginQuarterMonths = new int[] { 1, 4, 7, 10 };
                var endQuarterMonths = new int[] { 3, 6, 9, 12 };
                var beginingOfMonth = terminationDate.Day == 1;
                var endOfMonth = terminationDate.Day == DateTime.DaysInMonth(terminationDate.Year,terminationDate.Month);

                if (beginingOfMonth && beginQuarterMonths.Contains(terminationDate.Month) == true)
                {

                }
                else if (endOfMonth && endQuarterMonths.Contains(terminationDate.Month) == true)
                {

                }
                else
                {
                    terminationDate = terminationDate.AddDays(-1);
                }
            }
            else if (this._engagementProductType.BillingFrequency == "Monthly")
            {
                var beginingOfMonth = terminationDate.Day == 1;
                var endOfMonth = terminationDate.Day == DateTime.DaysInMonth(terminationDate.Year, terminationDate.Month);

                if (beginingOfMonth == false && endOfMonth == false)
                {
                    terminationDate = terminationDate.AddDays(-1);
                }
            }
            else
            {
                if (terminationDate.Day == 1 && terminationDate.Month == 1)
                {

                }
                else if (terminationDate.Day == 31 && terminationDate.Month == 12)
                {

                }
                else
                {
                    terminationDate = terminationDate.AddDays(-1);
                }
            }

            var typicalInvoiceFee = Calculator.CalculateInvoiceFee(this._annualFee, this._engagementProductType, this._generator.BillingDate, this._engagementEffectiveDate, false, this._isNewEngagement, this._engagementEffectiveDate);
            var invoiceCredit = Calculator.CalculateInvoiceCredit(this._isTerminatedEngagement, typicalInvoiceFee, terminationDate, this._engagementProductType);
            invoice.InvoiceFee = typicalInvoiceFee - invoiceCredit;
            invoice.InvoiceCredit = 0;
            return invoice;
        }

        private Model.Invoice getInvoiceMain(DateTime? billingDate = null, Model.BillingType? billingType = null)
        {
            var invoice = this.createInvoiceFromEngagement(billingType);
            invoice.IsMainInvoice = true;
            invoice.AnnualFee = this._annualFee;
            invoice.InvoiceFee = this._invoiceFee;
            invoice.InvoiceCredit = this._invoiceCredit;
            invoice.TotalPlanAssetsUsed = this._planAssetValue;

            if (billingDate == null)
            {
                invoice.BillingType = Calculator.GetInvoiceBillingType(this._engagementProductType, this._engagementEffectiveDate, this._generator.BillingDate, this._isNewEngagement);
            }
            else
            {
                invoice.BillingType = Calculator.GetInvoiceBillingType(this._engagementProductType, this._engagementEffectiveDate, (DateTime)billingDate, this._isNewEngagement);
            }

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
            var newInvoice = this.createInvoiceFromEngagement(Model.BillingType.InAdvanced);
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

        private Model.Invoice createInvoiceFromEngagement(Model.BillingType? billingType = null)
        {
            var invoice = new Model.Invoice();
            invoice.Name = this._generator.BillingDate.ToString("MM/dd/yyyy") + " invoice for " + this._engagement.Name;
            invoice.BilledOn = this._generator.BillingDate.AddHours(12);
            invoice.StartDate = DateTime.SpecifyKind(this._engagement.GetInvoicePeriodStartDate(this._generator.BillingDate, this._isNewEngagement, billingType), DateTimeKind.Utc).AddHours(12);
            invoice.EndDate = DateTime.SpecifyKind(this._engagement.GetInvoicePeriodEndDate(this._generator.BillingDate, this._isNewEngagement, billingType), DateTimeKind.Utc).AddHours(12);
            invoice.EarnedOn = invoice.EndDate;
            invoice.DaysToPay = this._engagement.GetDaysToPay();
            invoice.GeneralLedgerAccountId = this._engagement.GetGeneralLedgerAccountId();
            invoice.EngagementId = this._engagement.Id;
            invoice.ClientId = this._engagement.ClientId;
            return invoice;
        }
    }
}
