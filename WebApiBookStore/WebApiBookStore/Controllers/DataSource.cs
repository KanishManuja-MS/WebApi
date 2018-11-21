using BookStore.Models;
using Microsoft.OData.Edm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
    public static class DataSource
    {
        private static IList<Book> _books { get; set; }

        public static IList<Book> GetBooks()
        {
            if (_books != null)
            {
                return _books;
            }

            _books = new List<Book>();

            // book #1
            Book book = new Book
            {
                Id = 1,
                ISBN = "978-0-321-87758-1",
                Token = Guid.NewGuid(),

                //{
                //    Guid.NewGuid(),
                //    Guid.NewGuid()
                //},
                Title = "Essential C#5.0",
                Author = "Mark Michaelis",
                Price = 59.99m,
                Location = new Address { City = "Redmond", Street = "156TH AVE NE" }, 
                Reviews = new List<Review>() {
                    new Review { Rating = 4, Comment = "A good book" },
                    new Review { Rating = 1, Comment = "I don't like C#" },
                    new Review { Rating = 5, Comment = "I liked how the author explained delegates." }
                },
                Others = new List<byte[]>
                {
                    new byte[] { 1, 2, 3}
                },
                Binary = new byte[] { 22, 4, 5 },
                Press = new Press()
                {
                    Id = 1,
                    Name = "Addison-Wesley",
                    Category = Category.Book,
                },
                Date = new Date(2018, 9,5)

            };
            _books.Add(book);

            // book #2
            book = new Book
            {
                Id = 2,
                ISBN = "063-6-920-02371-5",
                Title = "Enterprise Games",
                Token = Guid.NewGuid(),
                Author = "Michael Hugos",
                Date = new Date(2018, 10, 5),
                Price = 49.99m,
                Binary = new byte[] { 2, 4, 5 },
                Location = new Address { City = "Bellevue", Street = "Main ST" }, //new List<Address>
                Reviews = new List<Review>() {
                    new Review { Rating = 4, Comment = "good book" },
                    new Review { Rating = 1, Comment = "bad book" },
                    new Review { Rating = 1, Comment = "I don't like C#" }
                },
                Others = new List<byte[]>(),
                Press = new Press()
                {
                    Id = 2,
                    Name = "O'Reilly",
                    Category = Category.EBook,
                    StockOwner= new Owner()
                    {
                        Id=1,
                        Name="Kanish Manuja",
                        Child = new Beneficiary ()
                        {
                            Id=10,
                            Name="Anju Manuja"
                        }
                    },
                }

            };
            _books.Add(book);
            // book #3
            book = new Book
            {
                Id = 3,
                ISBN = "063-6-920-02371-5",
                Title = "Enterprise Games",
                Token = Guid.NewGuid(),
                Author = "Michael Hugos",
                Date = new Date(2018, 10, 5),
                Price = 49.99m,
                Binary = new byte[] { 2, 4, 5 },
                Location = new Address { City = "Bellevue", Street = "Main ST" }, //new List<Address>
                Reviews = new List<Review>() {
                    new Review { Rating = 4, Comment = "good book" },
                    new Review { Rating = 1, Comment = "bad book" },
                    new Review { Rating = 1, Comment = "I don't like C#" }
                },
                Others = new List<byte[]>(),
                Press = new Press()
                {
                    Id = 2,
                    Name = "O'Reilly",
                    Category = Category.EBook,
                    StockOwner = new Owner()
                    {
                        Id = 1,
                        Name = "Kanish Manuja",
                        Child = new Beneficiary()
                        {
                            Id = 10,
                            Name = "Anju Manuja"
                        }
                    },
                }

            };
            _books.Add(book);
            return _books;
        }
    }
}
