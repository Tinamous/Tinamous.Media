using System;
using AnalysisUK.Tinamous.Media.Domain.Helpers;
using NUnit.Framework;

namespace AnalysisUK.Tinamous.Media.Domain.Tests.Helpers
{
    [TestFixture]
    public class DateTimeExtensionTest
    {
        [Test]
        public void ToUnixSeconds_ForEpochDate_ReturnsExpectedTime()
        {
            // Arrange
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            decimal seconds = date.ToUnixSeconds();

            // Assert
            Assert.AreEqual(0M, seconds);
        }

        [Test]
        public void ToUnixSeconds_ForDate_ReturnsExpectedTime()
        {
            // Arrange
            DateTime date = new DateTime(2020, 03, 29, 15, 39, 0, DateTimeKind.Utc);

            // Act
            decimal seconds = date.ToUnixSeconds();

            // Assert
            Assert.AreEqual(1585496340M, seconds);
        }

        [Test]
        public void ToLongUnixSeconds_ForEpochDate_ReturnsExpectedTime()
        {
            // Arrange
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            long seconds = date.ToLongUnixSeconds();

            // Assert
            Assert.AreEqual(0, seconds);
        }

        [Test]
        public void ToLongUnixSeconds_ForDate_ReturnsExpectedTime()
        {
            // Arrange
            DateTime date = new DateTime(2020, 03, 29, 15, 39, 0, DateTimeKind.Utc);

            // Act
            long seconds = date.ToLongUnixSeconds();

            // Assert
            Assert.AreEqual(1585496340, seconds);
        }
    }
}