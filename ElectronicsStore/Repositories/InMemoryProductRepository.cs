using ElectronicsStore.Models;

namespace ElectronicsStore.Repositories
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new();

        public void Add(Product product)
        {
            if (product != null) _products.Add(product);
        }

        public IReadOnlyList<Product> GetAll() => _products;

        public Product? GetBySku(string sku) => _products.FirstOrDefault(p => p.Sku == sku);
    }
}