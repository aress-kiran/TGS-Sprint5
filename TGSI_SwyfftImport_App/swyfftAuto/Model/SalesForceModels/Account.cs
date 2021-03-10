namespace swyfftAuto.Model.SalesForceModels
{
    public class Account
    {
        //public int Id { get; set; }
        public string Name { get; set; }
        //public Address BillingAddress { get; set; }
        public string BillingCountry { get; set; }
        public string RecordTypeId { get; set; }
        public string BillingCity { get; internal set; }
        public string BillingState { get; internal set; }
        public object BillingPostalCode { get; internal set; }
        public string BillingStreet { get; internal set; }
    }
}
