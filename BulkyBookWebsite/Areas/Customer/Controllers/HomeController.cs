using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWebsite.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitofwork;
       

        public HomeController(ILogger<HomeController> logger , IUnitOfWork unitofwork)
        {
            _logger = logger;
            _unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> listProduct = _unitofwork.Product.GetAll(pro: "Category,CoverType");

            return View(listProduct);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart shopObj = new()
            {
                Count = 1,
                ProductId = productId,
                Product = _unitofwork.Product.GetFirstOrDefault(x => x.Id == productId, pro:"Category,CoverType")

            };


            return View(shopObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shopping)
        {

           
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var Claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shopping.ApplicationUserId = Claim.Value;

            ShoppingCart cartFromDb = _unitofwork.ShoppingCart.GetFirstOrDefault(
                x => x.ApplicationUserId == Claim.Value && x.ProductId ==shopping.ProductId );

            if (cartFromDb == null)
            {

                _unitofwork.ShoppingCart.Add(shopping);
            }
            else
            {

                _unitofwork.ShoppingCart.IncrementCount(cartFromDb , shopping.Count);
            
            }

            _unitofwork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}