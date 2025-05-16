using System;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using eShopLegacyMVC.Controllers;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;



namespace eShopLegacyMVC.Test.Controllers
{
    [TestClass]
    public class PicControllerTest
    {
        private Mock<ICatalogService> _mockCatalogService;
        private PicController _controller;
        private Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        [TestInitialize]
        public void Setup()
        {
            // Setup mock services
            _mockCatalogService = new Mock<ICatalogService>();
            
            // Setup mock services
            _mockHttpContext = new Mock<HttpContext>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Configure HttpContextAccessor
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

            // Setup controller
            _controller = new PicController(_mockCatalogService.Object, _mockWebHostEnvironment.Object);

            // Create a controller context
            var actionContext = new ActionContext(
                _mockHttpContext.Object,
                new RouteData(),
                new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());

            _controller.ControllerContext = new ControllerContext(actionContext);
        }

        [TestMethod]
        public void Index_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            int invalidId = 0;

            // Act
            var result = _controller.Index(invalidId) as StatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod]
        public void Index_WithNonExistingItem_ReturnsNotFound()
        {
            // Arrange
            int nonExistingId = 999;
            _mockCatalogService.Setup(s => s.FindCatalogItem(nonExistingId)).Returns((CatalogItem)null);

            // Act
            var result = _controller.Index(nonExistingId) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Index_WithValidItem_ReturnsFileResult()
        {
            // This test requires special handling because it accesses the file system
            // We'll need to create a mock file system or setup Server.MapPath to return a path that works

            // Arrange
            int validId = 1;
            string testFileName = "testimage.png";
            string webRoot = Path.Combine(Path.GetTempPath(), "testpics");
            string testFilePath = Path.Combine(webRoot, testFileName);
            
            // Ensure test directory exists
            Directory.CreateDirectory(webRoot);
            
            // Create a test file
            byte[] testFileContent = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header
            try
            {
                File.WriteAllBytes(testFilePath, testFileContent);

                var catalogItem = new CatalogItem { Id = validId, Name = "Test Item", PictureFileName = testFileName };
                _mockCatalogService.Setup(s => s.FindCatalogItem(validId)).Returns(catalogItem);
                
                // Setup WebHostEnvironment to return our test directory
                _mockWebHostEnvironment.Setup(s => s.ContentRootPath).Returns(webRoot);
                _mockWebHostEnvironment.Setup(s => s.WebRootPath).Returns(webRoot);

                // Act
                var result = _controller.Index(validId) as FileContentResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("image/png", result.ContentType);
                CollectionAssert.AreEqual(testFileContent, result.FileContents);
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath))
                    File.Delete(testFilePath);
                
                if (Directory.Exists(webRoot))
                    Directory.Delete(webRoot);
            }
        }
    }
}