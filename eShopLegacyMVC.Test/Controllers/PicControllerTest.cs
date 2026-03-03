using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
        private Mock<IWebHostEnvironment> _mockEnv;
        private PicController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockCatalogService = new Mock<ICatalogService>();
            _mockEnv = new Mock<IWebHostEnvironment>();
            _controller = new PicController(_mockCatalogService.Object, _mockEnv.Object);
        }

        [TestMethod]
        public void Index_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            int invalidId = 0;

            // Act
            var result = _controller.Index(invalidId) as BadRequestResult;

            // Assert
            Assert.IsNotNull(result);
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
            // Arrange
            int validId = 1;
            string testFileName = "testimage.png";
            string webRoot = Path.Combine(Path.GetTempPath(), "testpics_" + Guid.NewGuid());
            string picsDir = Path.Combine(webRoot, "Pics");
            string testFilePath = Path.Combine(picsDir, testFileName);

            Directory.CreateDirectory(picsDir);

            byte[] testFileContent = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header
            try
            {
                File.WriteAllBytes(testFilePath, testFileContent);

                var catalogItem = new CatalogItem { Id = validId, Name = "Test Item", PictureFileName = testFileName };
                _mockCatalogService.Setup(s => s.FindCatalogItem(validId)).Returns(catalogItem);
                _mockEnv.Setup(e => e.WebRootPath).Returns(webRoot);

                // Act
                var result = _controller.Index(validId) as FileContentResult;

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("image/png", result.ContentType);
                CollectionAssert.AreEqual(testFileContent, result.FileContents);
            }
            finally
            {
                if (File.Exists(testFilePath))
                    File.Delete(testFilePath);
                if (Directory.Exists(picsDir))
                    Directory.Delete(picsDir);
                if (Directory.Exists(webRoot))
                    Directory.Delete(webRoot);
            }
        }
    }
}
