using EmployeeManagement.Models;
using EmployeeManagement.Security;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
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
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDataProtector _protector;

        public HomeController(IEmployeeRepository employeeRepository,
                              IHostingEnvironment hostingEnvironment,
                              IDataProtectionProvider dataProtectionProvider,
                              DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            _employeeRepository = employeeRepository;
            _hostingEnvironment = hostingEnvironment;
            _protector = dataProtectionProvider.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }

        [AllowAnonymous]
        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees()
                               .Select(e =>
                               {
                                   // Encrypt the ID value and store in EncryptedId property
                                   e.EncryptedId = _protector.Protect(e.Id.ToString());
                                   return e;
                               });
            return View(model);
        }

        public ViewResult Details(string id)
        {
            //throw new Exception("Error in Details View");

            // Decrypt the employee id using Unprotect method
            string decryptedId = _protector.Unprotect(id);
            int decryptedIntId = Convert.ToInt32(decryptedId);

            Employee employee = _employeeRepository.GetEmployee(decryptedIntId);

            if (employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", decryptedIntId);
            }

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = employee,
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

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(model);

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

        //[HttpPost]
        //public IActionResult Create(EmployeeCreateViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string uniqueFileName = null;

        //        if (model.Photos != null && model.Photos.Count > 0)
        //        {
        //            foreach (IFormFile photo in model.Photos)
        //            {
        //                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
        //                uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
        //                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
        //                photo.CopyTo(new FileStream(filePath, FileMode.Create));
        //            }
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

        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(employeeEditViewModel);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;

                if (model.Photo != null)
                {
                    if (model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(_hostingEnvironment.WebRootPath,
                            "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    employee.PhotoPath = ProcessUploadedFile(model);
                }

                Employee updatedEmployee = _employeeRepository.Update(employee);

                return RedirectToAction("index");
            }

            return View(model);
        }

        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;

            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }
    }
}
