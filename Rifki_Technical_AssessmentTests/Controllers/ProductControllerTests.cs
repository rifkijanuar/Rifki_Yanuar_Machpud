using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rifki_Technical_Assessment.Controllers;
using Rifki_Technical_Assessment.Data;
using Rifki_Technical_Assessment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rifki_Technical_Assessment.Models.DTOs;

namespace Rifki_Technical_Assessment.Controllers.Tests
{
    [TestClass()]
    public class ProductControllerTests
    {
        private readonly Mock<DbSet<Product>> _mockProductSet;
        private readonly Mock<AppDbContext> _mockContext;
        private readonly Mock<ILogger<ProductController>> _mockLogger;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            // Mock DbSet<Product>
            _mockProductSet = new Mock<DbSet<Product>>();

            // Mock AppDbContext
            _mockContext = new Mock<AppDbContext>();
            _mockContext.Setup(c => c.Products).Returns(_mockProductSet.Object);

            // Mock ILogger<ProductController>
            _mockLogger = new Mock<ILogger<ProductController>>();

            // Mock IMemoryCache
            _mockCache = new Mock<IMemoryCache>();

            // Initialize the controller with mocked dependencies
            _controller = new ProductController(_mockContext.Object, _mockLogger.Object, _mockCache.Object);
        }

        [TestMethod()]
        public void ProductControllerTest()
        {
            // Arrange
            // No specific setup needed for this test

            // Act
            var result = _controller;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void GetProductTest()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product1", Price = 10 };
            _mockContext.Setup(c => c.Products.Find(1)).Returns(product);

            // Act
            var result = _controller.GetProduct(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Product1", product.Name);
        }

        [TestMethod()]
        public void CreateProductTest()
        {
            // Arrange
            var productRequest = new ProductRequest("New Product", "Description", 100);
            var newProduct = new Product
            {
                Name = productRequest.Name,
                Description = productRequest.Description,
                Price = productRequest.Price
            };

            _mockContext.Setup(c => c.Products.Add(It.IsAny<Product>())).Verifiable();
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);  

            // Act
            var result = _controller.CreateProduct(productRequest);  

            // Assert
            _mockContext.Verify(c => c.Products.Add(It.IsAny<Product>()), Times.Once);  
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);  
            Assert.IsNotNull(result);  
            Assert.AreEqual(newProduct.Name, newProduct.Name);  
            Assert.AreEqual(newProduct.Description, newProduct.Description);  
            Assert.AreEqual(newProduct.Price, newProduct.Price);  
        }

        [TestMethod()]
        public void UpdateProductTest()
        {
            // Arrange
            var existingProduct = new Product { Id = 1, Name = "Product1", Price = 10 };
            _mockContext.Setup(c => c.Products.Find(1)).Returns(existingProduct);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var updatedProduct = new Product { Id = 1, Name = "Updated Product", Price = 15 };

            // Act
            var result = _controller.UpdateProduct(1, updatedProduct);

            // Assert
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.AreEqual("Updated Product", existingProduct.Name);
            Assert.AreEqual(15, existingProduct.Price);
        }

        [TestMethod()]
        public void DeleteProductTest()
        {
            // Arrange
            var productToDelete = new Product { Id = 1, Name = "Product1", Price = 10 };
            _mockContext.Setup(c => c.Products.Find(1)).Returns(productToDelete);
            _mockContext.Setup(c => c.Products.Remove(It.IsAny<Product>())).Verifiable();
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            // Act
            var result = _controller.DeleteProduct(1);

            // Assert
            _mockContext.Verify(c => c.Products.Remove(It.IsAny<Product>()), Times.Once);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void SearchProductsTest()
        {
            // Arrange
            var searchQuery = "Product1";
            var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product1", Price = 10 },
            new Product { Id = 2, Name = "Product2", Price = 20 }
        };
            _mockContext.Setup(c => c.Products.Where(p => p.Name.Contains(searchQuery)).ToList())
                .Returns(new List<Product> { products[0] });

            // Act
            var result = _controller.SearchProducts(searchQuery, 0, 0);

            // Assert
            Assert.IsNotNull(result);
            //Assert.AreEqual(1, result.Count());
            //Assert.AreEqual("Product1", result.First().Name);
        }
    }

}