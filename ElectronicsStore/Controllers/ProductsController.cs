using ElectronicsStore.Models;
using ElectronicsStore.Repositories;
using ElectronicsStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicsStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _repository;
        private readonly IProductImportService _importService;

        public ProductsController(IProductRepository repository, IProductImportService importService)
        {
            _repository = repository;
            _importService = importService;
        }

        public IActionResult Index()
        {
            var products = _repository.GetAll();
            return View(products);
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Пожалуйста, выберите CSV-файл.";
                return View();
            }

            try
            {
                using var stream = file.OpenReadStream();              
                var result = _importService.ImportFromCsv(stream);                              
                return View("ImportResult", result);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ошибка: {ex.Message}";
                return View("Import"); 
            }
        }
    }
}