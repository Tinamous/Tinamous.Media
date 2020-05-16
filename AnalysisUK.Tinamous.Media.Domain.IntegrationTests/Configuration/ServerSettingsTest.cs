using AnalysisUK.Tinamous.Media.Domain.Configuration;
using NUnit.Framework;

namespace AnalysisUK.Tinamous.Media.Domain.IntegrationTests.Configuration
{
    [TestFixture]
    public class ServerSettingsTest
    {
        [Test]
        public void ServerName_GetsServerName()
        {
            // Arrange
            
            // Act
            string serverName = ServerSettings.ServerName;

            // Assert
            Assert.AreEqual("Test", serverName);
        }
    }
}