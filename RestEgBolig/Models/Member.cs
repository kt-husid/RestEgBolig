using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace RestEgBolig.Models
{
    [DataContract]
    public class Member
    {
        [DataMember(Name = "memberCompanyNo")]
        public short MemberCompanyNo { get; set; }

        [DataMember(Name = "memberNo")]
        public decimal MemberNo { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "address")]
        public string Address { get; set; }

        [DataMember(Name = "postalCodeCity")]
        public string PostalCodeCity { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "cprNo")]
        public string CprNo { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "homePhone")]
        public string HomePhone { get; set; }

        [DataMember(Name = "mobilePhone")]
        public string MobilePhone { get; set; }

        [DataMember(Name = "children")]
        public short? Children { get; set; }

        [DataMember(Name = "statusForType1")]
        public string StatusForType1 { get; set; } //(db.MedlemAfSelskab)

        [DataMember(Name = "statusForType4")]
        public string StatusForType4 { get; set; }

        [DataMember(Name = "statusForType7")]
        public string StatusForType7 { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; } //(db.Medlem)
    }
}