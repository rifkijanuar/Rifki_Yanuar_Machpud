using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rifki_Technical_Assessment.Controllers;
using Rifki_Technical_Assessment.Data;
using Rifki_Technical_Assessment.Models;
using Rifki_Technical_Assessment.Repositories;
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
        private  ProductController _controller;
        private  Mock<IProductRepository> _mockProductRepository;


        [TestInitialize]
        public void Setup()
        {
            _mockProductRepository = new Mock<IProductRepository>();

            var mockLogger = new Mock<ILogger<ProductController>>();
            var mockCache = new Mock<IMemoryCache>();

            _controller = new ProductController(mockLogger.Object, mockCache.Object, _mockProductRepository.Object);
        }


        [TestMethod()]
        public void GetProductTest()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product1", Description = "Desc1", Price = 10, CreatedAt = DateTime.Now };
            _mockProductRepository.Setup(r => r.GetProductById(1)).ReturnsAsync(product);

            // Act
            var actionResult = _controller.GetProduct(1).Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(actionResult);
            var actualProduct = actionResult.Value as Product;
            Assert.AreEqual(1, actualProduct.Id);
            Assert.AreEqual("Product1", actualProduct.Name);
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

            _mockProductRepository.Setup(r => r.CreateProduct(It.IsAny<Product>())).ReturnsAsync(newProduct);

            // Act
            var result = _controller.CreateProduct(productRequest).Result as CreatedAtActionResult;

            // Assert
            _mockProductRepository.Verify(r => r.CreateProduct(It.IsAny<Product>()), Times.Once);
            Assert.IsNotNull(result);
            var actualProduct = result.Value as Product;
            Assert.AreEqual(newProduct.Name, actualProduct.Name);
            Assert.AreEqual(newProduct.Description, actualProduct.Description);
            Assert.AreEqual(newProduct.Price, actualProduct.Price);
        }

        [TestMethod()]
        public async Task UpdateProductTest()
        {
            // Arrange
            var existingProduct = new Product { Id = 1, Name = "Product1", Description = "Desc1", Price = 10 };
            _mockProductRepository.Setup(r => r.GetProductById(1)).ReturnsAsync(existingProduct);

            // Simulating the update behavior: returning the updated product
            var updatedProduct = new Product { Name = "Updated Product 1", Description = "Updated Desc Product", Price = 40 };
            _mockProductRepository.Setup(r => r.UpdateProduct(1, It.IsAny<Product>())).ReturnsAsync(updatedProduct);

            // Act
            var result = await _controller.UpdateProduct(1, updatedProduct) as OkObjectResult;

            // Assert
            _mockProductRepository.Verify(r => r.UpdateProduct(1, It.IsAny<Product>()), Times.Once);
            Assert.IsNotNull(result);

            var actualProduct = result.Value as Product;
            Assert.IsNotNull(actualProduct);
            Assert.AreEqual(updatedProduct.Name, actualProduct.Name);
            Assert.AreEqual(updatedProduct.Description, actualProduct.Description);
            Assert.AreEqual(updatedProduct.Price, actualProduct.Price);
        }

        [TestMethod()]
        public void DeleteProductTest()
        {
            // Arrange
            var productToDelete = new Product { Id = 1, Name = "Product1", Price = 10 };
            _mockProductRepository.Setup(r => r.GetProductById(1)).ReturnsAsync(productToDelete);
            _mockProductRepository.Setup(r => r.DeleteProduct(It.IsAny<Product>())).Verifiable();
            _mockProductRepository.Setup(r => r.SaveChanges()).ReturnsAsync(1);  // Mock SaveChanges to return 1

            // Act
            var result = _controller.DeleteProduct(1).Result as NoContentResult;  // Should return NoContent (204)

            // Assert
            _mockProductRepository.Verify(r => r.DeleteProduct(It.IsAny<Product>()), Times.Once);  // Verify DeleteProduct was called once
            _mockProductRepository.Verify(r => r.SaveChanges(), Times.Once);  // Verify SaveChanges was called once
            Assert.IsNotNull(result);
            Assert.AreEqual(204, result.StatusCode);
        }

        //[TestMethod()]
        //public void SearchProductsTest()
        //{
        //    // Arrange
        //    var searchQuery = "Product1";
        //    var products = new List<Product>
        //    {
        //        new Product { Id = 1, Name = "Product1", Price = 10 },
        //        new Product { Id = 2, Name = "Product2", Price = 20 }
        //    };
        //    _mockProductRepository.Setup(r => r.GetProductById(searchQuery))
        //        .ReturnsAsync(new List<Product> { products[0] });

        //    // Act
        //    var result = _controller.SearchProducts(searchQuery, 0, 0).Result as OkObjectResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    var actualProducts = result.Value as List<Product>;
        //    Assert.IsNotNull(actualProducts);
        //    Assert.AreEqual(1, actualProducts.Count);
        //    Assert.AreEqual("Product1", actualProducts.First().Name);
        //}
    }

}