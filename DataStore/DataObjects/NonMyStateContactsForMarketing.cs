namespace MyState.WebApplication.DataStore.DataObjects
{
    public class NonMyStateContactsForMarketing
    {
        public string PhoneNumber { get; set; }
        public string DisplayName { get; set; }
        public int TimesContacted { get; set; }
        public bool Starred { get; set; }
        public bool Favorite { get; set; }
        public bool Recents { get; set; }
    }
}
