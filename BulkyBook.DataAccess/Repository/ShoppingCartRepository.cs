using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext rdb;

        public ShoppingCartRepository(ApplicationDbContext db) : base(db)  
        {
            rdb = db; 
        }

        public int DecrementCount(ShoppingCart shopping, int count)
        {
            shopping.Count -= count;
            return shopping.Count;
        }

        public int IncrementCount(ShoppingCart shopping, int count)
        {
            shopping.Count += count;
            return shopping.Count;
        }
    }
}
