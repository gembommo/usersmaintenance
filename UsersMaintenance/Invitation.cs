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
    
    public partial class Invitation
    {
        public int Id { get; set; }
        public string Inviter { get; set; }
        public string Invitee { get; set; }
        public bool LinkActivate { get; set; }
        public string UniqueKey { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
    }
}
