namespace swyfftAuto.Model.SalesForceModels
{

    public class Property
    {
        //public int Id { get; set; }
        //public string Address { get; set; }
        //public string City { get; set; }
        //public string State { get; set; }
        //public string Zipcode { get; set; }
        public string TGS_Account__c { get; internal set; }
        public string Name { get; internal set; }
        public string TGS_Zip_Code__c { get; internal set; }
        public string TGS_City__c { get; internal set; }
        public string TGS_State__c { get; internal set; }
    }
}
