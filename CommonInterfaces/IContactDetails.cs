namespace CommonInterfaces
{
    public interface IContactDetails
    {
        string PhoneNumber { get; set; }
        string SourcePhoneNumber { get; set; }
        string Name { get; set; }
        string RowKey { get; set; }
        bool Disabled { get; set; }
        bool ForbidenWord { get; set; }
        bool Reported { get; set; }
        IContactDetails Clone();
    }
}
