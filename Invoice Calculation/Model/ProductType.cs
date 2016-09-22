using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceCalculation.Model
{
    public class ProductType
    {
        public int ProductTypeId;
        public int FeeScheduleId;
        public int IsTierOnly;
        public string Name;
        public string BillingFrequency;
        public string BillingSchedule;
        public string BillingLength;

        public ProductType()
        {

        }
    }
}
