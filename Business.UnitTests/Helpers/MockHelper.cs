using Business.UnitTests.Contexts;
using DataAccess.Concrete;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.UnitTests.Helpers
{
    internal static class MockHelper
    {
        public static Mock<TestEntities> CreateTestContext(Action<ApplicationDbContext> fillFunc = null)
        {
            DbContextOptions<TestEntities> contextOptions = new DbContextOptionsBuilder<TestEntities>()
                .UseInMemoryDatabase(TestContext.CurrentContext.Test.ID)
                .Options;

            var contextMock = new Mock<TestEntities>(contextOptions) { CallBase = true };

            contextMock.Object.Database.EnsureCreated();

            if (fillFunc != null)
            {
                fillFunc(contextMock.Object);
                contextMock.Object.SaveChanges();
            }

            return contextMock;
        }
    }
}
