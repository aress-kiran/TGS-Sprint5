namespace swyfftAuto.Model.Enums
{
    public enum PolicyType
    {
        New,
        Endorsement,
        Renewal
    }

    public enum RecordType
    {
        Person,
        Insured,
        Policy
    }

    public enum Status
    {
        Issued,
        Cancelled
    }

    public enum TGSIStatus
    {
        Active,
        Expired,
        Cancel
    }

    public enum PolicyTerm
    {
        Policy_0,
        Policy_1,
        Policy_2
    }

    public enum Process
    {
        Insert,
        Update
    }
}
