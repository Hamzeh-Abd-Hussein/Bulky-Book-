using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext rdb;

        public ProductRepository(ApplicationDbContext db) : base(db)  
        {
            rdb = db;
            
        }


        public void Update(Product obj)
        {
            var objfromdb = rdb.Products.FirstOrDefault(x => x.Id==obj.Id);

            if (objfromdb != null)
            { 
            
                objfromdb.Title = obj.Title;
                objfromdb.ISBN = obj.ISBN; 
                objfromdb.Description = obj.Description;
                objfromdb.ListPrice = obj.ListPrice;
                objfromdb.Price = obj.Price; 
                objfromdb.Price50 = obj.Price50;
                objfromdb.Price100 = obj.Price100;
                objfromdb.CategoryId = obj.CategoryId;
                objfromdb.Author = obj.Author;
                objfromdb.CoverTypeId = obj.CoverTypeId;
                if (obj.ImageUrl != null)
                { 
                  objfromdb.ImageUrl = obj.ImageUrl;
               
                }
                
            
            
            
            }
        }
    }
}
