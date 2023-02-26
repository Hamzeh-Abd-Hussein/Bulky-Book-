using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {

        private ApplicationDbContext rdb;

        public UnitOfWork(ApplicationDbContext db) 
        {
            rdb = db;
            Category = new CategoryRepository(rdb);
            CoverType = new CoverTypeRepository(rdb);
            Product = new ProductRepository(rdb);
            Company = new CompanyRepository(rdb);
            ShoppingCart = new ShoppingCartRepository(rdb);
            ApplicationUser = new ApplicationUserRepository(rdb);
            OrderHeader = new OrderHeaderRepository(rdb);
            OrderDetail = new OrderDetailRepository(rdb);


        }

        public ICategoryRepository Category { get; private set; }

        public ICoverTypeRepository CoverType { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }

        public void Save()
        {
            rdb.SaveChanges();
        }
    }
}
