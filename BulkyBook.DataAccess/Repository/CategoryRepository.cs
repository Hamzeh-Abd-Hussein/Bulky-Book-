﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext rdb;

        public CategoryRepository(ApplicationDbContext db) : base(db)  
        {
            rdb = db; 
        }


        

        public void Update(Category obj)
        {
            rdb.Categories.Update(obj);
        }
    }
}
