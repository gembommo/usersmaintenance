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
    
    public partial class Phone
    {
        public string Phone1 { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public int BusinessID { get; set; }
        public System.DateTime CreatedDate { get; set; }
    
        public virtual Business Business { get; set; }
    }
}
