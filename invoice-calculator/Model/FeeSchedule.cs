using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceCalculation.Model
{
    public class FeeSchedule
    {
        public int FeeScheduleId;
        public int TierLevel;
        public BillingType BillingType;
        public DateTime StartDate;
        public DateTime? EndDate;
        public decimal AssetSizeMinimum;
        public decimal AssetSizeMaximum;
        public decimal AnnualFeePercentage;
        public decimal AnnualFeeFixed;

        public FeeSchedule()
        {

        }

        public bool IsAfterDateTime(DateTime dateTime)
        {
            var after = false;

            if (this.StartDate.Date <= dateTime.Date)
            {
                after = true;
            }

            return after;
        }

        public bool IsBeforeDateTime(DateTime dateTime)
        {
            var before = false;

            if (this.EndDate != null && ((DateTime)this.EndDate).Date >= dateTime.Date)
            {
                before = true;
            }

            if (this.EndDate == null)
            {
                before = true;
            }

            return before;
        }

        public bool IsWithinDateTime(DateTime dateTime)
        {
            var after = false;
            var before = false;

            if (this.StartDate.Date <= dateTime.Date)
            {
                after = true;
            }

            if (this.EndDate != null && ((DateTime)this.EndDate).Date >= dateTime.Date)
            {
                before = true;
            }

            if (this.EndDate == null)
            {
                before = true;
            }

            return after && before;
        }
    }
}
