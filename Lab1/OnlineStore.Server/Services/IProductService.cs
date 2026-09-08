using OnlineStore.Shared.Models;

namespace OnlineStore.Server.Services;

public interface IProductService
{
    List<Product> GetProducts();
}
