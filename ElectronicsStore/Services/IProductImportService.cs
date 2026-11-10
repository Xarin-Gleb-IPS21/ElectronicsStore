using ElectronicsStore.Models;

namespace ElectronicsStore.Services
{
    public interface IProductImportService
    {
        ImportResult ImportFromCsv(Stream fileStream);
    }
}