using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using eShopLegacyMVC.Controllers;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Services;
using eShopLegacyMVC.ViewModel;

namespace eShopLegacyMVC.Test.Controllers
{
    [TestClass]
    public class CatalogControllerTest
    {
        private Mock<ICatalogService> _mockCatalogService;
        private CatalogController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockCatalogService = new Mock<ICatalogService>();
            _controller = new CatalogController(_mockCatalogService.Object);
        }

        [TestMethod]
        public void Details_WithNullId_ReturnsBadRequest()
        {
            // Act
            var result = _controller.Details(null) as HttpStatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod]
        public void Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int invalidId = 999;
            _mockCatalogService.Setup(s => s.FindCatalogItem(invalidId)).Returns((CatalogItem)null);

            // Act
            var result = _controller.Details(invalidId) as HttpNotFoundResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Create_Get_ReturnsViewResult_WithBrandsAndTypes()
        {
            // Arrange
            var brands = new List<CatalogBrand> { new CatalogBrand { Id = 1, Brand = "Brand 1" } };
            var types = new List<CatalogType> { new CatalogType { Id = 1, Type = "Type 1" } };
            _mockCatalogService.Setup(s => s.GetCatalogBrands()).Returns(brands);
            _mockCatalogService.Setup(s => s.GetCatalogTypes()).Returns(types);

            // Act
            var result = _controller.Create() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as CatalogItem;
            Assert.IsNotNull(model);
            Assert.IsNotNull(result.ViewBag.CatalogBrandId);
            Assert.IsNotNull(result.ViewBag.CatalogTypeId);
        }

        [TestMethod]
        public void Create_Post_WithInvalidModel_ReturnsView_WithBrandsAndTypes()
        {
            // Arrange
            var brands = new List<CatalogBrand> { new CatalogBrand { Id = 1, Brand = "Brand 1" } };
            var types = new List<CatalogType> { new CatalogType { Id = 1, Type = "Type 1" } };
            _mockCatalogService.Setup(s => s.GetCatalogBrands()).Returns(brands);
            _mockCatalogService.Setup(s => s.GetCatalogTypes()).Returns(types);

            var catalogItem = new CatalogItem { Id = 1, Name = null };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.Create(catalogItem) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as CatalogItem;
            Assert.IsNotNull(model);
            Assert.AreEqual(catalogItem.Id, model.Id);
            Assert.IsNotNull(result.ViewBag.CatalogBrandId);
            Assert.IsNotNull(result.ViewBag.CatalogTypeId);
        }

        [TestMethod]
        public void Edit_Get_WithNullId_ReturnsBadRequest()
        {
            // Act
            var result = _controller.Edit((int?)null) as HttpStatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod]
        public void Edit_Get_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int invalidId = 999;
            _mockCatalogService.Setup(s => s.FindCatalogItem(invalidId)).Returns((CatalogItem)null);

            // Act
            var result = _controller.Edit(invalidId) as HttpNotFoundResult;

            // Assert            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Edit_Post_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var catalogItem = new CatalogItem { Id = 1, Name = "Updated Item" };
            _mockCatalogService.Setup(s => s.UpdateCatalogItem(catalogItem)).Verifiable();

            // Act
            var result = _controller.Edit(catalogItem) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            _mockCatalogService.Verify();
        }

        [TestMethod]
        public void Delete_Get_WithNullId_ReturnsBadRequest()
        {
            // Act
            var result = _controller.Delete(null) as HttpStatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod]
        public void DeleteConfirmed_WithValidId_RedirectsToIndex()
        {
            // Arrange
            int validId = 1;
            var catalogItem = new CatalogItem { Id = validId, Name = "Test Item" };
            _mockCatalogService.Setup(s => s.FindCatalogItem(validId)).Returns(catalogItem);
            _mockCatalogService.Setup(s => s.RemoveCatalogItem(catalogItem)).Verifiable();

            // Act
            var result = _controller.DeleteConfirmed(validId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            _mockCatalogService.Verify();
        }
    }
}
