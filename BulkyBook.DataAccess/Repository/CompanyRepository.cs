﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private ApplicationDbContext rdb;

        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            rdb = db;
        }

        public void Update(Company obj)
        {
            rdb.Companies.Update(obj);
        }

    }
}
