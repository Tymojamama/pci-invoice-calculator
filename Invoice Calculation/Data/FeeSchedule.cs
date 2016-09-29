using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using Models = InvoiceCalculation.Model;

namespace InvoiceCalculation.Data
{
    public static class FeeSchedule
    {
        /// <summary>
        /// Obtains a list of all fee schedules.
        /// </summary>
        /// <returns></returns>
        public static List<Model.FeeSchedule> GetFeeSchedules()
        {
            var feeSchedules = new List<Model.FeeSchedule>();

            using (var parser = new TextFieldParser(@"C:\Users\zallen\Desktop\FeeSchedules.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadFields(); //skip first row of column headers
                while (!parser.EndOfData)
                {
                    //Processing row
                    var fields = parser.ReadFields();
                    var feeSchedule = new Model.FeeSchedule();
                    feeSchedule.FeeScheduleId = int.Parse(fields[0]);
                    feeSchedule.TierLevel = int.Parse(fields[1]);
                    feeSchedule.BillingType = (Model.BillingType)int.Parse(fields[2]);

                    feeSchedule.StartDate = DateTime.Parse(fields[3]);

                    if (fields[4] != "")
                    {
                        feeSchedule.EndDate = DateTime.Parse(fields[4]);
                    }
                    else
                    {
                        feeSchedule.EndDate = null;
                    }

                    feeSchedule.AssetSizeMinimum = decimal.Parse(fields[5]);
                    feeSchedule.AssetSizeMaximum = decimal.Parse(fields[6]);
                    feeSchedule.AnnualFeePercentage = decimal.Parse(fields[7]);
                    feeSchedule.AnnualFeeFixed = decimal.Parse(fields[8]);
                    feeSchedules.Add(feeSchedule);
                }
            }

            return feeSchedules;
        }

        /// <summary>
        /// Obtains a list of all fee schedules for a product type and contract date.
        /// </summary>
        /// <param name="productType"></param>
        /// <param name="clientFeeScheduleDate"></param>
        /// <returns></returns>
        public static List<Model.FeeSchedule> GetFeeSchedules(Model.ProductType productType, DateTime clientFeeScheduleDate, DateTime billingDate)
        {
            if (clientFeeScheduleDate.Date == new DateTime(2015, 05, 16).Date)
            {

            }

            var feeSchedules = Data.FeeSchedule.GetFeeSchedules()
                .FindAll(x => x.FeeScheduleId.Equals(productType.FeeScheduleId))
                .FindAll(x => x.IsWithinDateTime(clientFeeScheduleDate))
                .FindAll(x => x.IsBeforeDateTime(billingDate));

            if (feeSchedules.Count == 0)
            {
                feeSchedules = Data.FeeSchedule.GetFeeSchedules()
                    .FindAll(x => x.FeeScheduleId.Equals(productType.FeeScheduleId))
                    .FindAll(x => x.IsWithinDateTime(billingDate));
            }

            var startDateCount = feeSchedules.GroupBy(x => x.StartDate).Count();

            // only use most recent fee schedule if contract date is 
            // within a range of multiple fee schedules
            if (startDateCount > 1)
            {
                var greatestStartDate = DateTime.MinValue;
                foreach (var feeSchedule in feeSchedules)
                {
                    if (greatestStartDate <= feeSchedule.StartDate)
                    {
                        greatestStartDate = feeSchedule.StartDate;
                    }
                }

                feeSchedules = feeSchedules.FindAll(x => x.StartDate == greatestStartDate);
            }

            return feeSchedules;
        }
    }
}
