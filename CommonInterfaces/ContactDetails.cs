namespace CommonInterfaces
{
    public class ContactDetails : IContactDetails
    {
        public string PhoneNumber { get; set; }
        public string SourcePhoneNumber { get; set; }
        public string Name { get; set; }
        public string RowKey { get; set; }
        public bool Disabled { get; set; }
        public bool ForbidenWord { get; set; }
        public bool Reported { get; set; }
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