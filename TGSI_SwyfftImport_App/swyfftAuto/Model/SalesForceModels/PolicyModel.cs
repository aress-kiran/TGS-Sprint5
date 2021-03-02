using System;

namespace swyfftAuto.Model.SalesForceModels
{
    public class PolicyModel
    {
        public int Id { get; set; }
        public Account Account { get; set; }
        public Contact Contact { get; set; }
        public Property Property { get; set; }
        public string Carrier { get; set; }
        public string CarrierProduct { get; set; }
        public string RecordType { get; set; }
        public string ProductType { get; set; }
        public string PolicyNumber { get; set; }
        public string BillingFrequency { get; set; }
        public string TypeOfBilling { get; set; }
        public string BillingMethod { get; set; }
        public decimal Premium { get; set; }
        public string PolicyType { get; set; }
        public string Status { get; set; }
        public string TGSIStatus { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime CancellationDate { get; set; }
        public string Term { get; set; }
        public string PriorTerm { get; set; }
        public string PriorPolicyNumber { get; set; }
        public string NextPolicyNumber { get; set; }
    }
}
