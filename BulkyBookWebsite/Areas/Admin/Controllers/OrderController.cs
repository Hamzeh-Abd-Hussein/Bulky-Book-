using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Drawing;
using System.Security.Claims;

namespace BulkyBookWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        [BindProperty]
        public OrderVM orderVM { get; set; }

        public OrderController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork; 

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            orderVM = new OrderVM()
            {
                orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(x => x.Id == orderId, pro: "ApplicationUser"),
                orderDetail = _unitofwork.OrderDetail.GetAll(x => x.OrderId == orderId, pro: "Product"),
            };


            return View(orderVM);
        }
        [HttpPost]
        [ActionName("Details")]
        [ValidateAntiForgeryToken]
        public IActionResult Details_PAY_NOW()
        {
            orderVM.orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(x => x.Id == orderVM.orderHeader.Id, pro: "ApplicationUser");
            orderVM.orderDetail = _unitofwork.OrderDetail.GetAll(x => x.OrderId == orderVM.orderHeader.Id, pro: "Product");

                //payment_method
            var domain = "https://localhost:44358/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={orderVM.orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderVM.orderHeader.Id}",
            };

            foreach (var item in orderVM.orderDetail)
            {

                var sessionline = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionline);

            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitofwork.OrderHeader.UpdateStripe(orderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
            _unitofwork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }

        public IActionResult PaymentConfirmation(int orderHeaderid)
        {
            OrderHeader orderHeader = _unitofwork.OrderHeader.GetFirstOrDefault(x => x.Id == orderHeaderid);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);


                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitofwork.OrderHeader.UpdateStripe(orderHeaderid, session.Id, session.PaymentIntentId);
                    _unitofwork.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitofwork.Save();
                }   


            }


            return View(orderHeaderid);
        }



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        
        public IActionResult UpdateDetails()
        {
            var orderUpdateFromDb = _unitofwork.OrderHeader.GetFirstOrDefault(x => x.Id == orderVM.orderHeader.Id ,tracked:false);
            orderUpdateFromDb.Name = orderVM.orderHeader.Name;
            orderUpdateFromDb.PhoneNumber = orderVM.orderHeader.PhoneNumber;
            orderUpdateFromDb.StreetAddress = orderVM.orderHeader.StreetAddress;
            orderUpdateFromDb.City = orderVM.orderHeader.City;
            orderUpdateFromDb.State = orderVM.orderHeader.State;
            orderUpdateFromDb.PostalCode = orderVM.orderHeader.PostalCode;
            if (orderUpdateFromDb.Carrier != null)
            {
                orderUpdateFromDb.Carrier = orderVM.orderHeader.Carrier;
            }
            if (orderUpdateFromDb.TrackingNumber != null)
            {
                orderUpdateFromDb.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            }
            _unitofwork.OrderHeader.Update(orderUpdateFromDb);
            _unitofwork.Save();
            TempData["Success"] = "Order Details Update Successfully ";

            return RedirectToAction("Details","Order",new {orderId = orderUpdateFromDb.Id});
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _unitofwork.OrderHeader.UpdateStatus(orderVM.orderHeader.Id , SD.StatusInProcess); 
            _unitofwork.Save();
            TempData["Success"] = "Order Status Updateed Successfully";

            return RedirectToAction("Details", "Order", new { orderId = orderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderShip = _unitofwork.OrderHeader.GetFirstOrDefault(x => x.Id == orderVM.orderHeader.Id, tracked: false);
            orderShip.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            orderShip.Carrier = orderVM.orderHeader.Carrier;
            orderShip.OrderStatus = SD.StatusShipped;
            orderShip.ShippingDate = DateTime.Now;
            if (orderShip.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {

                orderShip.PaymentDueDate = DateTime.Now.AddDays(30);


            }
            _unitofwork.OrderHeader.Update(orderShip);
            _unitofwork.Save();
            TempData["Success"] = "Order Shipped Updateed Successfully";

            return RedirectToAction("Details", "Order", new { orderId = orderVM.orderHeader.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderheader = _unitofwork.OrderHeader.GetFirstOrDefault(x => x.Id == orderVM.orderHeader.Id, tracked: false);
            if (orderheader.OrderStatus == SD.PaymentStatusApproved)
            {
                var option = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderheader.PaymentIntentId,

                };
                var service = new RefundService();
                Refund refund = service.Create(option);

                _unitofwork.OrderHeader.UpdateStatus(orderheader.Id , SD.StatusCancelled , SD.StatusRefunded);
            }
            else
            {

                _unitofwork.OrderHeader.UpdateStatus(orderheader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitofwork.Save();
            TempData["Success"] = "Order Cancelled Successfully";

            return RedirectToAction("Details", "Order", new { orderId = orderVM.orderHeader.Id });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAllList(string status)
            {
            IEnumerable<OrderHeader> orderheaders;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            { 
            orderheaders = _unitofwork.OrderHeader.GetAll(pro: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var Claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderheaders = _unitofwork.OrderHeader.GetAll(x=>x.ApplicationUserId==Claim.Value,pro: "ApplicationUser");

            }
            

            switch (status)
            {
                case "pending": orderheaders = orderheaders.Where(x=>x.PaymentStatus ==SD.PaymentStatusDelayedPayment); break;


                case "inprocess":
                    orderheaders = orderheaders.Where(x => x.OrderStatus == SD.StatusInProcess); break;

                case "completed":
                    orderheaders = orderheaders.Where(x => x.OrderStatus == SD.StatusShipped); break;

                case "approved":
                    orderheaders = orderheaders.Where(x => x.OrderStatus == SD.StatusApproved); break;
                   
                default: break;
                   
                   
            }
                return Json(new { data = orderheaders });

        }
        #endregion

    }
}
