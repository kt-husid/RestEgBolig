using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace RestEgBolig.Models
{
    public class WaitListObject
    {
        [DataMember(Name = "companyNo")]
        public short CompanyNo { get; set; }

        [DataMember(Name = "departmentNo")]
        public short DepartmentNo { get; set; }

        [DataMember(Name = "type")]
        public short Type { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "address")]
        public string Address { get; set; }

        [DataMember(Name = "postalCodeCity")]
        public string PostalCodeCity { get; set; }

        [DataMember(Name = "rooms")]
        public short Rooms { get; set; }

        [DataMember(Name = "areal")]
        public decimal Areal { get; set; }

        [DataMember(Name = "price")]
        public decimal Price { get; set; }

        [DataMember(Name = "amount")]
        public int Amount { get; set; }

        [DataMember(Name = "numberOnList")]
        public short? NumberOnList { get; set; }
    }
}