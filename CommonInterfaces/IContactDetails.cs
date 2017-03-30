namespace CommonInterfaces
{
    public interface IContactDetails
    {
        string PhoneNumber { get; set; }
        string SourcePhoneNumber { get; set; }
        string Name { get; set; }
        string RowKey { get; set; }
        IContactDetails Clone();
    }
}
