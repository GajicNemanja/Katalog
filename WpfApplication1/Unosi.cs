//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Katalog
{
    using System;
    using System.Collections.Generic;
    
    public partial class Unosi
    {
        public long PrimKey { get; set; }
        public long ItemID { get; set; }
        public string Dobavljac { get; set; }
        public Nullable<decimal> Ulazna_cena { get; set; }
        public decimal Rabat { get; set; }
        public Nullable<decimal> Izlazna_cena { get; set; }
        public Nullable<decimal> Marza { get; set; }
        public string Komentari { get; set; }
        public System.DateTime Datum { get; set; }
    
        public virtual Stavke Stavke { get; set; }
    }
}