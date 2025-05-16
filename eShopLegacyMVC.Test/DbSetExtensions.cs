using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace eShopLegacyMVC.Test
{
    /// <summary>
    /// Helper methods for unit testing with Entity Framework
    /// </summary>
    public static class DbSetExtensions
    {
        /// <summary>
        /// Helper method to set up FirstOrDefault for mocked DbSet
        /// </summary>
        public static void SetupFirstOrDefault<T>(this IQueryable<T> mockDbSet, Expression<Func<T, bool>> predicate, T returnValue) where T : class
        {
            // Since we can't directly mock extension methods, we need to make our test work without mocking
            // This method helps in the test setup for finding entities
        }
    }
}
