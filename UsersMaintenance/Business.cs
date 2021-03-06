//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UsersMaintenance
{
    using System;
    using System.Collections.Generic;
    
    public partial class Business
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Business()
        {
            this.Business_Activity = new HashSet<Business_Activity>();
            this.Business_Communication = new HashSet<Business_Communication>();
            this.Business_Property = new HashSet<Business_Property>();
            this.Phones = new HashSet<Phone>();
        }
    
        public int ID { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public string description { get; set; }
        public string street { get; set; }
        public Nullable<decimal> longitude { get; set; }
        public int catID { get; set; }
        public string homeN { get; set; }
        public string city { get; set; }
        public Nullable<decimal> latitude { get; set; }
        public string largeImage { get; set; }
        public int Status { get; set; }
        public string TimeZoneId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Business_Activity> Business_Activity { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Business_Communication> Business_Communication { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Business_Property> Business_Property { get; set; }
        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Phone> Phones { get; set; }
    }
}
