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
    
    public partial class Log
    {
        public int Id { get; set; }
        public Nullable<System.DateTime> ClientTime { get; set; }
        public System.DateTime DBTime { get; set; }
        public string PhoneNumber { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public Nullable<int> Severity { get; set; }
        public string CallStack { get; set; }
        public string Extra { get; set; }
    }
}
