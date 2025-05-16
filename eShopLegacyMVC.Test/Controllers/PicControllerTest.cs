using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using eShopLegacyMVC.Controllers;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Services;

namespace eShopLegacyMVC.Test.Controllers
{
    [TestClass]
    public class PicControllerTest
    {
        private Mock<ICatalogService> _mockCatalogService;
        private PicController _controller;
        private Mock<HttpServerUtilityBase> _mockServer;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockHttpRequest;

        [TestInitialize]
        public void Setup()
        {
            // Setup mock services
            _mockCatalogService = new Mock<ICatalogService>();
            
            // Setup mock HTTP context
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockHttpRequest = new Mock<HttpRequestBase>();
            _mockServer = new Mock<HttpServerUtilityBase>();
              // Setup controller with HttpContext
            _controller = new PicController(_mockCatalogService.Object);
            _controller.ControllerContext = new ControllerContext(
                _mockHttpContext.Object,
                new System.Web.Routing.RouteData(),
                _controller);
            
            // Setup Server.MapPath - we need to set it through reflection since it's read-only
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContext.Setup(c => c.Server).Returns(_mockServer.Object);
            
            // Set the controller's HttpContext instead of trying to set Server directly
            var controllerContextType = typeof(ControllerContext);
            var httpContextProperty = controllerContextType.GetProperty("HttpContext");
            httpContextProperty.SetValue(_controller.ControllerContext, _mockHttpContext.Object);
        }

        [TestMethod]
        public void Index_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            int invalidId = 0;

            // Act
            var result = _controller.Index(invalidId) as HttpStatusCodeResult;

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
            var result = _controller.Index(nonExistingId) as HttpNotFoundResult;

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
                
                // Setup Server.MapPath to return our test directory
                _mockServer.Setup(s => s.MapPath(It.IsAny<string>())).Returns(webRoot);

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
