using System.IO;
using System.Text;
using Xunit;
using ElectronicsStore.Models;
using ElectronicsStore.Repositories;
using ElectronicsStore.Services;

namespace ElectronicsStore.Tests
{
    public class ProductImportServiceTests
    {
        private Stream CreateCsvStream(string csvContent)
        {
            var bytes = Encoding.UTF8.GetBytes(csvContent);
            return new MemoryStream(bytes);
        }

        // ==========================================
        // 1. ТЕСТЫ ДЛЯ РЕПОЗИТОРИЯ (InMemoryProductRepository)
        // ==========================================

        [Fact]
        public void Repository_Add_ShouldStoreProduct()
        {
            var repository = new InMemoryProductRepository();
            var product = new Product { Sku = "TEST-01", Name = "Тестовый товар" };

            repository.Add(product);

            var result = repository.GetBySku("TEST-01");
            Assert.NotNull(result);
            Assert.Equal("Тестовый товар", result.Name);
            Assert.Single(repository.GetAll());
        }

        [Fact]
        public void Repository_GetBySku_ShouldReturnNull_IfNotFound()
        {
            var repository = new InMemoryProductRepository();

            var result = repository.GetBySku("NOT-EXIST");

            Assert.Null(result);
        }

        // ==========================================
        // 2. ТЕСТЫ ДЛЯ ПАРСЕРА (Логика чтения CSV в сервисе)
        // ==========================================

        [Fact]
        public void Parser_ShouldDetectSemicolonSeparator()
        {
            var repository = new InMemoryProductRepository();
            var service = new ProductImportService(repository);
            var csv = "sku;name;description;price;currency;category_name;stock;image_url;is_active;brand;warranty_months;model_number\n" +
                      "2001;Смартфон;Описание;15000;RUB;Гаджеты;10;url;true;Xiaomi;12;ModelX";
            var stream = CreateCsvStream(csv);

            var result = service.ImportFromCsv(stream);

            Assert.Equal(1, result.ImportedCount);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Parser_ShouldHandleCyrillicUTF8()
        {
            var repository = new InMemoryProductRepository();
            var service = new ProductImportService(repository);
            var csv = "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number\n" +
                      "3001,Ёжик в тумане,Книга,500,RUB,Книги,5,url,true,Мир,0,ISBN1";
            var stream = CreateCsvStream(csv);

            var result = service.ImportFromCsv(stream);

            Assert.Equal(1, result.ImportedCount);
            Assert.Equal("Ёжик в тумане", repository.GetBySku("3001").Name);
        }

        // ==========================================
        // 3. ТЕСТЫ ДЛЯ СЕРВИСА ИМПОРТА (Валидация данных)
        // ==========================================

        [Fact]
        public void Service_NegativePrice_ReturnsError()
        {
            var repository = new InMemoryProductRepository();
            var service = new ProductImportService(repository);
            var csv = "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number\n" +
                      "1002,Мышь,Описание,-500,RUB,Периферия,10,url,true,Logitech,12,Model2";
            var stream = CreateCsvStream(csv);

            var result = service.ImportFromCsv(stream);

            Assert.Equal(0, result.ImportedCount);
            Assert.Single(result.Errors);
            Assert.Contains("Цена должна быть числом больше 0", result.Errors[0].Message);
        }

        [Fact]
        public void Service_DuplicateSku_ReturnsError()
        {
            var repository = new InMemoryProductRepository();
            repository.Add(new Product { Sku = "1003", Name = "Клавиатура" });

            var service = new ProductImportService(repository);
            var csv = "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number\n" +
                      "1003,Клавиатура 2,Описание,3000,RUB,Периферия,5,url,true,A4Tech,12,Model3";
            var stream = CreateCsvStream(csv);

            var result = service.ImportFromCsv(stream);

            Assert.Equal(0, result.ImportedCount);
            Assert.Single(result.Errors);
            Assert.Contains("уже существует", result.Errors[0].Message);
        }

        [Fact]
        public void Service_WarrantyOutOfRange_ReturnsError()
        {
            var repository = new InMemoryProductRepository();
            var service = new ProductImportService(repository);
            var csv = "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number\n" +
                      "1004,Монитор,Описание,30000,RUB,Компьютеры,5,url,true,Dell,100,Model4";
            var stream = CreateCsvStream(csv);

            var result = service.ImportFromCsv(stream);
            
            Assert.Equal(0, result.ImportedCount);
            Assert.Contains("Гарантия должна быть от 0 до 60", result.Errors[0].Message);
        }
    }
}