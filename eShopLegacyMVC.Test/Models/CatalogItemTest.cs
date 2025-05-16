using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using eShopLegacyMVC.Models;
using System.Linq;

namespace eShopLegacyMVC.Test.Models
{
    [TestClass]
    public class CatalogItemTest
    {
        [TestMethod]
        public void Constructor_SetsDefaultPictureName()
        {
            // Act
            var item = new CatalogItem();

            // Assert
            Assert.AreEqual(CatalogItem.DefaultPictureName, item.PictureFileName);
        }

        [TestMethod]
        public void Name_Required_ValidationFails_WhenEmpty()
        {
            // Arrange
            var item = new CatalogItem
            {
                Name = string.Empty,
                Price = 10.0m,
                CatalogBrandId = 1,
                CatalogTypeId = 1
            };

            // Act
            var validationResults = ValidateModel(item);

            // Assert
            Assert.IsTrue(HasValidationError(validationResults, "Name", "The Name field is required."));
        }

        [TestMethod]
        public void Price_Range_ValidationFails_WhenNegative()
        {
            // Arrange
            var item = new CatalogItem
            {
                Name = "Test Item",
                Price = -10.0m,
                CatalogBrandId = 1,
                CatalogTypeId = 1
            };

            // Act
            var validationResults = ValidateModel(item);

            // Assert
            Assert.IsTrue(HasValidationError(validationResults, "Price", "The field Price must be between 0 and 1000000."));
        }

        [TestMethod]
        public void Price_Range_ValidationFails_WhenTooLarge()
        {
            // Arrange
            var item = new CatalogItem
            {
                Name = "Test Item",
                Price = 2000000m,
                CatalogBrandId = 1,
                CatalogTypeId = 1
            };

            // Act
            var validationResults = ValidateModel(item);

            // Assert
            Assert.IsTrue(HasValidationError(validationResults, "Price", "The field Price must be between 0 and 1000000."));
        }

        [TestMethod]
        public void Price_Format_ValidationFails_WithTooManyDecimals()
        {
            // Arrange
            var item = new CatalogItem
            {
                Name = "Test Item",
                Price = 10.999m,  // Three decimals should fail
                CatalogBrandId = 1,
                CatalogTypeId = 1
            };

            // Act
            var validationResults = ValidateModel(item);

            // Assert
            Assert.IsTrue(HasValidationError(validationResults, "Price", "The field Price must be a positive number with maximum two decimals."));
        }

        [TestMethod]
        public void AvailableStock_Range_ValidationFails_WhenNegative()
        {
            // Arrange
            var item = new CatalogItem
            {
                Name = "Test Item",
                Price = 10.0m,
                CatalogBrandId = 1,
                CatalogTypeId = 1,
                AvailableStock = -10
            };

            // Act
            var validationResults = ValidateModel(item);

            // Assert
            Assert.IsTrue(HasValidationError(validationResults, "AvailableStock", "The field Stock must be between 0 and 10 million."));
        }

        [TestMethod]
        public void RestockThreshold_Range_ValidationFails_WhenNegative()
        {
            // Arrange
            var item = new CatalogItem
            {
                Name = "Test Item",
                Price = 10.0m,
                CatalogBrandId = 1,
                CatalogTypeId = 1,
                RestockThreshold = -10
            };

            // Act
            var validationResults = ValidateModel(item);

            // Assert
            Assert.IsTrue(HasValidationError(validationResults, "RestockThreshold", "The field Restock must be between 0 and 10 million."));
        }

        [TestMethod]
        public void MaxStockThreshold_Range_ValidationFails_WhenNegative()
        {
            // Arrange
            var item = new CatalogItem
            {
                Name = "Test Item",
                Price = 10.0m,
                CatalogBrandId = 1,
                CatalogTypeId = 1,
                MaxStockThreshold = -10
            };

            // Act
            var validationResults = ValidateModel(item);

            // Assert
            Assert.IsTrue(HasValidationError(validationResults, "MaxStockThreshold", "The field Max stock must be between 0 and 10 million."));
        }

        [TestMethod]
        public void ValidModel_PassesAllValidations()
        {
            // Arrange
            var item = new CatalogItem
            {
                Name = "Test Item",
                Price = 10.99m,
                CatalogBrandId = 1,
                CatalogTypeId = 1,
                AvailableStock = 100,
                RestockThreshold = 10,
                MaxStockThreshold = 1000
            };

            // Act
            var validationResults = ValidateModel(item);

            // Assert
            Assert.AreEqual(0, validationResults.Count);
        }

        #region Helpers

        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, validationResults, true);
            return validationResults;
        }

        private bool HasValidationError(IList<ValidationResult> validationResults, string propertyName, string errorMessage)
        {
            foreach (var validationResult in validationResults)
            {
                if (validationResult.MemberNames.Contains(propertyName) && validationResult.ErrorMessage.Contains(errorMessage))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
