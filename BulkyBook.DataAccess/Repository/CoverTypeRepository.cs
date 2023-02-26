using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private ApplicationDbContext rdb;

        public CoverTypeRepository(ApplicationDbContext db) : base(db)  
        {
            rdb = db; 
        }


        public void Update(CoverType obj)
        {
            rdb.CoverTypes.Update(obj);
        }
    }
}
