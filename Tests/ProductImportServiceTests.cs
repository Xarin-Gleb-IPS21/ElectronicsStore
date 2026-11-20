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

        [Fact]
        public void ImportFromCsv_ValidData_ReturnsImportedCount()
        {
            var repository = new InMemoryProductRepository();
            var service = new ProductImportService(repository);
            var csv = "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number\n" +
                      "1001,Ноутбук,Описание,45990,RUB,Компьютеры,5,url,true,Lenovo,24,Model1";
            var stream = CreateCsvStream(csv);

            var result = service.ImportFromCsv(stream);

            Assert.Equal(1, result.ImportedCount);
            Assert.Empty(result.Errors);
            Assert.NotNull(repository.GetBySku("1001"));
        }

        [Fact]
        public void ImportFromCsv_NegativePrice_ReturnsError()
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
        public void ImportFromCsv_DuplicateSku_ReturnsError()
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
    }
}