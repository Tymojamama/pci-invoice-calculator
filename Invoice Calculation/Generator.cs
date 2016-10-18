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
            this.Engagements = this._filterEngagementsByBillingFrequency();
            this.PlanAccounts = CRM.Data.PlanAccount.Retrieve();
            this.PlanEngagements = CRM.Data.PlanEngagement.Retrieve();
            this.PlanAssets = CRM.Data.PlanAsset.Retrieve();
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

            Console.WriteLine("Calculating invoices");
            this._calculateInvoices();
            Console.WriteLine("Calculating line items");
            this._calculateLineItems();
            Console.WriteLine("Creating and updating invoice records");
            this._createInvoiceRecordsInCrm();
            Console.WriteLine("Process complete");
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
                    var lineItem = new Model.InvoiceLineItem();
                    foreach (DataRow row2 in single.Rows)
                    {
                        var billableHours = decimal.Parse(row2["BillableHours"].ToString());
                        var billableAmount = decimal.Parse(row2["BillableAmount"].ToString());
                        var startTime = DateTime.Parse(row2["StartTime"].ToString());

                        lineItem.Name = taskName;
                        lineItem.LineItemType = Model.LineItemType.Fee;

                        // i need make in advanced billing pull amounts from previous quarter
                        if (invoice.BillingType == Model.BillingType.InAdvanced)
                        {
                            var range = (invoice.EndDate - invoice.StartDate).TotalDays - 3;
                            var billingStartDate = invoice.StartDate.AddDays(range * -1);
                            billingStartDate = new DateTime(billingStartDate.Year, billingStartDate.Month, 1);
                            var billingEndDate = invoice.StartDate.Date.AddSeconds(-1);

                            if (startTime >= billingStartDate && startTime < billingEndDate)
                            {
                                lineItem.Amount = lineItem.Amount + billableAmount;

                                if (String.IsNullOrWhiteSpace(lineItem.Description))
                                {
                                    lineItem.Description = billableHours.ToString("0.00") + " Hours";
                                }
                                else
                                {
                                    var hours = decimal.Parse(lineItem.Description.Replace(" Hours", ""));
                                    lineItem.Description = (hours + billableHours).ToString("0.00") + " Hours";
                                }
                            }
                            lineItem.StartDate = DateTime.SpecifyKind(billingStartDate, DateTimeKind.Utc).AddHours(12);
                            lineItem.EndDate = DateTime.SpecifyKind(billingEndDate, DateTimeKind.Utc).AddSeconds(1).AddHours(-12);
                        }
                        else
                        {
                            if (startTime >= invoice.StartDate && startTime < invoice.EndDate)
                            {
                                lineItem.Amount = lineItem.Amount + billableAmount;
                            }

                            lineItem.StartDate = invoice.StartDate;
                            lineItem.EndDate = invoice.EndDate;
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

                var currentInvoice = currentInvoices
                    .FindAll(x => x.EngagementId == invoice.EngagementId)
                    .Find(x => x.BillingType == (int)invoice.BillingType);

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
