using BilgeShop.Business.Dtos;
using BilgeShop.Business.Services;
using BilgeShop.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BilgeShop.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _environment;
        public ProductController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment environment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _environment = environment;
        }

        public IActionResult List()
        {
            var productDtoList = _productService.GetProducts();
            var viewModel = productDtoList.Select(x => new ProductListViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                UnitPrice = x.UnitPrice,
                UnitsInStock = x.UnitsInStock,
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                ImagePath = x.ImagePath,
            }).ToList();

            return View(viewModel);
        }
        public IActionResult New()
        {
            ViewBag.Categories = _categoryService.GetCategories();
            return View("Form", new ProductFormViewModel());
        }

        public IActionResult Update(int id)
        {
            var updateProductDto = _productService.GetProductById(id);

            var viewModel = new ProductFormViewModel
            {
                Id = updateProductDto.Id,
                Name = updateProductDto.Name,
                Description = updateProductDto.Description,
                UnitPrice = updateProductDto.UnitPrice,
                UnitsInStock = updateProductDto.UnitsInStock,
                CategoryId = updateProductDto.CategoryId
            };

            ViewBag.ImagePath = updateProductDto.ImagePath;
            ViewBag.Categories = _categoryService.GetCategories();

            return View("Form", viewModel);
        }

        [HttpPost]
        public IActionResult Save(ProductFormViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _categoryService.GetCategories();
                return View("Form", formData);
            }

            var newFileName = "";

            if (formData.File is not null)
            {
                var allowedFileTypes = new string[] { "image/jpeg", "image/jpg", "image/png", "image/jfif"};

                var allowedFileExtensions = new string[] { ".jpg", ".jpeg", ".png", ".jfif" };

                var fileContentType = formData.File.ContentType;

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(formData.File.FileName);

                var fileExtension = Path.GetExtension(formData.File.FileName);

                if (!allowedFileTypes.Contains(fileContentType) || !allowedFileExtensions.Contains(fileExtension))
                {
                    // HATA MESAJI EKLE!!!
                    ViewBag.Categories = _categoryService.GetCategories();
                    return View("Form", formData);
                }

                newFileName = fileNameWithoutExtension + "-" + Guid.NewGuid() + fileExtension;

                var folderPath = Path.Combine("images", "products");

                var wwwrootFolderPath = Path.Combine(_environment.WebRootPath, folderPath);

                var wwwrootFilePath = Path.Combine(wwwrootFolderPath, newFileName);

                Directory.CreateDirectory(wwwrootFolderPath);

                using (var filestream = new FileStream(wwwrootFilePath, FileMode.Create))
                {
                    formData.File.CopyTo(filestream);
                }

            }

            if (formData.Id == 0)
            {
                var addProductDto = new AddProductDto
                {
                    Name = formData.Name.Trim(),
                    Description = formData.Description,
                    UnitPrice = formData.UnitPrice,
                    UnitsInStock = formData.UnitsInStock,
                    CategoryId = formData.CategoryId,
                    ImagePath = newFileName
                };

                _productService.AddProduct(addProductDto);
                return RedirectToAction("List");
            }
            else 
            {
                var updateProductDto = new UpdateProductDto
                {
                    Id = formData.Id,
                    Name = formData.Name,
                    Description = formData.Description,
                    UnitPrice = formData.UnitPrice,
                    UnitsInStock = formData.UnitsInStock,
                    CategoryId = formData.CategoryId,
                    ImagePath = newFileName
                };

                _productService.UpdateProduct(updateProductDto);

                return RedirectToAction("List");
            }
        }

        public IActionResult Delete(int id)
        {
            _productService.DeleteProduct(id);

            return RedirectToAction("List");
        }
    }
}
