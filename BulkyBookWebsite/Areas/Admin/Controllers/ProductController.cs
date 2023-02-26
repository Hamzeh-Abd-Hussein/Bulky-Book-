using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;

namespace BulkyBookWebsite.Areas.Admin.Controllers;

    [Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class ProductController : Controller
{
        private readonly IUnitOfWork _unitofwork;
        private readonly IWebHostEnvironment _hostEnv; 

        public ProductController(IUnitOfWork unitofwork, IWebHostEnvironment hostEnv)
        {
            _unitofwork = unitofwork;
            _hostEnv = hostEnv;
        }

        public IActionResult Index()
        {
           

            return View();
        }


        //GET
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {

                Product = new(),
                CategoryList = _unitofwork.Category.GetAll().Select( i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()

                }),
                CoverTypeList = _unitofwork.CoverType.GetAll().Select( i=> new SelectListItem 
                { 
                
                    Text = i.Name,
                    Value = i.Id.ToString()
                
                }),

            };

            if (id == null || id == 0)
            {
                //Create product

               
                return View(productVM);

            }
            else
            {
            //update product
            productVM.Product = _unitofwork.Product.GetFirstOrDefault(x=> x.Id==id);
                
             return View(productVM);
             }
        
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj , IFormFile? file)
        {
           
            if (ModelState.IsValid)
            {
                string RootPath = _hostEnv.WebRootPath;
                if (file!=null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(RootPath, @"images\products");
                    var ext = Path.GetExtension(file.FileName);

                    if (obj.Product.ImageUrl != null)
                    {
                    var oldImg = Path.Combine(RootPath,obj.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImg))
                        {
                            System.IO.File.Delete(oldImg);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(upload,fileName+ext),FileMode.Create) )
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName+ ext;

                }
                if (obj.Product.Id == 0)
                {
                    _unitofwork.Product.Add(obj.Product);
                    _unitofwork.Save();

                    TempData["success"] = "Product Created Successfully";
            }
                else
                { 
            
                _unitofwork.Product.Update(obj.Product);
                _unitofwork.Save();

                TempData["success"] = "Product Edit Successfully";

                 }
                
                

             return RedirectToAction("Index");
            }
            return View(obj);
        }

        

    #region API CALLS
    [HttpGet]
    public IActionResult GetAllList()
    {
        var productlist = _unitofwork.Product.GetAll(pro: "Category,CoverType");
        return Json(new {data = productlist });


    }

    //POST
    [HttpDelete]
    
    public IActionResult Delete(int? id)
    {
        var obj = _unitofwork.Product.GetFirstOrDefault(u => u.Id == id);

        if (obj == null)
        {

            return Json(new { success = false , message = "Error while deleting" });

        }
        var oldImg = Path.Combine(_hostEnv.WebRootPath, obj.ImageUrl.TrimStart('\\'));

        if (System.IO.File.Exists(oldImg))
        {
            System.IO.File.Delete(oldImg);
        }
            _unitofwork.Product.Remove(obj);
            _unitofwork.Save();
           return Json(new { success = true, message = "successe deleting" });



    }
    #endregion
}
