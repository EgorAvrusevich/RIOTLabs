using OnlineStore.Server.Data;
using OnlineStore.Shared.Models;

namespace OnlineStore.Server.Services;

public class ProductService : IProductService
{
    public List<Product> GetProducts() => InMemoryStore.Products.ToList();
}
