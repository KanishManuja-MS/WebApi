using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    [Route("api/[controller]")]
    public class BooksController : ODataController
    {
        private BookStoreContext _db;

        public BooksController(BookStoreContext context)
        {
            _db = context;
            if (context.Books.Count() == 0)
            {
                foreach (var b in DataSource.GetBooks())
                {
                    context.Books.Add(b);
                    //context.Press.Add(b.Presses[0]);
                }
                context.SaveChanges();
            }
        }

        [EnableQuery(MaxExpansionDepth =5)]
        public IActionResult Get()
        {
            return Ok(_db.Books);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            return Ok(_db.Books.FirstOrDefault(c => c.Id == key));
        }

        [EnableQuery]
        public IActionResult Post([FromBody]Book book)
        {
            _db.Books.Add(book);
            _db.SaveChanges();
            return Created(book);
        }

        [EnableQuery]
        public IActionResult PostToRandomCollection(int key, [FromBody]SubAddress newInt)
        {
            Book ToBeEdited = _db.Books.FirstOrDefault(c => c.Id == key);
            ToBeEdited.RandomCollection.Add(newInt);
            _db.Books.Update(ToBeEdited);
            _db.SaveChanges();
            return Ok(newInt);
        }

        [EnableQuery]
        public IActionResult PostToOthers(int key, [FromBody]byte[] newInt)
        {
            Book ToBeEdited = _db.Books.FirstOrDefault(c => c.Id == key);
            ToBeEdited.Others.Add(newInt);
            _db.SaveChanges();
            return Ok(newInt);
        }
    }
}
