using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BulkyBookWebsite.Areas.Admin.Controllers;

    [Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class CompanyController : Controller
{
        private readonly IUnitOfWork _unitofwork;
       

        public CompanyController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
            
        }

        public IActionResult Index()
        {
           

            return View();
        }


        //GET
        public IActionResult Upsert(int? id)
        {

        Company company = new();
           

            if (id == null || id == 0)
            {
                //Create product

               
                return View(company);

            }
            else
            {
            //update product
             company = _unitofwork.Company.GetFirstOrDefault(x=> x.Id==id);
                
             return View(company);
             }
        
        }

    //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj )
        {
           
            if (ModelState.IsValid)
            {
                
                if (obj.Id == 0)
                {
                    _unitofwork.Company.Add(obj);
                   

                    TempData["success"] = "Company Created Successfully";
                   }
                else
                { 
            
                    _unitofwork.Company.Update(obj);
                    

                    TempData["success"] = "Company Edit Successfully";

                 }
                _unitofwork.Save();
                

             return RedirectToAction("Index");
            }
            return View(obj);
        }

        

    #region API CALLS
    [HttpGet]
    public IActionResult GetAllList()
    {
        var companylist = _unitofwork.Company.GetAll();
        return Json(new {data = companylist });


    }

    //POST
    [HttpDelete]
    
    public IActionResult Delete(int? id)
    {
        var obj = _unitofwork.Company.GetFirstOrDefault(u => u.Id == id);

        if (obj == null)
        {

            return Json(new { success = false , message = "Error while deleting" });

        }
        
            _unitofwork.Company.Remove(obj);
            _unitofwork.Save();
           return Json(new { success = true, message = "successe deleting" });



    }
    #endregion
}
