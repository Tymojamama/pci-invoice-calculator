using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using Data = InvoiceCalculation.Data;
using Model = InvoiceCalculation.Model;

namespace InvoiceCalculation.Test.FeeSchedule
{
    public static class TestMachine
    {
        public static bool Execute()
        {
            var result = true;

            Console.WriteLine("Fee schedule unit test begin");

            var unitTestSuccesses = new List<UnitTest>();
            var unitTestFailures = new List<UnitTest>();

            foreach(var unitTest in GetAllUnitTests())
            {
                var linesToPrint = new List<String>();

                linesToPrint.Add("=========================");
                linesToPrint.Add("=========================");

                linesToPrint.Add("Product type: " + unitTest.ProductType.Name);
                linesToPrint.Add("Plan asset value: " + unitTest.PlanAssetValue);
                linesToPrint.Add("Client fee schedule date: " + unitTest.ClientFeeScheduleDate);
                linesToPrint.Add("Engagement start date: " + unitTest.EngagementStartDate);
                linesToPrint.Add("Billing date: " + unitTest.BillingDate);
                linesToPrint.Add("Tier level: " + unitTest.TierLevel);
                linesToPrint.Add("Termination date: " + unitTest.TerminationDate);
                linesToPrint.Add("Expected annual fee: " + unitTest.ExpectedAnnualFee);
                linesToPrint.Add("Expected invoice fee: " + unitTest.ExpectedInvoiceFee);
                linesToPrint.Add("Expected credit: " + unitTest.ExpectedCredit);

                linesToPrint.Add("=========================");

                if (UnitTestPasses(unitTest))
                {
                    unitTestSuccesses.Add(unitTest);
                    linesToPrint.Add("=========================");
                    linesToPrint.Add("End unit: SUCCESS");
                    linesToPrint.Add("=========================");
                    linesToPrint.Add("=========================");
                }
                else
                {
                    unitTestFailures.Add(unitTest);
                    linesToPrint.Add("=========================");

                    foreach (var line in linesToPrint)
                    {
                        Console.WriteLine(line);
                    }

                    result = false;
                }
            }

            if (unitTestFailures.Count() == 0)
            {
                Console.WriteLine("Fee schedule unit test end: SUCCESS");
            }
            else
            {
                Console.WriteLine("Fee schedule unit test end: FAILURE");
                Console.WriteLine("Successful unit tests: " + unitTestSuccesses.Count().ToString());
                Console.WriteLine("Failed unit tests: " + unitTestFailures.Count().ToString());
            }

            return result;
        }

        public static void SpitBarz(int i)
        {
            for (var j = 0; j < i; j++)
            {
                Console.WriteLine("=========================");
            }
        }

        public static bool UnitTestPasses(UnitTest unitTest)
        {
            var result = true;
            if (!AnnualFeePasses(unitTest))
            {
                result = false;
            }

            if (!InvoiceFeePasses(unitTest))
            {
                result = false;
            }

            if (!TerminationCreditPasses(unitTest))
            {
                result = false;
            }
            return result;
        }

        public static bool AnnualFeePasses(UnitTest unitTest)
        {
            var result = true;
            var annualFee = Program.CalculateAnnualFee(unitTest.ProductType, unitTest.BillingDate, unitTest.ClientFeeScheduleDate, unitTest.PlanAssetValue, unitTest.TierLevel);
            annualFee = Math.Round(annualFee, 2);
            if (unitTest.ExpectedAnnualFee == annualFee)
            {
                //Console.WriteLine("Annual fee: SUCCESS");
                //Console.WriteLine("Annual fee calculated & expected: " + annualFee);
            }
            else
            {
                Console.WriteLine("Annual fee: FAILURE");
                Console.WriteLine("Expected annual fee: " + unitTest.ExpectedAnnualFee);
                Console.WriteLine("Calculated annual fee: " + annualFee);
                result = false;
            }
            return result;
        }

