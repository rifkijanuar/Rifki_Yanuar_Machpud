using Rifki_Technical_Assessment.Models;
using static Rifki_Technical_Assessment.Models.DTOs;

namespace Rifki_Technical_Assessment.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetProducts();

        Task <Product> GetProductById(int id);

        Task<Product> CreateProduct(Product product);

        Task<Product> UpdateProduct(int id, Product product);

        Task<Product> DeleteProduct(Product product);

        IQueryable<Product> GetProductAll();

        Task<int> SaveChanges();
    }
}
