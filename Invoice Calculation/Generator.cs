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
        public readonly List<CRM.Model.Account> Accounts;
        public readonly List<CRM.Model.Engagement> Engagements;
        public readonly List<CRM.Model.PlanAccount> PlanAccounts;
        public readonly List<CRM.Model.PlanEngagement> PlanEngagements;
        public readonly List<CRM.Model.PlanAsset> PlanAssets;
        public readonly List<CRM.Model.GeneralLedgerAccount> GeneralLedgerAccounts;
        public readonly List<CRM.Model.GlaInvoiceTeamSplit> GlaInvoiceTeamSplits;
        public readonly List<CRM.Model.GlaInvoiceTeamSplit> GlaInvoiceTeamSplitsAccounts;
        public readonly List<CRM.Model.GlaInvoiceTeamSplit> GlaInvoiceTeamSplitsInvoices;
        public readonly List<CRM.Model.GlaInvoiceTeamSplit> GlaInvoiceTeamSplitsInvoiceLineItems;
        private List<Model.Invoice> _generatedInvoices;

        public DateTime BillingDate;
        private string _billingFrequency;

        public Generator(DateTime billingDate)
        {
            this.BillingDate = DateTime.SpecifyKind(billingDate, DateTimeKind.Utc);
            if (!this._isValidBillingDate())
            {
                return;
            }

            Console.WriteLine("Fetching plan and engagement data from CRM");
            this.Accounts = CRM.Data.Account.Retrieve();
            this.Engagements = CRM.Data.Engagement.Retrieve()
                .FindAll(x => x.IsWithinDateTime(this.BillingDate))
                .FindAll(x => x.ProductType != -1);

            // set parent engagement field
            foreach (var engagement in this.Engagements)
            {
                var otherEngagements = this.Engagements.FindAll(x => x.ClientId == engagement.ClientId);

                foreach (var otherEngagement in otherEngagements)
                {
                    var productType = otherEngagement.GetProductTypeDetail();
                    if (productType.IsServiceOngoing)
                    {
                        if (engagement.GetProductTypeDetail().IsTieredRate == false && engagement.GetProductTypeDetail().IsServiceOngoing == false && engagement.ProductType != 11) //vs
                        {
                            engagement.HasParentEngagement = true;
                        }
                    }
                }
            }

            this.Engagements = this._filterEngagementsByBillingFrequency();
            this.PlanAccounts = CRM.Data.PlanAccount.Retrieve();
            this.PlanEngagements = CRM.Data.PlanEngagement.Retrieve();
            this.PlanAssets = CRM.Data.PlanAsset.Retrieve();
            this.GeneralLedgerAccounts = CRM.Data.GeneralLedgerAccount.Retrieve();
            this.GlaInvoiceTeamSplits = CRM.Data.GlaInvoiceTeamSplit.Retrieve();
            this.GlaInvoiceTeamSplitsAccounts = this.GlaInvoiceTeamSplits.FindAll(x => x.AccountId != Guid.Empty);
            this.GlaInvoiceTeamSplitsInvoices = this.GlaInvoiceTeamSplits.FindAll(x => x.InvoiceId != Guid.Empty);
            this.GlaInvoiceTeamSplitsInvoiceLineItems = this.GlaInvoiceTeamSplits.FindAll(x => x.InvoiceLineItemId != Guid.Empty);
        }

        /// <summary>
        /// Calculates invoices, confirms values from user, and inserts records into CRM.
        /// </summary>
        public void Run()
        {
            if (String.IsNullOrWhiteSpace(this._billingFrequency))
            {
                return;
            }

            var startTime = DateTime.Now;

            Console.WriteLine("Calculating invoices");
            this._calculateInvoices();
            Console.WriteLine("Calculating line items");
            this._calculateLineItems();
            Console.WriteLine("Creating and updating invoice records");
            this._createInvoiceRecordsInCrm();

            var endTime = DateTime.Now;
            var duration = (endTime - startTime);

            var finalMessage = "Process complete: ";
            finalMessage = finalMessage + duration.Minutes.ToString() + "min ";
            finalMessage = finalMessage + duration.Seconds.ToString() + "sec";
            Console.WriteLine(finalMessage);
        }

        public List<Model.Invoice> CalculateInvoice(Guid engagementId)
        {
            var engagement = this.Engagements.Find(x => x.Id == engagementId);
            var generatorInvoiceInstance = new GeneratorInvoice(engagement, this);
            return generatorInvoiceInstance.GetInvoices();
        }

        private List<CRM.Model.Engagement> _filterEngagementsByBillingFrequency()
        {
            if (this._billingFrequency == "Quarterly")
            {
                return this.Engagements
                    .FindAll(x => x.GetProductTypeDetail().BillingFrequency == "Monthly" || x.GetProductTypeDetail().BillingFrequency == "Quarterly");
            }
            else
            {
                return this.Engagements
                    .FindAll(x => x.GetProductTypeDetail().BillingFrequency == this._billingFrequency);
            }
        }

        private bool _isValidBillingDate()
        {
            var firstDayOfMonth = new DateTime(this.BillingDate.Year, this.BillingDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var billingDateQuarter = (this.BillingDate.Month - 1) / 3 + 1;
            DateTime firstDayOfQuarter = new DateTime(this.BillingDate.Year, (billingDateQuarter - 1) * 3 + 1, 1);
            DateTime lastDayOfQuarter = firstDayOfQuarter.AddMonths(3).AddDays(-1);

            var billingFrequency = "";
            if (this.BillingDate.Date == lastDayOfQuarter.Date)
            {
                billingFrequency = "Quarterly";
            }
            else if (this.BillingDate.Date == lastDayOfMonth.Date)
            {
                billingFrequency = "Monthly";
            }
            else
            {
                Console.WriteLine("Unsupported billing date. Please use end of month dates only.");
                Console.WriteLine("Aborting process.");
                return false;
            }

            this._billingFrequency = billingFrequency;
            return true;
        }

        private void _calculateInvoices()
        {
            this._generatedInvoices = new List<Model.Invoice>();
            foreach (var engagement in this.Engagements)
            {
                var generatorInvoiceInstance = new GeneratorInvoice(engagement, this);
                var invoices = generatorInvoiceInstance.GetInvoices();
                this._generatedInvoices.AddRange(invoices);
            }
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

                DateTime? overageApprovedOn = null;
                if (String.IsNullOrWhiteSpace(row["OverageApprovedOn"].ToString()) == false)
                {
                    overageApprovedOn = DateTime.Parse(row["OverageApprovedOn"].ToString());
                }

                var engagement = this.Engagements.Find(x => x.Name == clientName);

                if (engagement == null)
                {
                    continue;
                }

                var invoices = this._generatedInvoices
                    .FindAll(x => x.EngagementId == engagement.Id)
                    .FindAll(x => x.IsMainInvoice == true);

                var single = invoiceMaster.RunSingle(clientName, taskName, beginningOfYear);

                foreach (var invoice in invoices)
                {
                    var workplaceLineItem = new Model.InvoiceLineItem();
                    workplaceLineItem.ApprovedOn = overageApprovedOn;

                    var otherLineItem = new Model.InvoiceLineItem();
                    otherLineItem.ApprovedOn = overageApprovedOn;

                    foreach (DataRow row2 in single.Rows)
                    {
                        var teamId = Guid.Parse(row2["TeamId"].ToString());
                        var overageWorkplaceHours = decimal.Parse(row2["OverageWorkplaceHours"].ToString());
                        var overageWorkplaceAmount = decimal.Parse(row2["OverageWorkplaceAmount"].ToString());
                        var overageOtherHours = decimal.Parse(row2["OverageOtherHours"].ToString());
                        var overageOtherAmount = decimal.Parse(row2["OverageOtherAmount"].ToString());

                        var startTime = DateTime.Parse(row2["StartTime"].ToString());

                        workplaceLineItem.Name = taskName;
                        workplaceLineItem.TeamId = teamId;
                        workplaceLineItem.IsWorkplace = true;
                        workplaceLineItem.LineItemType = Model.LineItemType.Fee;

                        otherLineItem.Name = taskName;
                        otherLineItem.TeamId = teamId;
                        otherLineItem.IsWorkplace = false;
                        otherLineItem.LineItemType = Model.LineItemType.Fee;

                        if (invoice.BillingType == Model.BillingType.InAdvanced)
                        {
                            var range = (invoice.EndDate - invoice.StartDate).TotalDays - 3;
                            var billingStartDate = invoice.StartDate.AddDays(range * -1);
                            billingStartDate = new DateTime(billingStartDate.Year, billingStartDate.Month, 1);
                            var billingEndDate = invoice.StartDate.Date.AddSeconds(-1);

                            if (startTime >= billingStartDate && startTime < billingEndDate)
                            {
                                workplaceLineItem.Amount = workplaceLineItem.Amount + overageWorkplaceAmount;
                                if (String.IsNullOrWhiteSpace(workplaceLineItem.Description))
                                {
                                    workplaceLineItem.Description = overageWorkplaceHours.ToString("0.00") + " Hours";
                                }
                                else
                                {
                                    var hours = decimal.Parse(workplaceLineItem.Description.Replace(" Hours", ""));
                                    workplaceLineItem.Description = (hours + overageWorkplaceHours).ToString("0.00") + " Hours";
                                }

                                otherLineItem.Amount = otherLineItem.Amount + overageOtherAmount;
                                if (String.IsNullOrWhiteSpace(otherLineItem.Description))
                                {
                                    otherLineItem.Description = overageOtherHours.ToString("0.00") + " Hours";
                                }
                                else
                                {
                                    var hours = decimal.Parse(otherLineItem.Description.Replace(" Hours", ""));
                                    otherLineItem.Description = (hours + overageOtherHours).ToString("0.00") + " Hours";
                                }
                            }

                            workplaceLineItem.StartDate = DateTime.SpecifyKind(billingStartDate, DateTimeKind.Utc).AddHours(12);
                            workplaceLineItem.EndDate = DateTime.SpecifyKind(billingEndDate, DateTimeKind.Utc).AddSeconds(1).AddHours(-12);

                            otherLineItem.StartDate = DateTime.SpecifyKind(billingStartDate, DateTimeKind.Utc).AddHours(12);
                            otherLineItem.EndDate = DateTime.SpecifyKind(billingEndDate, DateTimeKind.Utc).AddSeconds(1).AddHours(-12);
                        }
                        else
                        {
                            if (startTime.Date >= invoice.StartDate.Date && startTime.Date < invoice.EndDate.Date)
                            {
                                workplaceLineItem.Amount = workplaceLineItem.Amount + overageWorkplaceAmount;
                                otherLineItem.Amount = otherLineItem.Amount + overageOtherAmount;
                            }

                            workplaceLineItem.StartDate = invoice.StartDate;
                            workplaceLineItem.EndDate = invoice.EndDate;

                            otherLineItem.StartDate = invoice.StartDate;
                            otherLineItem.EndDate = invoice.EndDate;
                        }
                    }

                    if (workplaceLineItem.Amount > 0)
                    {
                        invoice.LineItems.Add(workplaceLineItem);
                    }

                    if (otherLineItem.Amount > 0)
                    {
                        invoice.LineItems.Add(otherLineItem);
                    }
                }
            }

            foreach (var invoice in this._generatedInvoices.FindAll(x => x.InvoiceCredit > 0))
            {
                var engagement = this.Engagements.Find(x => x.Id == invoice.EngagementId);

                var lineItem = new Model.InvoiceLineItem();
                lineItem.Name = "Automatically Generated Invoice Credit";
                lineItem.LineItemType = Model.LineItemType.Credit;
                lineItem.Amount = invoice.InvoiceCredit;
                lineItem.StartDate = invoice.StartDate;
                lineItem.EndDate = invoice.EndDate;
                //invoice.LineItems.Add(lineItem); // removed on 11/21/2016 - will be calculated manually for the time being
            }
        }

        private void _createInvoiceRecordInCrm(Model.Invoice invoice, List<CRM.Model.Invoice> currentInvoices)
        {
            // insert invoice record into crm with invoice credit line item if applicable
            // update invoice record if it already exists, don't update if statusreason is locked
            // if invoice fee is 0 and invoice credit is 0, don't create invoice
            var crmInvoice = new CRM.Model.Invoice(invoice);

            var currentInvoice = currentInvoices
                .FindAll(x => x.EngagementId == invoice.EngagementId)
                .Find(x => x.BillingType == (int)invoice.BillingType);

            if (invoice.InvoiceFee + invoice.InvoiceCredit + invoice.LineItems.Count <= 0)
            {
                return;
            }
            else if (currentInvoice != null && currentInvoice.StatusReason == 100000000)
            {
                return;
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

            this._createInvoiceLineItemRecordsInCrm(crmInvoice, invoice);
            this._createGlaInvoiceTeamSplitRecordsInCrm(crmInvoice, invoice);
        }

        private void _createInvoiceLineItemRecordsInCrm(CRM.Model.Invoice crmInvoice, Model.Invoice invoice)
        {
            var crmLineItems = CRM.Data.InvoiceLineItem.Retrieve(crmInvoice);

            foreach (var lineItem in invoice.LineItems)
            {
                var crmLineItem = new CRM.Model.InvoiceLineItem(lineItem);
                crmLineItem.CustomerInvoiceId = crmInvoice.Id;

                var matchingLineItem = crmLineItems
                    .FindAll(x => x.LineItemType == crmLineItem.LineItemType)
                    .Find(x => x.Name == crmLineItem.Name);

                if (matchingLineItem != null)
                {
                    crmLineItem.Id = matchingLineItem.Id;
                }

                CRM.Data.InvoiceLineItem.Save(crmLineItem);
                this._createGlaInvoiceLineItemTeamSplitRecordsInCrm(crmInvoice, crmLineItem, lineItem);
            }
        }

        private void _createGlaInvoiceLineItemTeamSplitRecordsInCrm(CRM.Model.Invoice crmInvoice, CRM.Model.InvoiceLineItem crmInvoiceLineItem, Model.InvoiceLineItem invoiceLineItem)
        {
            var engagement = this.Engagements.Find(x => x.Id == crmInvoice.EngagementId);

            if (engagement.IsOngoingEngagement() && invoiceLineItem.LineItemType == Model.LineItemType.Fee)
            {
                var split = new CRM.Model.GlaInvoiceTeamSplit();
                split.InvoiceLineItemId = crmInvoiceLineItem.Id;
                split.StartDate = (DateTime)crmInvoiceLineItem.StartDate;
                split.EndDate = (DateTime)crmInvoiceLineItem.EndDate;
                split.TotalSplit = 100.00m;
                split.ProductType = engagement.ProductType;

                if (invoiceLineItem.IsWorkplace && invoiceLineItem.TeamId == new Guid("9FB7C526-7EAF-E311-B765-D8D385C29900")) // Workplace && RetireAdvisers
                {
                    if (invoiceLineItem.ApprovedOn == null || ((DateTime)invoiceLineItem.ApprovedOn).Date.AddMonths(12) > invoiceLineItem.StartDate.Date) // IsNew
                    {
                        split.GeneralLedgerAccountId = this.GeneralLedgerAccounts.Find(x => x.QuickBooksAccountNumber.Equals("4042")).Id;
                    }
                    else // IsLegacy
                    {
                        split.GeneralLedgerAccountId = this.GeneralLedgerAccounts.Find(x => x.QuickBooksAccountNumber.Equals("4046")).Id;
                    }
                }
                else
                {
                    switch (invoiceLineItem.TeamId.ToString().ToUpper())
                    {
                        case "7671B7ED-E4BA-DB11-9697-001143F10531": //ERISA
                            split.GeneralLedgerAccountId = this.GeneralLedgerAccounts.Find(x => x.QuickBooksAccountNumber.Equals("4020")).Id;
                            break;
                        case "7287DE04-E5BA-DB11-9697-001143F10531": //Investment
                            split.GeneralLedgerAccountId = this.GeneralLedgerAccounts.Find(x => x.QuickBooksAccountNumber.Equals("4030")).Id;
                            break;
                        case "9FB7C526-7EAF-E311-B765-D8D385C29900": //RetireAdvisers
                            split.GeneralLedgerAccountId = this.GeneralLedgerAccounts.Find(x => x.QuickBooksAccountNumber.Equals("4040")).Id;
                            break;
                        case "F8ECF41B-E5BA-DB11-9697-001143F10531": //Vendor
                            split.GeneralLedgerAccountId = this.GeneralLedgerAccounts.Find(x => x.QuickBooksAccountNumber.Equals("4050")).Id;
                            break;
                    }
                }

                var splitsInvoice = (CRM.Model.GlaInvoiceTeamSplit)this.GlaInvoiceTeamSplitsInvoiceLineItems
                    .FindAll(x => x.InvoiceLineItemId == crmInvoiceLineItem.Id)
                    .FindAll(x => x.GeneralLedgerAccountId == split.GeneralLedgerAccountId || (x.GeneralLedgerAccountId == Guid.Empty && split.GeneralLedgerAccountId == Guid.Empty))
                    .Find(x => x.StartDate.Date == split.StartDate.Date || x.StartDate.Date == ((DateTime)crmInvoice.StartDate).Date);

                var isNew = true;
                if (splitsInvoice != null)
                {
                    split.Id = splitsInvoice.Id;
                    isNew = false;
                }

                CRM.Data.GlaInvoiceTeamSplit.Save(split, isNew);
            }
            else
            {
                var splitsAccounts = this.GlaInvoiceTeamSplitsAccounts
                    .FindAll(x => x.AccountId == engagement.ClientId)
                    .FindAll(x => x.ProductType == engagement.ProductType)
                    .FindAll(x => x.StartDate.Date <= ((DateTime)crmInvoice.EndDate).Date)
                    .FindAll(x => x.EndDate.Date > ((DateTime)crmInvoice.StartDate).Date);

                foreach (var split in splitsAccounts)
                {
                    var splitsInvoice = (CRM.Model.GlaInvoiceTeamSplit)this.GlaInvoiceTeamSplitsInvoiceLineItems
                        .FindAll(x => x.InvoiceLineItemId == crmInvoiceLineItem.Id)
                        .FindAll(x => x.GeneralLedgerAccountId == split.GeneralLedgerAccountId || (x.GeneralLedgerAccountId == Guid.Empty && split.GeneralLedgerAccountId == Guid.Empty))
                        .Find(x => x.StartDate.Date == split.StartDate.Date || x.StartDate.Date == ((DateTime)crmInvoice.StartDate).Date);

                    var isNew = true;
                    if (splitsInvoice != null)
                    {
                        split.Id = splitsInvoice.Id;
                        isNew = false;
                    }

                    var startDate = split.StartDate;
                    if (split.StartDate.Date <= ((DateTime)crmInvoice.StartDate).Date)
                    {
                        split.StartDate = (DateTime)crmInvoice.StartDate;
                    }
                    else
                    {
                        var coveredDays = (decimal)(((DateTime)crmInvoice.EndDate).Date - split.StartDate.Date).TotalDays;
                        var totalDays = (decimal)(((DateTime)crmInvoice.EndDate).Date - ((DateTime)crmInvoice.StartDate).Date).TotalDays;
                        split.TotalSplit = (coveredDays / totalDays) * 100.00m;
                    }

                    var endDate = split.EndDate;
                    if (split.EndDate.Date >= ((DateTime)crmInvoice.EndDate).Date)
                    {
                        split.EndDate = (DateTime)crmInvoice.EndDate;
                    }
                    else
                    {
                        var coveredDays = (decimal)(split.EndDate.Date - ((DateTime)crmInvoice.StartDate).Date).TotalDays;
                        var totalDays = (decimal)(((DateTime)crmInvoice.EndDate).Date - ((DateTime)crmInvoice.StartDate).Date).TotalDays;
                        split.TotalSplit = (coveredDays / totalDays) * 100.00m;
                    }

                    var accountId = split.AccountId;
                    var invoiceId = split.InvoiceId;

                    split.InvoiceId = Guid.Empty;
                    split.AccountId = Guid.Empty;
                    split.InvoiceLineItemId = crmInvoiceLineItem.Id;
                    CRM.Data.GlaInvoiceTeamSplit.Save(split, isNew);

                    split.AccountId = accountId;
                    split.InvoiceId = invoiceId;
                    split.StartDate = startDate;
                    split.EndDate = endDate;
                }
            }

        }

        private void _createGlaInvoiceTeamSplitRecordsInCrm(CRM.Model.Invoice crmInvoice, Model.Invoice invoice)
        {

            var engagement = this.Engagements.Find(x => x.Id == crmInvoice.EngagementId);
            var splitsAccounts = this.GlaInvoiceTeamSplitsAccounts
                .FindAll(x => x.AccountId == engagement.ClientId)
                .FindAll(x => x.ProductType == engagement.ProductType)
                .FindAll(x => x.StartDate.Date <= ((DateTime)crmInvoice.EndDate).Date)
                .FindAll(x => x.EndDate.Date > ((DateTime)crmInvoice.StartDate).Date);

            foreach (var split in splitsAccounts)
            {
                var splitsInvoice = this.GlaInvoiceTeamSplitsInvoices
                    .FindAll(x => x.InvoiceId == crmInvoice.Id)
                    .FindAll(x => x.GeneralLedgerAccountId == split.GeneralLedgerAccountId || (x.GeneralLedgerAccountId == Guid.Empty && split.GeneralLedgerAccountId == Guid.Empty))
                    .Find(x => x.StartDate.Date == split.StartDate.Date || x.StartDate.Date == ((DateTime)crmInvoice.StartDate).Date);

                var isNew = true;
                if (splitsInvoice != null)
                {
                    split.Id = splitsInvoice.Id;
                    isNew = false;
                }

                var totalSplit = split.TotalSplit;
                var startDate = split.StartDate;
                if (split.StartDate.Date <= ((DateTime)crmInvoice.StartDate).Date)
                {
                    split.StartDate = (DateTime)crmInvoice.StartDate;
                }
                else
                {
                    var coveredDays = 1 + (decimal)(((DateTime)crmInvoice.EndDate).Date - split.StartDate.Date).TotalDays;
                    var totalDays = 1 + (decimal)(((DateTime)crmInvoice.EndDate).Date - ((DateTime)crmInvoice.StartDate).Date).TotalDays;
                    split.TotalSplit = (coveredDays / totalDays) * 100.00m;
                }

                var endDate = split.EndDate;
                if (split.EndDate.Date >= ((DateTime)crmInvoice.EndDate).Date)
                {
                    split.EndDate = (DateTime)crmInvoice.EndDate;
                }
                else
                {
                    var coveredDays = 1 + (decimal)(split.EndDate.Date - ((DateTime)crmInvoice.StartDate).Date).TotalDays;
                    var totalDays = 1 + (decimal)(((DateTime)crmInvoice.EndDate).Date - ((DateTime)crmInvoice.StartDate).Date).TotalDays;
                    split.TotalSplit = (coveredDays / totalDays) * 100.00m;
                }

                var accountId = split.AccountId;
                var invoiceId = split.InvoiceId;
                var invoiceLineItemId = split.InvoiceLineItemId;

                split.InvoiceId = crmInvoice.Id;
                split.AccountId = Guid.Empty;
                split.InvoiceLineItemId = Guid.Empty;
                CRM.Data.GlaInvoiceTeamSplit.Save(split, isNew);

                split.AccountId = accountId;
                split.InvoiceId = invoiceId;
                split.InvoiceLineItemId = invoiceLineItemId;
                split.StartDate = startDate;
                split.EndDate = endDate;
                split.TotalSplit = totalSplit;
            }
        }

        private void _createInvoiceRecordsInCrm()
        {
            var currentInvoices = CRM.Data.Invoice.Retrieve(this.BillingDate.AddHours(12));
            foreach (var invoice in this._generatedInvoices)
            {
                this._createInvoiceRecordInCrm(invoice, currentInvoices);
            }
        }
    }
}
