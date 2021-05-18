using RestEgBolig.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace RestEgBolig.Controllers
{
    public class EgBoligController : ApiController
    {
        // GET: api/EgBolig/5
        [HttpGet]
        [Route("getMemberByCPR")]
        public Member GetMember(string cprNo)
        {
            /**************** FROM WEB SERVICE ****************/
            string cprNoFormatted = cprNo.Insert(6, "0");

            EgBoligService.Service10540Client svc = new EgBoligService.Service10540Client();
            EgBoligService.Member[] memberFromService = svc.MemberGetListByCprNo(cprNoFormatted, false);

            /**************** FROM DATABASE ****************/

            // connectionstring
            SqlConnection connection = new SqlConnection(@"Data Source=HAXDMA49; Initial Catalog=Bolig2; Integrated Security=False; User ID=EGBoligWS; Password=zYnc6hvWeytL9AVe; Multipleactiveresultsets=True; App=EntityFramework");
            connection.Open();

            // SQL to get status for LMtypes
            string sqlGetStatus = "select Lmtype, status from [Bolig2].[dbo].[MedlemAfSelskab] where sel = " + memberFromService[0].MemberCompanyNo + " and medlem = " + memberFromService[0].MemberNo;
            SqlCommand cmdGetStatus = new SqlCommand(sqlGetStatus, connection);

            string statusForType1 = "";
            string statusForType4 = "";
            string statusForType7 = "";

            using (SqlDataReader dr = cmdGetStatus.ExecuteReader())
            {
                // Loop to find all LMtypes and store status
                while (dr.Read())
                {
                    if (dr["Lmtype"].ToString() == "1")
                    {
                        statusForType1 = dr["status"].ToString();
                    }
                    if (dr["Lmtype"].ToString() == "4")
                    {
                        statusForType4 = dr["status"].ToString();
                    }
                    if (dr["Lmtype"].ToString() == "7")
                    {
                        statusForType7 = dr["status"].ToString();
                    }
                }
            }

            // SQL for comment in Medlem table in Bolig2 database
            string sqlGetComment = "select Top(1) krittext from [Bolig2].[dbo].[Medlem] where sel = " + memberFromService[0].MemberCompanyNo + " and medlem = " + memberFromService[0].MemberNo;
            SqlCommand cmdGetComment = new SqlCommand(sqlGetComment, connection);

            string comment = "";

            // get dat from MedlemAfSelskab table
            using (SqlDataReader dr = cmdGetComment.ExecuteReader())
            {
                if (dr.Read())
                {
                    comment = dr["krittext"].ToString();
                }
            }

            connection.Close();

            /**************** DEFINE MEMBER TO RETURN ****************/

            Member member = new Member();

            member.MemberCompanyNo = memberFromService[0].MemberCompanyNo;
            member.MemberNo = memberFromService[0].MemberNo;
            member.Name = memberFromService[0].Name;
            member.Address = memberFromService[0].Address;
            member.PostalCodeCity = memberFromService[0].PostalCodeCity;
            member.Country = memberFromService[0].Country;
            member.CprNo = (memberFromService[0].CprNo).Remove(6, 1);
            member.Email = memberFromService[0].Email;
            member.HomePhone = memberFromService[0].HomePhone;
            member.MobilePhone = memberFromService[0].MobilePhone;
            member.Children = memberFromService[0].Children;
            member.StatusForType1 = statusForType1;
            member.StatusForType4 = statusForType4;
            member.StatusForType7 = statusForType7;
            member.Comment = comment;

            /**************** RETURN MEMBER ****************/

            return member;
        }

        [HttpPut]
        [Route("updateMember")]
        public string UpdateMember(string cprNo, string country, string postalCodeCity, string address, string email, string homePhone, string mobilePhone, short children, string comment)
        {

            /**************** FROM WEB SERVICE ****************/
            string cprNoFormatted = cprNo.Insert(6, "0");

            RestEgBolig.EgBoligService.Service10540Client svc = new RestEgBolig.EgBoligService.Service10540Client();
            RestEgBolig.EgBoligService.Member[] memberFromService = svc.MemberGetListByCprNo(cprNoFormatted, false);

            short memberCompanyNo = memberFromService[0].MemberCompanyNo;
            decimal memberNo = memberFromService[0].MemberNo;
            int interessentNo = memberFromService[0].InteressentNo;
            decimal autoNo = memberFromService[0].AutoNo;
            string type = memberFromService[0].Type;

            /**************** FROM DATABASE ****************/

            // connectionstring
            SqlConnection connection = new SqlConnection(@"Data Source=HAXDMA49; Initial Catalog=Bolig2; Integrated Security=False; User ID=EGBoligWS; Password=zYnc6hvWeytL9AVe; Multipleactiveresultsets=True; App=EntityFramework");
            connection.Open();

            // SQL for Kontrakt table in Bolig2 database - Update comment
            string sqlSetComment = "update Top(1) [Bolig2].[dbo].[Medlem] set krittext = '" + comment + "' where sel = " + memberCompanyNo + " and medlem = " + memberNo;
            SqlCommand cmd = new SqlCommand(sqlSetComment, connection);

            // execute command
            cmd.ExecuteReader();
            connection.Close();

            //define member to update
            RestEgBolig.EgBoligService.Member memberUpdate = new RestEgBolig.EgBoligService.Member();

            memberUpdate.MemberCompanyNo = memberCompanyNo;
            memberUpdate.MemberNo = memberNo;
            memberUpdate.InteressentNo = interessentNo;
            memberUpdate.AutoNo = autoNo;
            memberUpdate.Type = type;
            memberUpdate.Country = country;
            memberUpdate.PostalCodeCity = postalCodeCity;
            memberUpdate.Address = address;
            memberUpdate.Email = email;
            memberUpdate.HomePhone = homePhone;
            memberUpdate.MobilePhone = mobilePhone;
            memberUpdate.Children = children;
            // comment

            // Update member in service
            svc.MemberUpdate(memberUpdate);

            return "Member " + memberUpdate.Name + " updated!";

        }

        [HttpGet]
        [Route("getAllDepartments")]
        public List<WaitListObject> GetAllDepartments()
        {
            // connectionstring
            SqlConnection connection = new SqlConnection(@"Data Source=HAXDMA49; Initial Catalog=Bolig2; Integrated Security=False; User ID=EGBoligWS; Password=zYnc6hvWeytL9AVe; Multipleactiveresultsets=True; App=EntityFramework");
            connection.Open();

            // SQL for Kontrakt table in Bolig2 database - Update comment
            string sqlGetDepartment = "SELECT count(*) as amount, afd.[Sel] as company, afd.[Afd] as department, afd.navn as name, afd.Adresse as address, afd.postby as postalCodeCity, lm.Lmtype as type, Antrum FROM[Bolig2].[dbo].[Lejemaal] as lm inner join[Bolig2].[dbo].[Afdeling] as afd on lm.Afd = afd.afd and afd.sel = lm.sel where lmtype in (1, 4, 7) and afd.sel = 1 group by afd.[Sel], afd.[Afd], afd.navn, afd.Adresse, afd.postby, Antrum, lm.Lmtype order by afd.afd";
            SqlCommand cmdGetComment = new SqlCommand(sqlGetDepartment, connection);

            short company = 0;
            int amount = 0;
            short department = 0;
            string name = "";
            string address = "";
            string postalCodeCity = "";
            short type = 0;
            short rooms = 0;

            List<WaitListObject> listOfDepartments = new List<WaitListObject>();

            // get dat from MedlemAfSelskab table
            using (SqlDataReader dr = cmdGetComment.ExecuteReader())
            {
                while (dr.Read())
                {
                    company = Convert.ToInt16(dr["company"]);
                    amount = Convert.ToInt32(dr["amount"]);
                    department = Convert.ToInt16(dr["department"]);
                    name = dr["name"].ToString();
                    address = dr["address"].ToString();
                    postalCodeCity = dr["postalCodeCity"].ToString();
                    type = Convert.ToInt16(dr["type"]);
                    rooms = Convert.ToInt16(dr["Antrum"]);

                    WaitListObject wishListObject = new WaitListObject();

                    // Areal og prísur verður definerað seinni, tí tað samsvarar ikki við tað sum er í GetWishList (øll feltini eru ikki definerað)
                    wishListObject.CompanyNo = company;
                    wishListObject.DepartmentNo = department;
                    wishListObject.Type = type;
                    wishListObject.Name = name.Split(new Char[] { ';' })[0]; // everything after ; is not being shown
                    wishListObject.Address = address.Split(new Char[] { ';' })[0];
                    wishListObject.PostalCodeCity = postalCodeCity;
                    wishListObject.Rooms = rooms;
                    wishListObject.Amount = amount;

                    listOfDepartments.Add(wishListObject);
                }
            }
            connection.Close();

            return listOfDepartments;
        }

        [HttpGet]
        [Route("getWaitList")]
        public List<WaitListObject> GetWaitList(string cprNo)
        {
            /**************** FROM WEB SERVICE ****************/
            string cprNoFormatted = cprNo.Insert(6, "0");

            RestEgBolig.EgBoligService.Service10540Client svc = new RestEgBolig.EgBoligService.Service10540Client();
            RestEgBolig.EgBoligService.Member[] memberFromService = svc.MemberGetListByCprNo(cprNoFormatted, false);

            short MemberCompanyNo = memberFromService[0].MemberCompanyNo;
            decimal MemberNo = memberFromService[0].MemberNo;

            List<WaitListObject> waitList = new List<WaitListObject>();
            RestEgBolig.EgBoligService.WaitList[] waitListObjectFromService = svc.WaitListGetList(MemberCompanyNo, MemberNo);

            foreach (RestEgBolig.EgBoligService.WaitList value in waitListObjectFromService)
            {
                /***************** FROM DATABASE *****************/

                //ToString get address and postalcodecity
                SqlConnection connection = new SqlConnection(@"Data Source=HAXDMA49; Initial Catalog=Bolig2; Integrated Security=False; User ID=EGBoligWS; Password=zYnc6hvWeytL9AVe; Multipleactiveresultsets=True; App=EntityFramework");
                connection.Open();

                string sqlGetFromAfdeling = "select Top(1) navn, adresse, postby from [Bolig2].[dbo].[Afdeling] where sel = " + value.CompanyNo + " and afd = " + value.DepartmentNo;
                SqlCommand cmd = new SqlCommand(sqlGetFromAfdeling, connection);

                string navn = "";
                string address = "";
                string postalCodeCity = "";
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        navn = dr["navn"].ToString();
                        address = dr["adresse"].ToString();
                        postalCodeCity = dr["postby"].ToString();
                    }
                }

                //to get areal average and price average
                string sqlGetFromLejemaal = "select bareal, vurd from [Bolig2].[dbo].[Lejemaal] where sel = " + value.CompanyNo + " and afd = " + value.DepartmentNo + " and antrum = " + value.Rooms + " and lmtype = " + value.TenancyType;
                SqlCommand cmd1 = new SqlCommand(sqlGetFromLejemaal, connection);

                List<decimal> arealList = new List<decimal>();
                decimal arealAverage = 0;

                List<decimal> priceList = new List<decimal>();
                decimal priceAverage = 0;
                using (SqlDataReader dr = cmd1.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string areal = "";
                        areal = dr["bareal"].ToString();
                        string price = "";

                        price = dr["vurd"].ToString();

                        if (areal != "")
                        {
                            arealList.Add(Convert.ToDecimal(areal));
                        }
                        if (price != "")
                        {
                            priceList.Add(Convert.ToDecimal(price));
                        }
                    }
                }
                // claculate areage of areal and price
                if (arealList.Count == 0)
                {
                    arealAverage = 0;
                }
                else
                {
                    arealAverage = arealList.Average();
                }

                if (priceList.Count == 0)
                {
                    priceAverage = 0;
                }
                else
                {
                    priceAverage = priceList.Average();
                }

                connection.Close();

                WaitListObject waitListObject = new WaitListObject();

                waitListObject.CompanyNo = value.CompanyNo;
                waitListObject.DepartmentNo = value.DepartmentNo;
                waitListObject.Type = value.TenancyType;
                waitListObject.Name = navn;
                waitListObject.Address = address;
                waitListObject.PostalCodeCity = postalCodeCity;
                waitListObject.Rooms = value.Rooms;
                waitListObject.Areal = decimal.Round(arealAverage);
                waitListObject.Price = decimal.Round(priceAverage);
                waitListObject.Amount = arealList.Count;
                waitListObject.NumberOnList = value.PriorityNoActive;
                waitList.Add(waitListObject);
            }

            return waitList;
        }

        [HttpPost]
        [Route("addWish")]
        public string AddWish(string cprNo, short companyNo, short departmentNo, byte rooms, short? type)
        {
            /**************** FROM WEB SERVICE ****************/
            string cprNoFormatted = cprNo.Insert(6, "0");

            RestEgBolig.EgBoligService.Service10540Client svc = new RestEgBolig.EgBoligService.Service10540Client();
            RestEgBolig.EgBoligService.Member[] memberFromService = svc.MemberGetListByCprNo(cprNoFormatted, false);

            short memberCompanyNo = memberFromService[0].MemberCompanyNo;
            decimal memberNo = memberFromService[0].MemberNo;

            // to get autoNo
            RestEgBolig.EgBoligService.Member member = svc.MemberGet(memberCompanyNo, memberNo);
            // define wish
            RestEgBolig.EgBoligService.Wish wish = new RestEgBolig.EgBoligService.Wish();

            wish.MemberCompanyNo = memberCompanyNo;
            wish.MemberNo = memberNo;
            wish.AutoNo = member.AutoNo;
            wish.CompanyNo = companyNo;
            wish.DepartmentNo = departmentNo;
            wish.FloorMax = 99;
            wish.FloorMin = -1;
            wish.Room = rooms;
            wish.SqmMax = 9999;
            wish.SqmMin = 0;
            wish.TenancyType = type;

            // add wish
            svc.WishAdd(wish, null);

            return "Wish added";
        }

        [HttpDelete]
        [Route("deleteWish")]
        public string DeleteWish(string cprNo, short companyNo, short departmentNo, byte rooms, short? type)
        {
            /**************** FROM WEB SERVICE ****************/
            string cprNoFormatted = cprNo.Insert(6, "0");

            RestEgBolig.EgBoligService.Service10540Client svc = new RestEgBolig.EgBoligService.Service10540Client();
            RestEgBolig.EgBoligService.Member[] memberFromService = svc.MemberGetListByCprNo(cprNoFormatted, false);

            short memberCompanyNo = memberFromService[0].MemberCompanyNo;
            decimal memberNo = memberFromService[0].MemberNo;

            // this method splits the wishes to seperate collums and makes it possible to delete a specific wish
            svc.WishGetList(memberCompanyNo, memberNo);

            //define wish to delete
            RestEgBolig.EgBoligService.Wish wishDelete = new RestEgBolig.EgBoligService.Wish();

            /**************** FROM DATABASE ****************/

            // connectionstring
            SqlConnection cn = new SqlConnection(@"Data Source=HAXDMA49; Initial Catalog=Bolig2; Integrated Security=False; User ID=EGBoligWS; Password=zYnc6hvWeytL9AVe; Multipleactiveresultsets=True; App=EntityFramework");
            cn.Open();

            // SQL for Kontrakt table in Bolig2 database
            string sql = "select autonum from [Bolig2].[dbo].[Medlafd] where sel = " + memberCompanyNo + " and medlem = " + memberNo + " and selmin = " + companyNo + " and selmax = " + companyNo + " and afdmin = " + departmentNo + " and afdmax = " + departmentNo + " and rummin = " + rooms + " and rummax = " + rooms + " and lmtype1 = " + type;
            SqlCommand cmd = new SqlCommand(sql, cn);

            short autoNo = 0;

            // get data from Kontrakt table
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    autoNo = Convert.ToInt16(dr["autonum"]);

                    wishDelete.MemberCompanyNo = memberCompanyNo;
                    wishDelete.MemberNo = memberNo;
                    wishDelete.AutoNo = autoNo;
                    wishDelete.CompanyNo = companyNo;
                    wishDelete.DepartmentNo = departmentNo;
                    wishDelete.Room = rooms;
                    wishDelete.TenancyType = type;

                    svc.WishDelete(wishDelete);
                }
            }
            cn.Close();

            return "Wish deleted!";
        }

        [HttpGet]
        [Route("getMembershipListByCPR")]
        public List<Membership> GetMembershipListByCPR(string cprNo)
        {
            /**************** FROM WEB SERVICE ****************/
            string cprNoFormatted = cprNo.Insert(6, "0");

            EgBoligService.Service10540Client svc = new EgBoligService.Service10540Client();
            EgBoligService.Member[] memberFromService = svc.MemberGetListByCprNo(cprNoFormatted, false);
            
            List<Membership> MembershipList = new List<Membership>();

            if(memberFromService.Length != 0)
            {
                short memberCompanyNo = memberFromService[0].MemberCompanyNo;
                decimal memberNo = memberFromService[0].MemberNo;

                EgBoligService.Membership[] membershipsFromService = svc.MembershipGetList(memberCompanyNo, memberNo, true);

                foreach (EgBoligService.Membership membershipFromService in membershipsFromService)
                {
                    Membership membership = new Membership();

                    membership.CompanyNo = membershipFromService.CompanyNo;
                    membership.MemberCompanyNo = membershipFromService.MemberCompanyNo;
                    membership.MemberNo = membershipFromService.MemberNo;
                    membership.TenancyType = membershipFromService.TenancyType;
                    membership.Status = membershipFromService.Status;
                    membership.ActivationDate = membershipFromService.ActivationDate;
                    membership.JoinDate = membershipFromService.JoinDate;
                    membership.QuitDate = membershipFromService.QuitDate;
                    membership.RenewalDate = membershipFromService.RenewalDate;
                    membership.SuspendedToDate = membershipFromService.SuspendedToDate;

                    MembershipList.Add(membership);
                }
            }
            return MembershipList;
        }

        [HttpPut]
        [Route("suspendMembership")]
        public string SuspendMemebership(string cprNo, short lmType)
        {
            /**************** FROM WEB SERVICE ****************/
            string cprNoFormatted = cprNo.Insert(6, "0");

            EgBoligService.Service10540Client svc = new EgBoligService.Service10540Client();
            EgBoligService.Member[] memberFromService = svc.MemberGetListByCprNo(cprNoFormatted, false);

            short memberCompanyNo = memberFromService[0].MemberCompanyNo;
            decimal memberNo = memberFromService[0].MemberNo;

            EgBoligService.Membership[] membershipsFromService = svc.MembershipGetList(memberCompanyNo, memberNo, true);

            foreach (EgBoligService.Membership membershipFromService in membershipsFromService)
            {
                if (membershipFromService.TenancyType == lmType & membershipFromService.Status == "0")
                {
                    EgBoligService.Membership membership = new EgBoligService.Membership();

                    membership.ActivationDate = membershipFromService.ActivationDate;
                    membership.CompanyNo = membershipFromService.CompanyNo;
                    membership.ExcemptFromFees = membershipFromService.ExcemptFromFees;
                    membership.JoinDate = membershipFromService.JoinDate;
                    membership.MemberCompanyNo = membershipFromService.MemberCompanyNo;
                    membership.MemberNo = membershipFromService.MemberNo;
                    membership.NewStatus = "1";
                    membership.QuitDate = membershipFromService.QuitDate;
                    membership.RenewalDate = membershipFromService.RenewalDate;
                    membership.Status = membershipFromService.Status;
                    membership.SuspendedToDate = membershipFromService.SuspendedToDate;
                    membership.TenancyType = membershipFromService.TenancyType;
                    membership.WaitListType = membershipFromService.WaitListType;
                    
                    svc.MembershipSuspendMembership(membership);

                    return "Membership with lmtype " + lmType + " is suspended.";
                }
            }
            return "Membership with lmtype " + lmType + " did not get changed!";
        }

        [HttpPut]
        [Route("activateMembership")]
        public string ActivateMembership(string cprNo, short lmType)
        {
            /**************** FROM WEB SERVICE ****************/
            string cprNoFormatted = cprNo.Insert(6, "0");

            EgBoligService.Service10540Client svc = new EgBoligService.Service10540Client();
            EgBoligService.Member[] memberFromService = svc.MemberGetListByCprNo(cprNoFormatted, false);

            short memberCompanyNo = memberFromService[0].MemberCompanyNo;
            decimal memberNo = memberFromService[0].MemberNo;

            EgBoligService.Membership[] membershipsFromService = svc.MembershipGetList(memberCompanyNo, memberNo, true);

            foreach (EgBoligService.Membership membershipFromService in membershipsFromService)
            {
                if (membershipFromService.TenancyType == lmType & membershipFromService.Status == "1")
                {
                    EgBoligService.Membership membership = new EgBoligService.Membership();

                    membership.ActivationDate = membershipFromService.ActivationDate;
                    membership.CompanyNo = membershipFromService.CompanyNo;
                    membership.ExcemptFromFees = membershipFromService.ExcemptFromFees;
                    membership.JoinDate = membershipFromService.JoinDate;
                    membership.MemberCompanyNo = membershipFromService.MemberCompanyNo;
                    membership.MemberNo = membershipFromService.MemberNo;
                    membership.NewStatus = "0";
                    membership.QuitDate = membershipFromService.QuitDate;
                    membership.RenewalDate = membershipFromService.RenewalDate;
                    membership.Status = membershipFromService.Status;
                    membership.SuspendedToDate = membershipFromService.SuspendedToDate;
                    membership.TenancyType = membershipFromService.TenancyType;
                    membership.WaitListType = membershipFromService.WaitListType;

                    svc.MembershipActivateMembership(membership);

                    return "Membership with lmtype " + lmType + " is activated.";
                }
            }
            return "Membership with lmtype " + lmType + " did not get changed!";
        }

        [HttpPut]
        [Route("deleteMembership")]
        public string DeleteMembership(string cprNo, short lmType)
        {
            /**************** FROM WEB SERVICE ****************/
            string cprNoFormatted = cprNo.Insert(6, "0");

            EgBoligService.Service10540Client svc = new EgBoligService.Service10540Client();
            EgBoligService.Member[] memberFromService = svc.MemberGetListByCprNo(cprNoFormatted, false);

            short memberCompanyNo = memberFromService[0].MemberCompanyNo;
            decimal memberNo = memberFromService[0].MemberNo;

            EgBoligService.Membership[] membershipsFromService = svc.MembershipGetList(memberCompanyNo, memberNo, true);

            foreach (EgBoligService.Membership membershipFromService in membershipsFromService)
            {
                // Not possible to delete lmtype 1, because it is the main lmtype that the member pays for
                if(lmType != 1)
                {
                    if (membershipFromService.TenancyType == lmType & (membershipFromService.Status == "1" || membershipFromService.Status == "0"))
                    {
                        EgBoligService.Membership membership = new EgBoligService.Membership();

                        membership.ActivationDate = membershipFromService.ActivationDate;
                        membership.CompanyNo = membershipFromService.CompanyNo;
                        membership.ExcemptFromFees = membershipFromService.ExcemptFromFees;
                        membership.JoinDate = membershipFromService.JoinDate;
                        membership.MemberCompanyNo = membershipFromService.MemberCompanyNo;
                        membership.MemberNo = membershipFromService.MemberNo;
                        membership.NewStatus = "3";
                        membership.QuitDate = membershipFromService.QuitDate;
                        membership.RenewalDate = membershipFromService.RenewalDate;
                        membership.Status = membershipFromService.Status;
                        membership.SuspendedToDate = membershipFromService.SuspendedToDate;
                        membership.TenancyType = membershipFromService.TenancyType;
                        membership.WaitListType = membershipFromService.WaitListType;

                        svc.MembershipDeleteMembership(membership);

                        return "Membership with lmtype " + lmType + " is deleted.";
                    }

                }
                
            }
            return "Membership with lmtype " + lmType + " did not get changed!";
        }
    }
}
