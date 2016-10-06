using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PensionConsultants.Data.Access;

namespace InvoiceCalculation.Data
{
    public class InvoiceMaster
    {
        public DataAccessComponent Database = new DataAccessComponent(DataAccessComponent.Connections.PCIDB_Pension_Consultants_MSCRM);

        public DataTable RunMain(DateTime year)
        {
            Hashtable parameterList = new Hashtable();
            parameterList.Add("@Year", year);
            return Database.ExecuteStoredProcedureQuery("[dbo].[usp_CRM_InvoiceReport_v2]", parameterList);
        }

        public DataTable RunSingle(string clientName, string taskName, DateTime year)
        {
            Hashtable parameterList = new Hashtable();
            parameterList.Add("@ClientName", clientName);
            parameterList.Add("@TaskName", taskName);
            parameterList.Add("@Year", year);
            return Database.ExecuteStoredProcedureQuery("[dbo].[usp_CRM_InvoiceReport_Single_v2]", parameterList);
        }
    }
}
