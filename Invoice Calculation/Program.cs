using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Query;
using SdkTypeProxy = Microsoft.Crm.SdkTypeProxy;
using Data = InvoiceCalculation.Data;
using Model = InvoiceCalculation.Model;

namespace InvoiceCalculation
{
    class Program
    {
        static void Main(string[] args)
        {
            CRM.Globals.Initialize();

            if (args.Length == 0)
            {
                Console.WriteLine("Welcome to the Pension Consultants, Inc. Invoicing Solution");
                Console.WriteLine("Type one of the following commands to get started:");
                Console.WriteLine("  -g   Generate invoices and line items");
                Console.WriteLine("  -t   Run fee schedule and invoice unit tests");

                var selection = Console.ReadLine();
                switch (selection)
                {
                    case "-g":
                        var billingDate = RequestDateTime("Please enter billing date");
                        args = new string[] { "-g", billingDate.ToString("MM/dd/yyyy") };
                        break;
                    case "-t":
                        args = new string[] { "-t" };
                        break;
                    default:
                        Console.WriteLine("Invalid input. Please try again");
                        Main(args);
                        return;
                }
            }

            // run tests
            if (args[0] == "-t" && args.Count() >= 2)
            {
                // test calculation
                if (args[1] == "-c")
                {
                    Test.FeeSchedule.TestMachine.Execute();
                }
                // test invoice generator
                else if (args[1] == "-i")
                {
                    var testMachine = new Test.Invoice.TestMachine();
                    testMachine.Execute();
                }
            }
            else if (args[0] == "-t")
            {
                Test.FeeSchedule.TestMachine.Execute();
                Console.WriteLine();
                var testMachine = new Test.Invoice.TestMachine();
                testMachine.Execute();
            }
            // calculate and populate all invoices
            else if (args[0] == "-g" && args.Count() >= 2)
            {
                RunGenerator(args);
            }
            else
            {
                RunCalculator();
            }

            Console.WriteLine("The program has completed all operations. You may press enter to close this window.");
            Console.ReadLine();
        }