        public static bool InvoiceFeePasses(UnitTest unitTest)
        {
            var result = true;

            var isTerminatedEngagement = false;
            var isNewEngagement = false;
            var engagementStartDate = new DateTime();
            if (unitTest.UnitTestType == UnitTestType.TerminatedEngagement)
            {
                isTerminatedEngagement = true;
            }
            else if (unitTest.UnitTestType == UnitTestType.NewEngagement)
            {
                isNewEngagement = true;
                engagementStartDate = (DateTime)unitTest.EngagementStartDate;
            }

            var invoiceFee = Calculator.CalculateInvoiceFee(unitTest.ExpectedAnnualFee, unitTest.ProductType, unitTest.BillingDate, unitTest.ClientFeeScheduleDate, isTerminatedEngagement, isNewEngagement, engagementStartDate);
            invoiceFee = Math.Round(invoiceFee, 2);

            if (unitTest.ExpectedInvoiceFee == invoiceFee)
            {
                //Console.WriteLine("Invoice fee: SUCCESS");
                //Console.WriteLine("Invoice fee calculated & expected: " + invoiceFee);
            }
            // margin of error of 1 penny (in client's favor)
            else if (unitTest.ExpectedInvoiceFee == invoiceFee + 0.01m)
            {
                //Console.WriteLine("Invoice fee: SUCCESS");
                //Console.WriteLine("Invoice fee calculated & expected: " + invoiceFee);
            }
            else
            {
                Console.WriteLine("Invoice fee: FAILURE");
                Console.WriteLine("Expected invoice fee: " + unitTest.ExpectedInvoiceFee);
                Console.WriteLine("Calculated invoice fee: " + invoiceFee);
                result = false;
            }
            return result;
        }

        public static bool TerminationCreditPasses(UnitTest unitTest)
        {
            var result = true;
            
            var invoiceCredit = 0m;
            if (unitTest.UnitTestType == UnitTestType.TerminatedEngagement)
            {
                var originalInvoiceFee = Calculator.CalculateOriginalInvoiceFee(unitTest.ExpectedAnnualFee, unitTest.ProductType, unitTest.BillingDate, unitTest.ClientFeeScheduleDate);
                invoiceCredit = Calculator.CalculateInvoiceCredit(true, originalInvoiceFee, (DateTime)unitTest.TerminationDate, unitTest.ProductType);
                invoiceCredit = Math.Round(invoiceCredit, 2);
                if (unitTest.ExpectedCredit == invoiceCredit)
                {
                    //Console.WriteLine("Credit: SUCCESS");
                    //Console.WriteLine("Credit calculated & expected: " + credit);
                }
                else
                {
                    Console.WriteLine("Credit: FAILURE");
                    Console.WriteLine("Expected credit: " + unitTest.ExpectedCredit);
                    Console.WriteLine("Calculated credit: " + invoiceCredit);
                    result = false;
                }
            }

            return result;
        }

        public static List<UnitTest> GetAllUnitTests()
        {
            var result = new List<UnitTest>();

            using (var parser = new TextFieldParser(@"\\PC03\Operations\2.  operations team\IT\Invoice Solution\Required Files\UnitTests.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadFields(); //skip first row of column headers
                while (!parser.EndOfData)
                {
                    //Processing row
                    var fields = parser.ReadFields();
                    var unitTest = new UnitTest();

                    unitTest.UnitTestType = UnitTestType.ExistingEngagement;
                    unitTest.ProductType = Data.ProductType.GetProductType(int.Parse(fields[0]));
                    unitTest.BillingDate = DateTime.Parse(fields[1]);
                    unitTest.ClientFeeScheduleDate = DateTime.Parse(fields[2]);
                    unitTest.PlanAssetValue = decimal.Parse(fields[3].Replace("$",""));
                    unitTest.TierLevel = int.Parse(fields[4]);

                    if (!String.IsNullOrWhiteSpace(fields[5]))
                    {
                        unitTest.UnitTestType = UnitTestType.NewEngagement;
                        unitTest.EngagementStartDate = DateTime.Parse(fields[5]);
                    }

                    if (!String.IsNullOrWhiteSpace(fields[6]))
                    {
                        unitTest.UnitTestType = UnitTestType.TerminatedEngagement;
                        unitTest.TerminationDate = DateTime.Parse(fields[6]);
                    }

                    unitTest.ExpectedInvoiceFee = decimal.Parse(fields[7].Replace("$",""));
                    unitTest.ExpectedAnnualFee = decimal.Parse(fields[8].Replace("$", ""));

                    if (int.Parse(fields[10]) == 1)
                    {
                        unitTest.IsActive = true;
                    }
                    else
                    {
                        unitTest.IsActive = false;
                    }

                    if (!String.IsNullOrWhiteSpace(fields[9]))
                    {
                        unitTest.ExpectedCredit = decimal.Parse(fields[9].Replace("$", ""));
                    }

                    result.Add(unitTest);
                }
            }

            return result.FindAll(x => x.IsActive == true);
        }
    }
}
