using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {
        private IEmployeeRepository _employeeRepository;
        private readonly IHostingEnvironment hostingEnvironment;

        public HomeController(IEmployeeRepository employeeRepository, IHostingEnvironment hostingEnvironment)
        {
            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees();
            return View(model);
        }

        public ViewResult Details(int? id)
        {
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = _employeeRepository.GetEmployee(id ?? 1),
                PageTitle = "Employee Details"
            };

            // Pass the ViewModel object to the View() helper method
            return View(homeDetailsViewModel);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        //[HttpPost]
        //public IActionResult Create(EmployeeCreateViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string uniqueFileName = null;

        //        if (model.Photo != null)
        //        {
        //            string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
        //            uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
        //            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
        //            model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
        //        }

        //        Employee newEmployee = new Employee
        //        {
        //            Name = model.Name,
        //            Email = model.Email,
        //            Department = model.Department,
        //            PhotoPath = uniqueFileName
        //        };

        //        _employeeRepository.Add(newEmployee);
        //        return RedirectToAction("details", new { id = newEmployee.Id });
        //    }

        //    return View();
        //}

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;

                if (model.Photos != null && model.Photos.Count > 0)
                {
                    foreach (IFormFile photo in model.Photos)
                    {
                        string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        photo.CopyTo(new FileStream(filePath, FileMode.Create));
                    }
                }

                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName
                };

                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }

            return View();
        }

    }
}
