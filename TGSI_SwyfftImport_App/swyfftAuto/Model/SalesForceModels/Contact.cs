namespace swyfftAuto.Model.SalesForceModels
{
    public class Contact
    {
        public string Id { get; set; }
        public string FirstName { get; internal set; }
        public string LastName { get; internal set; }
        public string MailingCity { get; internal set; }
        public string MailingState { get; internal set; }
        public string MailingCountry { get; internal set; }
        public string MailingPostalCode { get; internal set; }
        //public string Name { get; set; }
        //public string MailingAddress { get; set; }
        //public string MailingAddressWithCountry { get; set; }
        //public string RecordType { get; set; }
    }
}
