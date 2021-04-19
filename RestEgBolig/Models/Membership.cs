using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace RestEgBolig.Models
{
    public class Membership
    {
        [DataMember(Name = "companyNo")]
        public short CompanyNo { get; set; }

        [DataMember(Name = "memberCompanyNo")]
        public short MemberCompanyNo { get; set; }

        [DataMember(Name = "memberNo")]
        public decimal MemberNo { get; set; }

        [DataMember(Name = "tenancyType")]
        public short TenancyType { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "activationDate")]
        public DateTime? ActivationDate { get; set; }

        [DataMember(Name = "joinDate")]
        public DateTime? JoinDate { get; set; }

        [DataMember(Name = "quitDate")]
        public DateTime? QuitDate { get; set; }

        [DataMember(Name = "renewalDate")]
        public DateTime? RenewalDate { get; set; }

        [DataMember(Name = "suspendedToDate")]
        public DateTime? SuspendedToDate { get; set; }
    }
}