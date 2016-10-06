using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using Data = InvoiceCalculation.Data;
using Model = InvoiceCalculation.Model;

namespace InvoiceCalculation.Test.Invoice
{
    public class TestMachine
    {
        public List<Generator> generators = new List<Generator>();

        public TestMachine()
        {

        }

        public bool Execute()
        {
            var result = true;

            Console.WriteLine("Unit test begin");

            var unitTestSuccesses = new List<UnitTest>();
            var unitTestFailures = new List<UnitTest>();

            foreach (var unitTest in GetAllUnitTests())
            {
                var linesToPrint = new List<String>();

                linesToPrint.Add("=========================");
                linesToPrint.Add("=========================");


                linesToPrint.Add("Engagement ID: " + unitTest.EngagementId);
                linesToPrint.Add("Billed On: " + unitTest.BilledOn);
                linesToPrint.Add("Earned On: " + unitTest.EarnedOn);
                linesToPrint.Add("Start Date: " + unitTest.StartDate);
                linesToPrint.Add("End Date: " + unitTest.EndDate);
                linesToPrint.Add("Days to pay: " + unitTest.DaysToPay);
                linesToPrint.Add("Billing type: " + unitTest.BillingType);
                linesToPrint.Add("Annual fee: " + unitTest.AnnualFee);
                linesToPrint.Add("Calculated Fee: " + unitTest.CalculatedFee);
                linesToPrint.Add("Total plan assets used: " + unitTest.TotalPlanAssetsUsed);

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
                Console.WriteLine("Unit test end: SUCCESS");
            }
            else
            {
                Console.WriteLine("Unit test end: FAILURE");
                Console.WriteLine("Successful unit tests: " + unitTestSuccesses.Count().ToString());
                Console.WriteLine("Failed unit tests: " + unitTestFailures.Count().ToString());
            }

            return result;
        }

        public void SpitBarz(int i)
        {
            for (var j = 0; j < i; j++)
            {
                Console.WriteLine("=========================");
            }
        }

        public bool UnitTestPasses(UnitTest unitTest)
        {
            var result = true;

            var generator = this.generators.Find(x => x.BillingDate == unitTest.BilledOn);

            if (generator == null)
            {
                generator = new Generator(unitTest.BilledOn);
                generators.Add(generator);
            }

            var invoices = generator.CalculateInvoice(unitTest.EngagementId);
            var invoice = invoices.Find(x => (int)x.BillingType == unitTest.BillingType);

            if (invoice == null)
            {
                Console.WriteLine("Billing type is incorrect.");
                return false;
            }

            if (decimal.Round(invoice.AnnualFee, 2) != unitTest.AnnualFee)
            {
                result = false;
                Console.WriteLine("Generated annual fee: " + decimal.Round(invoice.AnnualFee, 2));
                Console.WriteLine("Unit Test annual fee: " + unitTest.AnnualFee);
            }

            if (decimal.Round(invoice.InvoiceFee, 2) != unitTest.CalculatedFee)
            {
                result = false;
                Console.WriteLine("Generated invoice fee: " + decimal.Round(invoice.InvoiceFee, 2));
                Console.WriteLine("Unit Test invoice fee: " + unitTest.CalculatedFee);
            }

            if (invoice.TotalPlanAssetsUsed != unitTest.TotalPlanAssetsUsed)
            {
                result = false;
                Console.WriteLine("Generated TotalPlanAssetsUsed: " + invoice.TotalPlanAssetsUsed);
                Console.WriteLine("Unit Test TotalPlanAssetsUsed: " + unitTest.TotalPlanAssetsUsed);
            }

            if (invoice.BilledOn.Date != unitTest.BilledOn.Date)
            {
                result = false;
                Console.WriteLine("Generated BilledOn: " + invoice.BilledOn);
                Console.WriteLine("Unit Test BilledOn: " + unitTest.BilledOn);
            }

            if (invoice.EarnedOn.Date != unitTest.EarnedOn.Date)
            {
                result = false;
                Console.WriteLine("Generated EarnedOn: " + invoice.EarnedOn);
                Console.WriteLine("Unit Test EarnedOn: " + unitTest.EarnedOn);
            }

            if (invoice.StartDate.Date != unitTest.StartDate.Date)
            {
                result = false;
                Console.WriteLine("Generated StartDate: " + invoice.StartDate);
                Console.WriteLine("Unit Test StartDate: " + unitTest.StartDate);
            }

            if (invoice.EndDate.Date != unitTest.EndDate.Date)
            {
                result = false;
                Console.WriteLine("Generated EndDate: " + invoice.EndDate);
                Console.WriteLine("Unit Test EndDate: " + unitTest.EndDate);
            }

            if (invoice.DaysToPay != unitTest.DaysToPay)
            {
                result = false;
                Console.WriteLine("Generated DaysToPay: " + invoice.DaysToPay);
                Console.WriteLine("Unit Test DaysToPay: " + unitTest.DaysToPay);
            }

            return result;
        }

        public static List<UnitTest> GetAllUnitTests()
        {
            var result = new List<UnitTest>();

            using (var parser = new TextFieldParser(@"C:\Users\zallen\Desktop\InvoiceUnitTests.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadFields(); //skip first row of column headers
                while (!parser.EndOfData)
                {
                    //Processing row
                    var fields = parser.ReadFields();
                    var unitTest = new UnitTest();
                    
	                unitTest.EngagementId = Guid.Parse(fields[0]);
	                unitTest.BilledOn = DateTime.SpecifyKind(DateTime.Parse(fields[1]), DateTimeKind.Utc);
                    unitTest.EarnedOn = DateTime.SpecifyKind(DateTime.Parse(fields[2]), DateTimeKind.Utc);
	                unitTest.StartDate = DateTime.SpecifyKind(DateTime.Parse(fields[3]), DateTimeKind.Utc);
	                unitTest.EndDate = DateTime.SpecifyKind(DateTime.Parse(fields[4]), DateTimeKind.Utc);
	                unitTest.DaysToPay = int.Parse(fields[5]);
	                unitTest.BillingType = int.Parse(fields[6]);
                    unitTest.AnnualFee = decimal.Parse(fields[7].Replace("$", "").Replace("-", "0"));
                    unitTest.CalculatedFee = decimal.Parse(fields[8].Replace("$", "").Replace("-", "0"));
                    unitTest.TotalPlanAssetsUsed = decimal.Parse(fields[9].Replace("$", "").Replace("-", "0"));

                    result.Add(unitTest);
                }
            }

            return result;
        }
    }
}
