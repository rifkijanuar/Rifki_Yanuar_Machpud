using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Rifki_Technical_Assessment.Data;
using Rifki_Technical_Assessment.Models;
using Rifki_Technical_Assessment.Repositories;
using Serilog;
using static Rifki_Technical_Assessment.Models.DTOs;

namespace Rifki_Technical_Assessment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IProductRepository _productRepository;

        public ProductController(ILogger<ProductController> logger, IMemoryCache cache, IProductRepository productRepository)
        {
            _logger = logger;
            _cache = cache;
            _productRepository = productRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProducts()
        {
            var cacheKey = "all_products";
            if (!_cache.TryGetValue(cacheKey, out List<Product> products))
            {
                // If not found in cache, fetch from database
                products = await _productRepository.GetProducts();

                // Set cache options (e.g., cache for 5 minutes)
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                // Store the data in the cache
                _cache.Set(cacheKey, products, cacheOptions);
            }

            return Ok(products);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                _logger.LogInformation("Received request to find a product: {Id}", id);
                var cacheKey = $"product_{id}";
                if (!_cache.TryGetValue(cacheKey, out Product product))
                {
                    // If not found in cache, fetch from database
                    //product = await _context.Products.FindAsync(id);
                    product = await _productRepository.GetProductById(id);
                    if (product == null)
                    {
                        return NotFound();
                    }

                    // Set cache options (e.g., cache for 5 minutes)
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                    // Store the product in the cache
                    //_cache.Set(cacheKey, product, cacheOptions);
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while fetching product");
                return StatusCode(500, "Internal server error while fetching product");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProduct(ProductRequest productRequest)
        {
            try
            {
                _logger.LogInformation("Received request to create a product: {ProductName}", productRequest.Name);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model state is invalid for product: {ProductName}", productRequest.Name);
                    return BadRequest(ModelState); // Return validation errors
                }

                var product = new Product
                {
                    Name = productRequest.Name,
                    Description = productRequest.Description,
                    Price = productRequest.Price,
                    CreatedAt = DateTime.Now,
                };

                _logger.LogInformation("Creating product with name: {ProductName}", product.Name);
                //_context.Products.Add(product);
                //await _context.SaveChangesAsync();
                await _productRepository.CreateProduct(product);

                // Clear the cache after creating a product
                _cache.Remove("all_products");

                _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while creating product");
                return StatusCode(500, "Internal server error while creating product");
            }

        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int id, Product productRequest)
        {
            try
            {
                _logger.LogInformation("Received request to update a product: {Id}", id);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState); // Return validation errors
                }

                var existingProduct = await _productRepository.GetProductById(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                var updatedProduct = await _productRepository.UpdateProduct(id, productRequest);
                _logger.LogInformation("Product updating successfully with ID: {ProductId}", id);
                return Ok(updatedProduct);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while updating product");
                return StatusCode(500, "Internal server error while updating product");
            }

        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                _logger.LogInformation("Received request to delete a product: {Id}", id);
                var product = await _productRepository.GetProductById(id);
                if (product == null) return NotFound();

                _logger.LogInformation("Deleting product with Id: {Id}", product.Id);
                await _productRepository.DeleteProduct(product);
                await _productRepository.SaveChanges();
                _logger.LogInformation("Product delete successfully with ID: {ProductId}", product.Id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while updating product");
                return StatusCode(500, "Internal server error while delete product");
            }

        }

        // Search and Filter
        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchProducts([FromQuery] string name, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "Name")
        {
            try
            {
                var query = _productRepository.GetProductAll();

                // Filter by name if provided
                _logger.LogInformation("Received request to search a product Name: {ProductName}", name);
                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(p => p.Name.Contains(name));
                }

                // Filter by price range if provided
                _logger.LogInformation("Received request to search a product min price : {ProductPrice}", minPrice);
                _logger.LogInformation("Received request to search a product max price : {ProductPrice}", maxPrice);
                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }
                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                // Sorting
                query = sortBy.ToLower() switch
                {
                    "price" => query.OrderBy(p => p.Price),
                    "name" => query.OrderBy(p => p.Name),
                    _ => query.OrderBy(p => p.Name),
                };

                // Pagination
                var totalItems = await query.CountAsync();
                var products = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Products = products
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while updating product");
                return StatusCode(500, "Internal server error while delete product");
            }
        }

    }
}
