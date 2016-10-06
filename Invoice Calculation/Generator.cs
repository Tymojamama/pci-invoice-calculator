using System;
using System.Collections.Generic;
using System.Data;
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
        private List<Model.Invoice> _generatedInvoices;

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
            Console.WriteLine("Calculation line items");
            this._calculateLineItems();
            Console.WriteLine("Creating and updating invoice records");
            this._createInvoiceRecordsInCrm();
            Console.WriteLine("Process complete");
        }

        private void _calculateInvoices()
        {
            this._generatedInvoices = new List<Model.Invoice>();
            foreach (var engagement in this._engagements.FindAll(x => x.ProductType != -1))
            {
                var invoices = CalculateInvoice(engagement);
                this._generatedInvoices.AddRange(invoices);
            }
        }

        public List<Model.Invoice> CalculateInvoice(Guid engagementId)
        {
            var engagement = this._engagements.Find(x => x.Id == engagementId);
            return CalculateInvoice(engagement);
        }

        public List<Model.Invoice> CalculateInvoice(CRM.Model.Engagement engagement)
        {
            var result = new List<Model.Invoice>();

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
                invoiceFee = invoiceFee + engagement.FixedProjectFeeForInvoicePeriod(this.BillingDate, isNewEngagement);
                invoiceFee = invoiceFee - (engagement.AnnualFeeOffset / 4m);
                
                if (isTerminatedEngagement)
                {
                    var typicalInvoiceFee = Calculator.CalculateInvoiceFee(annualFee, engagementProductType, this.BillingDate, engagementEffectiveDate, false, isNewEngagement, engagementEffectiveDate);
                    invoiceCredit = Calculator.CalculateInvoiceCredit(isTerminatedEngagement, typicalInvoiceFee, engagementTerminationDate, engagementProductType);
                }
            }

            var invoice = CreateInvoiceFromEngagement(engagement, isNewEngagement);
            invoice.AnnualFee = annualFee;
            invoice.InvoiceFee = invoiceFee;
            invoice.InvoiceCredit = invoiceCredit;
            invoice.TotalPlanAssetsUsed = planAssetValue;
            invoice.BillingType = Calculator.GetInvoiceBillingType(engagementProductType, engagementEffectiveDate, this.BillingDate, isNewEngagement);

            if (invoice.BillingType == Model.BillingType.InArrears && isTerminatedEngagement)
            {
                var typicalInvoiceFee = Calculator.CalculateInvoiceFee(annualFee, engagementProductType, this.BillingDate, engagementEffectiveDate, false, isNewEngagement, engagementEffectiveDate);
                invoiceCredit = Calculator.CalculateInvoiceCredit(isTerminatedEngagement, typicalInvoiceFee, engagementTerminationDate, engagementProductType);
                invoice.InvoiceFee = typicalInvoiceFee - invoiceCredit;
                invoice.InvoiceCredit = 0;
            }

            result.Add(invoice);

            // run for next billing period as well
            if (isNewEngagement && engagementProductType.BillingSchedule == "In Advance")
            {
                isNewEngagement = false;
                var newInvoice = CreateInvoiceFromEngagement(engagement, isNewEngagement);
                newInvoice.TotalPlanAssetsUsed = planAssetValue;
                newInvoice.BillingType = Calculator.GetInvoiceBillingType(engagementProductType, engagementEffectiveDate, this.BillingDate, isNewEngagement);
                if (planAssetValue != 0 || erisaVendorProductTypes.Contains(engagement.ProductType))
                {
                    newInvoice.AnnualFee = Calculator.CalculateAnnualFee(engagementProductType, this.BillingDate, engagementEffectiveDate, planAssetValue, engagementTierLevel);
                    newInvoice.InvoiceFee = Calculator.CalculateInvoiceFee(annualFee, engagementProductType, this.BillingDate, engagementEffectiveDate, isTerminatedEngagement, isNewEngagement, engagementEffectiveDate);
                    newInvoice.InvoiceFee = newInvoice.InvoiceFee + engagement.FixedProjectFeeForInvoicePeriod(this.BillingDate, isNewEngagement);
                    newInvoice.InvoiceCredit = Calculator.CalculateInvoiceCredit(isTerminatedEngagement, invoiceFee, engagementTerminationDate, engagementProductType);
                }

                result.Add(newInvoice);
            }

            if (isTerminatedEngagement)
            {

            }

            return result;
        }

        private Model.Invoice CreateInvoiceFromEngagement(CRM.Model.Engagement engagement, bool isNew)
        {
            var invoice = new Model.Invoice();
            invoice.Name = this.BillingDate.ToString("MM/dd/yyyy") + " invoice for " + engagement.Name;
            invoice.BilledOn = this.BillingDate.AddHours(12);
            invoice.StartDate = DateTime.SpecifyKind(engagement.GetInvoicePeriodStartDate(this.BillingDate, isNew), DateTimeKind.Utc).AddHours(12);
            invoice.EndDate = DateTime.SpecifyKind(engagement.GetInvoicePeriodEndDate(this.BillingDate, isNew), DateTimeKind.Utc).AddHours(12);
            invoice.EarnedOn = invoice.EndDate;
            invoice.DaysToPay = engagement.GetDaysToPay();
            invoice.GeneralLedgerAccountId = engagement.GetGeneralLedgerAccountId();
            invoice.EngagementId = engagement.Id;
            invoice.ClientId = engagement.ClientId;
            return invoice;
        }

        private void _calculateLineItems()
        {
            var beginningOfYear = new DateTime(this.BillingDate.Year, 01, 01);
            var invoiceMaster = new Data.InvoiceMaster();
            var main = invoiceMaster.RunMain(beginningOfYear);
            foreach (DataRow row in main.Rows)
            {
                var clientName = row["ClientName"].ToString();
                var taskName = row["TaskName"].ToString();
                var engagement = this._engagements.Find(x => x.Name == clientName);

                if (engagement == null)
                {
                    continue;
                }

                var invoices = this._generatedInvoices.FindAll(x => x.EngagementId == engagement.Id);
                var single = invoiceMaster.RunSingle(clientName, taskName, beginningOfYear);
                foreach (var invoice in invoices)
                {
                    var lineItem = new Model.InvoiceLineItem();
                    foreach (DataRow row2 in single.Rows)
                    {
                        var billableAmount = decimal.Parse(row2["BillableAmount"].ToString());
                        var startTime = DateTime.Parse(row2["StartTime"].ToString());

                        lineItem.Name = taskName + " Additional Billable Amount";
                        lineItem.LineItemType = Model.LineItemType.Fee;

                        // i need make in advanced billing pull amounts from previous quarter
                        if (invoice.BillingType == Model.BillingType.InAdvanced)
                        {
                            if (startTime >= invoice.StartDate.AddMonths(-3) && startTime < invoice.EndDate.AddMonths(-3))
                            {
                                lineItem.Amount = lineItem.Amount + billableAmount;
                            }
                        }
                        else
                        {
                            if (startTime >= invoice.StartDate && startTime < invoice.EndDate)
                            {
                                lineItem.Amount = lineItem.Amount + billableAmount;
                            }
                        }
                    }

                    if (lineItem.Amount > 0)
                    {
                        invoice.LineItems.Add(lineItem);
                    }
                }
            }

            foreach (var invoice in this._generatedInvoices.FindAll(x => x.InvoiceCredit > 0))
            {
                var lineItem = new Model.InvoiceLineItem();
                lineItem.Name = "Automatically Generated Invoice Credit";
                lineItem.LineItemType = Model.LineItemType.Credit;
                lineItem.Amount = invoice.InvoiceCredit;
                lineItem.StartDate = invoice.StartDate;
                lineItem.EndDate = invoice.EndDate;
                //lineItem.GeneralLedgerAccountId = 
                //lineItem.TeamId = 
                invoice.LineItems.Add(lineItem);
            }
        }

        private void _createInvoiceRecordsInCrm()
        {
            var currentInvoices = CRM.Data.Invoice.Retrieve(this.BillingDate.AddHours(12));
            foreach (var invoice in this._generatedInvoices)
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

                var isNew = false;
                if (crmInvoice.Id == Guid.Empty)
                {
                    isNew = true;
                    crmInvoice.Id = Guid.NewGuid();
                }

                CRM.Data.Invoice.Save(crmInvoice, isNew);

                var crmLineItems = CRM.Data.InvoiceLineItem.Retrieve(crmInvoice);

                foreach (var lineItem in invoice.LineItems)
                {
                    var crmLineItem = new CRM.Model.InvoiceLineItem(lineItem);
                    crmLineItem.CustomerInvoiceId = crmInvoice.Id;
                    crmLineItem.StartDate = crmInvoice.StartDate;
                    crmLineItem.EndDate = crmInvoice.EndDate;

                    var matchingLineItem = crmLineItems
                        .FindAll(x => x.LineItemType == crmLineItem.LineItemType)
                        .Find(x => x.Name == crmLineItem.Name);

                    if (matchingLineItem != null)
                    {
                        crmLineItem.Id = matchingLineItem.Id;
                    }

                    CRM.Data.InvoiceLineItem.Save(crmLineItem);
                }
            }
        }
    }
}