        public static void RunGenerator(string[] args)
        {
            var billingDate = new DateTime();
            if (args.Length > 1)
            {
                try
                {
                    billingDate = DateTime.Parse(args[1]);
                }
                catch
                {
                    Console.WriteLine("Program failure: Argument did not parse to datetime");
                    return;
                }

                if (Test.FeeSchedule.TestMachine.Execute())
                {
                    var generator = new Generator(billingDate);
                    generator.Run();
                }
                else
                {
                    Console.WriteLine("Program failure: Calculator did not pass automated tests");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Program failure: Calculator was not given a billing date");
                Console.WriteLine("Example: InvoiceCalculation -g 03/31/2016");
                return;
            }
        }

        public static void RunCalculatorWithNew()
        {
            var productType = RequestProductType();

            if (productType.BillingSchedule == "In Advance" && productType.BillingFrequency == "Quarterly")
            {
                var billingDate = RequestDateTime("Please enter billing date");
                var clientFeeScheduleDate = RequestDateTime("Please enter client fee schedule date");
                var startDate = RequestDateTime("Please enter engagement start date");
                var assetValue = RequestDecimal("Please enter plan asset value");

                var annualFee = 0m;
                if (productType.IsTierOnly == 1)
                {
                    var tierLevel = RequestInt("Please enter engagement tier level");
                    annualFee = CalculateAnnualFee(productType, billingDate, clientFeeScheduleDate, assetValue, tierLevel);
                }
                else
                {
                    annualFee = CalculateAnnualFee(productType, billingDate, clientFeeScheduleDate, assetValue);
                }

                var invoiceFee = CalculateInvoiceFee(annualFee, productType, billingDate, clientFeeScheduleDate);
                var actualFee = ProRataInvoiceFeeForNew(productType, invoiceFee, startDate);

                Console.WriteLine("Annual fee is " + string.Format("{0:C}", annualFee));
                Console.WriteLine("Invoice fee is " + string.Format("{0:C}", actualFee));
            }
            else
            {
                RunCalculator(productType);
            }
        }

        /// <summary>
        /// Calculates invoice fee of a terminated engagement.
        /// </summary>
        public static void RunCalculatorWithTermination()
        {
            var productType = RequestProductType();

            if (productType.BillingSchedule == "In Advance" && productType.BillingFrequency == "Quarterly")
            {
                var terminationDate = RequestDateTime("Please enter engagement termination date");

                var billingDate = RequestDateTime("Please enter billing date");
                var clientFeeScheduleDate = RequestDateTime("Please enter client fee schedule date");
                var assetValue = RequestDecimal("Please enter plan asset value");

                var annualFee = 0m;
                if (productType.IsTierOnly == 1)
                {
                    var tierLevel = RequestInt("Please enter engagement tier level");
                    annualFee = CalculateAnnualFee(productType, billingDate, clientFeeScheduleDate, assetValue, tierLevel);
                }
                else
                {
                    annualFee = CalculateAnnualFee(productType, billingDate, clientFeeScheduleDate, assetValue);
                }

                var invoiceFee = CalculateInvoiceFee(annualFee, productType, billingDate, clientFeeScheduleDate);
                var credit = CalculateCredit(invoiceFee, terminationDate, productType);

                //Console.WriteLine("Remaining Days: " + remainingDays.ToString());
                //Console.WriteLine("Total Days: " + totalDays.ToString());
                //Console.WriteLine("Percentage Remaining: " + (remainingDays / totalDays));

                Console.WriteLine("Annual fee is " + string.Format("{0:C}", annualFee));
                Console.WriteLine("Invoice fee is " + string.Format("{0:C}", 0d));
                Console.WriteLine("Credit to apply is " + string.Format("{0:C}", credit));
            }
            else
            {
                RunCalculator(productType);
            }
        }

        /// <summary>
        /// Calculates invoice fee.
        /// </summary>
        public static void RunCalculator()
        {
            var productType = RequestProductType();

            var billingDate = RequestDateTime("Please enter billing date");
            var clientFeeScheduleDate = RequestDateTime("Please enter client fee schedule date");
            var assetValue = RequestDecimal("Please enter plan asset value");

            var annualFee = 0m;
            if (productType.IsTierOnly == 1)
            {
                var tierLevel = RequestInt("Please enter engagement tier level");
                annualFee = CalculateAnnualFee(productType, billingDate, clientFeeScheduleDate, assetValue, tierLevel);
            }
            else
            {
                annualFee = CalculateAnnualFee(productType, billingDate, clientFeeScheduleDate, assetValue);
            }

            Console.WriteLine("Annual fee is " + string.Format("{0:C}", annualFee));

            var invoiceFee = CalculateInvoiceFee(annualFee, productType, billingDate, clientFeeScheduleDate);
            Console.WriteLine("Invoice fee is " + string.Format("{0:C}", invoiceFee));
        }

        /// <summary>
        /// Recursively runs the invoice calculator program until the console is closed.
        /// </summary>
        /// <param name="productType"></param>
        public static void RunCalculator(Model.ProductType productType)
        {
            var billingDate = RequestDateTime("Please enter billing date");
            var clientFeeScheduleDate = RequestDateTime("Please enter client fee schedule date");
            var assetValue = RequestDecimal("Please enter plan asset value");

            var annualFee = 0m;
            if (productType.IsTierOnly == 1)
            {
                var tierLevel = RequestInt("Please enter engagement tier level");
                annualFee = CalculateAnnualFee(productType, billingDate, clientFeeScheduleDate, assetValue, tierLevel);
            }
            else
            {
                annualFee = CalculateAnnualFee(productType, billingDate, clientFeeScheduleDate, assetValue);
            }

            Console.WriteLine("Annual fee is " + string.Format("{0:C}", annualFee));

            var invoiceFee = CalculateInvoiceFee(annualFee, productType, billingDate, clientFeeScheduleDate);
            Console.WriteLine("Invoice fee is " + string.Format("{0:C}", invoiceFee));
        }

        public static decimal ProRataInvoiceFeeForNew(Model.ProductType productType, decimal invoiceFee, DateTime startDate)
        {
            // Vendor Monitoring
            if (productType.BillingLength == "Annual")
            {
                return invoiceFee;
            }

            var beginningOfQuarter1 = DateTime.Parse("01/01/" + startDate.Year.ToString());
            var endingOfQuarter1 = DateTime.Parse("03/31/" + startDate.Year.ToString());
            var beginningOfQuarter2 = DateTime.Parse("04/01/" + startDate.Year.ToString());
            var endingOfQuarter2 = DateTime.Parse("06/30/" + startDate.Year.ToString());
            var beginningOfQuarter3 = DateTime.Parse("07/01/" + startDate.Year.ToString());
            var endingOfQuarter3 = DateTime.Parse("09/30/" + startDate.Year.ToString());
            var beginningOfQuarter4 = DateTime.Parse("10/01/" + startDate.Year.ToString());
            var endingOfQuarter4 = DateTime.Parse("12/31/" + startDate.Year.ToString());
            var beginningOfQuarter5 = DateTime.Parse("01/01/" + (startDate.Year + 1).ToString());

            // only beginning of day for accuracy
            startDate = startDate.Date;

            var totalDays = 0d;
            var remainingDays = 0d;

            if (startDate >= beginningOfQuarter1 && startDate <= endingOfQuarter1)
            {
                totalDays = (beginningOfQuarter2 - beginningOfQuarter1).TotalDays;
                remainingDays = (beginningOfQuarter2 - startDate).TotalDays;
            }
            else if (startDate >= beginningOfQuarter2 && startDate <= endingOfQuarter2)
            {
                totalDays = (beginningOfQuarter3 - beginningOfQuarter2).TotalDays;
                remainingDays = (beginningOfQuarter3 - startDate).TotalDays;
            }
            else if (startDate >= beginningOfQuarter3 && startDate <= endingOfQuarter3)
            {
                totalDays = (beginningOfQuarter4 - beginningOfQuarter3).TotalDays;
                remainingDays = (beginningOfQuarter4 - startDate).TotalDays;
            }
            else if (startDate >= beginningOfQuarter4 && startDate <= endingOfQuarter4)
            {
                totalDays = (beginningOfQuarter5 - beginningOfQuarter4).TotalDays;
                remainingDays = (beginningOfQuarter5 - startDate).TotalDays;
            }

            return invoiceFee * (decimal)(remainingDays / totalDays);
        }

        public static decimal CalculateCredit(decimal invoiceFee, DateTime terminationDate, Model.ProductType productType)
        {
            var credit = 0m;

            if (productType.BillingLength == "Annual")
            {
                return invoiceFee;
            }

            // only beginning of day for accuracy
            terminationDate = terminationDate.Date;

            var beginningOfQuarter1 = DateTime.Parse("01/01/" + terminationDate.Year.ToString());
            var endingOfQuarter1 = DateTime.Parse("03/31/" + terminationDate.Year.ToString());
            var beginningOfQuarter2 = DateTime.Parse("04/01/" + terminationDate.Year.ToString());
            var endingOfQuarter2 = DateTime.Parse("06/30/" + terminationDate.Year.ToString());
            var beginningOfQuarter3 = DateTime.Parse("07/01/" + terminationDate.Year.ToString());
            var endingOfQuarter3 = DateTime.Parse("09/30/" + terminationDate.Year.ToString());
            var beginningOfQuarter4 = DateTime.Parse("10/01/" + terminationDate.Year.ToString());
            var endingOfQuarter4 = DateTime.Parse("12/31/" + terminationDate.Year.ToString());
            var beginningOfQuarter5 = DateTime.Parse("01/01/" + (terminationDate.Year + 1).ToString());

            var totalDays = 0d;
            var remainingDays = 0d;

            if (terminationDate >= beginningOfQuarter1 && terminationDate <= endingOfQuarter1)
            {
                totalDays = (beginningOfQuarter2 - beginningOfQuarter1).TotalDays;
                remainingDays = (beginningOfQuarter2 - terminationDate).TotalDays;
            }
            else if (terminationDate >= beginningOfQuarter2 && terminationDate <= endingOfQuarter2)
            {
                totalDays = (beginningOfQuarter3 - beginningOfQuarter2).TotalDays;
                remainingDays = (beginningOfQuarter3 - terminationDate).TotalDays;
            }
            else if (terminationDate >= beginningOfQuarter3 && terminationDate <= endingOfQuarter3)
            {
                totalDays = (beginningOfQuarter4 - beginningOfQuarter3).TotalDays;
                remainingDays = (beginningOfQuarter4 - terminationDate).TotalDays;
            }
            else if (terminationDate >= beginningOfQuarter4 && terminationDate <= endingOfQuarter4)
            {
                totalDays = (beginningOfQuarter5 - beginningOfQuarter4).TotalDays;
                remainingDays = (beginningOfQuarter5 - terminationDate).TotalDays;
            }

            // 1 subtracted to numerator to pick up full day of billing on termination
            //credit = invoiceFee * (decimal)((remainingDays - 1d) / totalDays);
            credit = invoiceFee * (decimal)((remainingDays) / totalDays);

            return credit;
        }

        /// <summary>
        /// Returns the appropriate invoice fee for a given billing date based on a given product's annual fee.
        /// </summary>
        /// <param name="annualFee">The annual fee for the product type.</param>
        /// <param name="productType">The product type for the engagement.</param>
        /// <returns>Returns invoice fee.</returns>
        /// <remarks>I should clean up this code, and perhaps make the logic feed from a data source.</remarks>
        public static decimal CalculateInvoiceFee(decimal annualFee, Model.ProductType productType, DateTime invoiceDate, DateTime startDate)
        {
            var invoiceFee = 0m;

            if (productType.BillingFrequency == "Quarterly" && productType.BillingLength == "Ongoing")
            {
                // add logic here to identify engagements that have started in previous quarter
                // seperate invoice for beginning engagements that are in advance and started previous quarter
                // ex: start = 6/15, billing = 6/30, invoice = only 15 days of invoice fee
                invoiceFee = annualFee / 4;
            }
            else if (productType.BillingFrequency == "Quarterly" && productType.BillingLength == "Annual")
            {
                var beginningOfQuarter1 = DateTime.Parse("01/01/" + startDate.Year.ToString());
                var endingOfQuarter1 = DateTime.Parse("03/31/" + startDate.Year.ToString());
                var beginningOfQuarter2 = DateTime.Parse("04/01/" + startDate.Year.ToString());
                var endingOfQuarter2 = DateTime.Parse("06/30/" + startDate.Year.ToString());
                var beginningOfQuater3 = DateTime.Parse("07/01/" + startDate.Year.ToString());
                var endingOfQuarter3 = DateTime.Parse("09/30/" + startDate.Year.ToString());

                if (invoiceDate.Year > startDate.Year)
                {
                    invoiceFee = annualFee / 4;
                }
                else if (startDate >= beginningOfQuarter1 && startDate <= endingOfQuarter1)
                {
                    invoiceFee = annualFee / 3;
                }
                else if (startDate >= beginningOfQuarter2 && startDate <= endingOfQuarter2)
                {
                    invoiceFee = annualFee / 2;
                    if (invoiceDate > endingOfQuarter3)
                    {
                        invoiceFee = invoiceFee + 0.01m;
                    }
                }
                else if (startDate >= beginningOfQuater3 && startDate <= endingOfQuarter3)
                {
                    invoiceFee = annualFee;
                }
                else
                {
                    invoiceFee = annualFee;
                }
            }
            else if (productType.BillingFrequency == "Monthly" && productType.BillingLength == "Ongoing")
            {
                invoiceFee = annualFee / 12;
            }
            else
            {
                invoiceFee = annualFee;
            }

            return invoiceFee;
        }

        /// <summary>
        /// Returns the annual fee for a given product type.
        /// </summary>
        /// <param name="productType">The product type for the engagement.</param>
        /// <returns>Returns annual fee.</returns>
        public static decimal CalculateAnnualFee(Model.ProductType productType, DateTime billingDate, DateTime clientFeeScheduleDate, decimal assetValue, int tierLevel = 0)
        {
            var annualFee = 0m;

            var feeSchedules = Data.FeeSchedule.GetFeeSchedules(productType, clientFeeScheduleDate, billingDate);
            
            if (productType.IsTierOnly == 1)
            {
                foreach (var feeSchedule in feeSchedules)
                {
                    if (feeSchedule.TierLevel == tierLevel)
                    {
                        annualFee = feeSchedule.AnnualFeeFixed;
                        break;
                    }
                }
                return annualFee;
            }

            foreach (var feeSchedule in feeSchedules)
            {
                var greaterThan = assetValue >= feeSchedule.AssetSizeMinimum;
                var lessThan = assetValue <= feeSchedule.AssetSizeMaximum || feeSchedule.AssetSizeMaximum.Equals(0m);

                if (feeSchedule.EndDate < billingDate)
                {
                    continue;
                }

                if (greaterThan && lessThan)
                {
                    annualFee = annualFee + ((assetValue - feeSchedule.AssetSizeMinimum) * feeSchedule.AnnualFeePercentage) + feeSchedule.AnnualFeeFixed;
                }
                else if (greaterThan && !lessThan)
                {
                    annualFee = annualFee + ((feeSchedule.AssetSizeMaximum - feeSchedule.AssetSizeMinimum) * feeSchedule.AnnualFeePercentage) + feeSchedule.AnnualFeeFixed;
                }
            }

            return annualFee;
        }

        /// <summary>
        /// Requests a product type from the user via the Console object.
        /// </summary>
        /// <returns>Returns product type.</returns>
        static Model.ProductType RequestProductType()
        {
            Console.WriteLine("Please enter engagement product type");
            foreach (var _productType in Data.ProductType.GetProductTypes())
            {
                Console.WriteLine(_productType.ProductTypeId.ToString() + " - " + _productType.Name);
            
            }

            var productTypeId = int.Parse(Console.ReadLine());
            var productType = new Model.ProductType { ProductTypeId = -1 };
            foreach (var _productType in Data.ProductType.GetProductTypes())
            {
                if (_productType.ProductTypeId == productTypeId)
                {
                    productType = _productType;
                    break;
                }
            }

            if (productType.ProductTypeId != -1)
            {
                return productType;
            }
            else
            {
                Console.WriteLine("Invalid entry.");
                return RequestProductType();
            }
        }

        /// <summary>
        /// Requests a DateTime from the user via the Console object.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        static DateTime RequestDateTime(string line)
        {
            Console.WriteLine(line);
            try
            {
                return DateTime.Parse(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Invalid entry.");
                return RequestDateTime(line);
            }
        }

        /// <summary>
        /// Requests decimal from the user via the Console object.
        /// </summary>
        /// <returns>Returns tier level.</returns>
        static decimal RequestDecimal(string line)
        {
            Console.WriteLine(line);
            try
            {
                return decimal.Parse(Console.ReadLine().Replace("$", ""));
            }
            catch
            {
                Console.WriteLine("Invalid entry.");
                return RequestDecimal(line);
            }
        }

        /// <summary>
        /// Requests integer from the user via the Console object.
        /// </summary>
        /// <returns></returns>
        static int RequestInt(string line)
        {
            Console.WriteLine(line);
            try
            {
                return int.Parse(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Invalid entry.");
                return RequestInt(line);
            }
        }

        static bool RequestBool(string line)
        {
            Console.WriteLine(line);
            try
            {
                return bool.Parse(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Invalid entry.");
                return RequestBool(line);
            }
        }
    }
}
