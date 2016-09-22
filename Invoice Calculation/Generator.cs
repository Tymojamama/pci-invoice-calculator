using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceCalculation
{
    public class Generator
    {
        private List<CRM.Model.Account> _accounts;
        private List<CRM.Model.Engagement> _engagements;
        private List<CRM.Model.PlanAccount> _planAccounts;
        private List<CRM.Model.PlanEngagement> _planEngagements;
        private List<CRM.Model.PlanAsset> _planAssets;
        private List<Model.Invoice> _calculatedInvoices;

        public DateTime BillingDate;

        public Generator(DateTime billingDate)
        {
            Console.WriteLine("Fetching plan and engagement data from CRM");
            this.BillingDate = DateTime.SpecifyKind(billingDate, DateTimeKind.Utc);
            this._accounts = CRM.Data.Account.Retrieve();
            this._engagements = CRM.Data.Engagement.Retrieve()
                .FindAll(x => x.IsWithinDateTime(this.BillingDate));
            this._planAccounts = CRM.Data.PlanAccount.Retrieve();
            this._planEngagements = CRM.Data.PlanEngagement.Retrieve();
            this._planAssets = CRM.Data.PlanAsset.Retrieve();
        }

        /// <summary>
        /// Calculates invoices, confirms values from user, and inserts records into CRM.
        /// </summary>
        public void Run()
        {
            Console.WriteLine("Calculating invoices");
            this._calculateInvoices();
            Console.WriteLine("Creating and updating invoice records");
            this._createInvoiceRecordsInCrm();
            Console.WriteLine("Process complete");
        }

        private void _calculateInvoices()
        {
            this._calculatedInvoices = new List<Model.Invoice>();
            foreach (var engagement in this._engagements.FindAll(x => x.ProductType != -1))
            {
                if (engagement.Id == new Guid("A482F80A-2F80-E611-943A-00155D288102"))
                {

                }
                var engagementProductType = engagement.GetProductTypeDetail();
                var engagementEffectiveDate = (DateTime)engagement.EffectiveDate;
                var engagementTerminationDate = engagement.ContractTerminationDate;
                var engagementTierLevel = (int)engagement.Tier;
                var planAssetValue = engagement.GetAssetsForInvoice(this.BillingDate, this._planEngagements, this._planAccounts, this._planAssets);
                var isNewEngagement = engagement.IsNewOnBillingDate(this.BillingDate);
                var isTerminatedEngagement = engagement.IsTerminatedOnBillingDate(this.BillingDate);

                var annualFee = 0m;
                var invoiceFee = 0m;
                var invoiceCredit = 0m;

                var erisaVendorProductTypes = new int[] { 2, 5, 8, 11, 13, 14 };
                if (planAssetValue != 0 || erisaVendorProductTypes.Contains(engagement.ProductType))
                {
                    annualFee = Calculator.CalculateAnnualFee(engagementProductType, this.BillingDate, engagementEffectiveDate, planAssetValue, engagementTierLevel);
                    invoiceFee = Calculator.CalculateInvoiceFee(annualFee, engagementProductType, this.BillingDate, engagementEffectiveDate, isTerminatedEngagement, isNewEngagement, engagementEffectiveDate);
                    invoiceFee = invoiceFee + engagement.FixedProjectFeeForInvoicePeriod(this.BillingDate);
                    invoiceCredit = Calculator.CalculateInvoiceCredit(isTerminatedEngagement, invoiceFee, engagementTerminationDate, engagementProductType);
                }

                var invoice = CreateInvoiceFromEngagement(engagement);
                invoice.AnnualFee = annualFee;
                invoice.InvoiceFee = invoiceFee;
                invoice.InvoiceCredit = invoiceCredit;
                invoice.TotalPlanAssetsUsed = planAssetValue;
                invoice.BillingType = Calculator.GetInvoiceBillingType(engagementProductType, engagementEffectiveDate, this.BillingDate, isNewEngagement);

                this._calculatedInvoices.Add(invoice);
            
                // run for next billing period as well
                if (isNewEngagement && engagementProductType.BillingSchedule == "In Advance")
                {
                    isNewEngagement = false;
                    var newInvoice = CreateInvoiceFromEngagement(engagement);
                    newInvoice.TotalPlanAssetsUsed = planAssetValue;
                    newInvoice.BillingType = Calculator.GetInvoiceBillingType(engagementProductType, engagementEffectiveDate, this.BillingDate, isNewEngagement);
                    if (planAssetValue != 0 || erisaVendorProductTypes.Contains(engagement.ProductType))
                    {
                        newInvoice.AnnualFee = Calculator.CalculateAnnualFee(engagementProductType, this.BillingDate, engagementEffectiveDate, planAssetValue, engagementTierLevel);
                        newInvoice.InvoiceFee = Calculator.CalculateInvoiceFee(annualFee, engagementProductType, this.BillingDate, engagementEffectiveDate, isTerminatedEngagement, isNewEngagement, engagementEffectiveDate);
                        newInvoice.InvoiceFee = newInvoice.InvoiceFee + engagement.FixedProjectFeeForInvoicePeriod(this.BillingDate);
                        newInvoice.InvoiceCredit = Calculator.CalculateInvoiceCredit(isTerminatedEngagement, invoiceFee, engagementTerminationDate, engagementProductType);
                    }

                    this._calculatedInvoices.Add(newInvoice);
                }

                if (isTerminatedEngagement)
                {

                }
            }
        }

        private Model.Invoice CreateInvoiceFromEngagement(CRM.Model.Engagement engagement)
        {
            var invoice = new Model.Invoice();
            invoice.Name = this.BillingDate.ToString("MM/dd/yyyy") + " invoice for " + engagement.Name;
            invoice.BilledOn = this.BillingDate.AddHours(12);
            invoice.StartDate = DateTime.SpecifyKind(engagement.GetInvoicePeriodStartDate(this.BillingDate), DateTimeKind.Utc).AddHours(12);
            invoice.EndDate = DateTime.SpecifyKind(engagement.GetInvoicePeriodEndDate(this.BillingDate), DateTimeKind.Utc).AddHours(12);
            invoice.EarnedOn = DateTime.SpecifyKind(engagement.GetInvoicePeriodEndDate(this.BillingDate), DateTimeKind.Utc).AddHours(12);
            invoice.DaysToPay = engagement.GetDaysToPay();
            invoice.GeneralLedgerAccountId = engagement.GetGeneralLedgerAccountId();
            invoice.EngagementId = engagement.Id;
            invoice.ClientId = engagement.ClientId;
            return invoice;
        }

        private void _createInvoiceRecordsInCrm()
        {
            var currentInvoices = CRM.Data.Invoice.Retrieve(this.BillingDate.AddHours(12));
            foreach (var invoice in this._calculatedInvoices)
            {
                // insert invoice record into crm with invoice credit line item if applicable
                // update invoice record if it already exists, don't update if statusreason is locked
                // if invoice fee is 0 and invoice credit is 0, don't create invoice

                var crmInvoice = new CRM.Model.Invoice(invoice);

                var currentInvoice = currentInvoices.Find(x => x.EngagementId == invoice.EngagementId && x.BillingType == (int)invoice.BillingType);
                if (invoice.InvoiceFee + invoice.InvoiceCredit == 0)
                {
                    continue;
                }
                else if (currentInvoice != null && currentInvoice.StatusReason == 100000000)
                {
                    continue;
                }
                else if (currentInvoice != null)
                {
                    crmInvoice.Id = currentInvoice.Id;
                }

                CRM.Data.Invoice.Save(crmInvoice);
            }
        }
    }
}
