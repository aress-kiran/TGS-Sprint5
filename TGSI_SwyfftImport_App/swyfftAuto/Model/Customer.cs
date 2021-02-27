using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace swyfftAuto.Model
{

    
    public class Customer
    {
        public int RecID { get; set; }
        public string Address { get; set; }
        public decimal Amount { get; set; }
        public string CustName { get; set; }
        public DateTime? Date { get; set; }
        public string Payment { get; set; }
        public string Policy { get; set; }
        public string Invoice { get; set; }
        public string StatusText { get; set; }        
        public string InvoiceText { get; set; }
        public string SalesAgent { get; set; }
        public string PolicyNumber { get; set; }
        public string billing_frequency { get; set; }
        public string TypeofBilling { get; set; }
        public string BillingMethod { get; set; }
        public string UpdatedStatusText { get; set; }
        public DateTime? StatusModifiedDate { get; set; }

    }
}
