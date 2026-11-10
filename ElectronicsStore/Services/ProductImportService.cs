using System;
using System.IO;
using System.Text;
using ElectronicsStore.Models;
using ElectronicsStore.Repositories;

namespace ElectronicsStore.Services
{
    public class ProductImportService : IProductImportService
    {
        private readonly IProductRepository _repository;

        public ProductImportService(IProductRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public ImportResult ImportFromCsv(Stream fileStream)
        {
            var result = new ImportResult();
            try
            {
                using var reader = new StreamReader(fileStream, Encoding.UTF8);

                var headerLine = reader.ReadLine();
                if (headerLine == null)
                {
                    result.Errors.Add(new ImportError { LineNumber = 0, Message = "Файл пуст" });
                    return result;
                }

                int lineNumber = 1;

                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    result.TotalRows++;

                    char separator = line.Contains(';') ? ';' : ',';
                    var values = line.Split(separator);

                    if (values.Length < 12)
                    {
                        result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = $"Неверное количество колонок (найдено {values.Length}, ожидается 12)" });
                        continue;
                    }

                    try
                    {
                        var sku = values[0].Trim(' ', '"');
                        var name = values[1].Trim(' ', '"');
                        var description = values[2].Trim(' ', '"');
                        var priceStr = values[3].Trim(' ', '"');
                        var currency = values[4].Trim(' ', '"');
                        var category = values[5].Trim(' ', '"');
                        var stockStr = values[6].Trim(' ', '"');
                        var imageUrl = values[7].Trim(' ', '"');
                        var isActiveStr = values[8].Trim(' ', '"');
                        var brand = values[9].Trim(' ', '"');
                        var warrantyStr = values[10].Trim(' ', '"');
                        var modelNumber = values[11].Trim(' ', '"');

                        if (string.IsNullOrEmpty(sku)) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = "Артикул (sku) обязателен" }); continue; }
                        if (_repository.GetBySku(sku) != null) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = $"Артикул {sku} уже существует" }); continue; }
                        if (string.IsNullOrEmpty(name)) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = "Название обязательно" }); continue; }
                        if (string.IsNullOrEmpty(brand)) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = "Бренд обязателен" }); continue; }

                        if (!decimal.TryParse(priceStr, out var price) || price <= 0) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = "Цена должна быть числом больше 0" }); continue; }
                        if (!int.TryParse(stockStr, out var stock) || stock < 0) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = "Остаток не может быть отрицательным" }); continue; }

                        bool isActive = isActiveStr.ToLower() == "true" || isActiveStr == "1";

                        if (!int.TryParse(warrantyStr, out var warranty) || warranty < 0 || warranty > 60) { result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = "Гарантия должна быть от 0 до 60" }); continue; }

                        var product = new Product
                        {
                            Sku = sku,
                            Name = name,
                            Description = description,
                            Price = price,
                            Currency = string.IsNullOrEmpty(currency) ? "RUB" : currency,
                            CategoryName = string.IsNullOrEmpty(category) ? "Без категории" : category,
                            StockQuantity = stock,
                            ImageUrl = imageUrl,
                            IsActive = isActive,
                            Brand = brand,
                            WarrantyMonths = warranty,
                            ModelNumber = modelNumber
                        };

                        _repository.Add(product);
                        result.ImportedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ImportError { LineNumber = lineNumber, Message = $"Ошибка строки: {ex.Message}" });
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportError { LineNumber = 0, Message = $"Критическая ошибка: {ex.Message}" });
            }

            return result;
        }
    }
}