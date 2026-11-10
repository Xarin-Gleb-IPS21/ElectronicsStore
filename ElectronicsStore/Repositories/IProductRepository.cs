using ElectronicsStore.Models;

namespace ElectronicsStore.Repositories
{
    public interface IProductRepository
    {
        void Add(Product product);
        IReadOnlyList<Product> GetAll();
        Product? GetBySku(string sku);
    }
}