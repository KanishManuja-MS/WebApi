using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BookStore.Models
{
    // Book
    //[DataContract(Namespace ="BookStore.Models")]
    [Page(MaxTop=11,PageSize =2)]
    public class Book
    {
        public int Id { get; set; }
        public string ISBN { get; set; }
        public Guid Token { get; set; }
        public byte[] Binary { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public IList<Address> RandomCollection { get; set; }

        public IList<byte[]> Others { get; set; }
        public Date Date { get; set; }
        public Address Location { get; set; }
        //public IList<Press> Locations { get; set; }
        public Press Press { get; set; }
    }

    //public class GuidToken
    //{
    //    [Key]

    //}


    // Press
    [AutoExpand]
    [Page(PageSize =1)]
    public class Press
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Category Category { get; set; }
        public Owner StockOwner { get; set; }
    }

    // autoexpand
    [AutoExpand]
    public class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [AutoExpand]
        public Beneficiary Child { get; set; }
    }

    // autoexpand
    [AutoExpand]
    public class Beneficiary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
    }

    // Category
    public enum Category
    {
        Book,
        Magazine,
        EBook
    }

    // Address
    [Page(PageSize = 1)]
    public class Address
    {
        public string City { get; set; }
        public string Street { get; set; }
    }

    public class SubAddress:Address
    {
        public string HouseNumber { get; set; }
    }
}
