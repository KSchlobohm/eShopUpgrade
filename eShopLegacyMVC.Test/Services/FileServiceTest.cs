using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using eShopLegacyMVC.Services;

namespace eShopLegacyMVC.Test.Services
{
    [TestClass]
    public class FileServiceTest
    {
        private FileServiceConfiguration _configuration;
        private FileService _service;
        private string _testBasePath;
        private readonly string[] _testFiles = { "test1.txt", "test2.txt", "test3.txt" };

        [TestInitialize]
        public void Setup()
        {
            // Create a temporary directory for testing
            _testBasePath = Path.Combine(Path.GetTempPath(), "eShopTest_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testBasePath);

            // Create test files
            foreach (var file in _testFiles)
            {
                File.WriteAllText(Path.Combine(_testBasePath, file), "Test content");
            }

            // Configure the service with local user context (no impersonation)
            _configuration = new FileServiceConfiguration
            {
                BasePath = _testBasePath,
                // Leave credentials empty to use current user context
                ServiceAccountUsername = string.Empty,
                ServiceAccountDomain = string.Empty,
                ServiceAccountPassword = string.Empty
            };

            _service = new FileService(_configuration);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test files and directory
            if (Directory.Exists(_testBasePath))
            {
                foreach (var file in Directory.GetFiles(_testBasePath))
                {
                    File.Delete(file);
                }
                Directory.Delete(_testBasePath);
            }
        }

        [TestMethod]
        public void Constructor_NullConfiguration_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new FileService(null));
        }

        [TestMethod]
        public void ListFiles_ReturnsAllFilesInBasePath()
        {
            // Act
            var files = _service.ListFiles();

            // Assert
            Assert.IsNotNull(files);
            Assert.AreEqual(_testFiles.Length, files.Count());
            foreach (var expectedFile in _testFiles)
            {
                Assert.IsTrue(files.Contains(expectedFile), $"Expected file {expectedFile} was not found");
            }
        }

        [TestMethod]
        public void DownloadFile_WithExistingFile_ReturnsFileContent()
        {
            // Arrange
            string testContent = "This is test content";
            string testFile = "download_test.txt";
            File.WriteAllText(Path.Combine(_testBasePath, testFile), testContent);

            // Act
            byte[] result = _service.DownloadFile(testFile);

            // Assert
            Assert.IsNotNull(result);
            string actualContent = System.Text.Encoding.UTF8.GetString(result);
            Assert.AreEqual(testContent, actualContent);
        }

        [TestMethod]
        public void DownloadFile_WithNonExistingFile_ThrowsFileNotFoundException()
        {
            // Arrange
            string nonExistingFile = "non_existing_file.txt";

            // Act & Assert
            Assert.ThrowsException<FileNotFoundException>(() => _service.DownloadFile(nonExistingFile));
        }

        [TestMethod]
        public void UploadFile_WithFormFileCollection_DoesNotThrow()
        {
            // Arrange
            var fileContent = new byte[] { 0x01, 0x02, 0x03 };
            var mockFormFile = new Mock<IFormFile>();
            mockFormFile.Setup(f => f.FileName).Returns("test.txt");
            mockFormFile.Setup(f => f.CopyTo(It.IsAny<Stream>()))
                .Callback<Stream>(s => new MemoryStream(fileContent).CopyTo(s));

            var mockFormFileCollection = new Mock<IFormFileCollection>();
            mockFormFileCollection.Setup(f => f.Count).Returns(1);
            mockFormFileCollection.Setup(f => f[0]).Returns(mockFormFile.Object);

            // Act & Assert - should not throw any exceptions
            _service.UploadFile(mockFormFileCollection.Object);
            Assert.IsTrue(File.Exists(Path.Combine(_testBasePath, "test.txt")));
        }
    }
}
