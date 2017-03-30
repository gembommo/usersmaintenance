namespace CommonInterfaces
{
    public class ContactDetails : IContactDetails
    {
        public string PhoneNumber { get; set; }
        public string SourcePhoneNumber { get; set; }
        public string Name { get; set; }
        public string RowKey { get; set; }
        public IContactDetails Clone()
        {
            return new ContactDetails()
            {
                PhoneNumber = PhoneNumber,
                SourcePhoneNumber = SourcePhoneNumber,
                Name = Name,
                RowKey = RowKey
            };
        }
    }
}