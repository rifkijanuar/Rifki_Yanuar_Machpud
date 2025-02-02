using Microsoft.EntityFrameworkCore;
using Rifki_Technical_Assessment.Data;
using Rifki_Technical_Assessment.Models;
using static Rifki_Technical_Assessment.Models.DTOs;

namespace Rifki_Technical_Assessment.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product> CreateProduct(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> GetProductById(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<List<Product>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        public IQueryable<Product> GetProductAll()
        {
            return _context.Products.AsQueryable();
        }

        public async Task<Product> UpdateProduct(int id, Product product)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.CreatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return existingProduct;
        }

        public async Task<int> SaveChanges()
        {
            // Save changes to the database
            return await _context.SaveChangesAsync();
        }
    }
}
