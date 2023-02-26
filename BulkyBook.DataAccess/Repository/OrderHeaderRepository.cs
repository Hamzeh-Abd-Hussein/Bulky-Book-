using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext rdb;

        public OrderHeaderRepository(ApplicationDbContext db) : base(db)  
        {
            rdb = db; 
        }


        

        public void Update(OrderHeader obj)
        {
            rdb.OrderHeaders.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromdb = rdb.OrderHeaders.FirstOrDefault(x => x.Id == id);

            if (orderFromdb != null)
            { 
                orderFromdb.OrderStatus = orderStatus;

                if (paymentStatus != null)
                {
                    orderFromdb.PaymentStatus = paymentStatus;


                }
            }

        }

        public void UpdateStripe(int id, string sessionId, string paymentIntentId )
        {
            var orderFromdb = rdb.OrderHeaders.FirstOrDefault(x => x.Id == id);

            orderFromdb.PaymentDate = DateTime.Now;
            orderFromdb.SessionId = sessionId;
            orderFromdb.PaymentIntentId = paymentIntentId; 

        }









    }
}
