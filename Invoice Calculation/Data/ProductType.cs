using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using Models = InvoiceCalculation.Model;

namespace InvoiceCalculation.Data
{
    class ProductType
    {
        public static List<Model.ProductType> GetProductTypes()
        {
            var productTypes = new List<Model.ProductType>();

            using (var parser = new TextFieldParser(@"\\PC03\Operations\2.  operations team\IT\Invoice Solution\Required Files\ProductTypes.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadFields(); //skip first row of column headers
                while (!parser.EndOfData)
                {
                    //Processing row
                    var fields = parser.ReadFields();
                    var productType = new Model.ProductType();
                    productType.ProductTypeId = int.Parse(fields[0]);
                    productType.Name = fields[1];
                    productType.FeeScheduleId = int.Parse(fields[2]);
                    productType.IsTierOnly = int.Parse(fields[3]);
                    productType.BillingFrequency = fields[4];
                    productType.BillingSchedule = fields[5];
                    productType.BillingLength = fields[6];
                    productType.IsTieredRate = bool.Parse(fields[7]);
                    productType.IsServiceOngoing = bool.Parse(fields[8]);
                    productTypes.Add(productType);
                }
            }

            return productTypes;
        }

        public static Model.ProductType GetProductType(int productTypeId)
        {
            return GetProductTypes().Find(x => x.ProductTypeId == productTypeId);
        }
    }
}
