using AnalysisUK.Tinamous.Media.Domain.Configuration;
using NUnit.Framework;

namespace AnalysisUK.Tinamous.Media.Domain.IntegrationTests.Configuration
{
    [TestFixture]
    public class AwsDynamoDbConfigTest
    {
        [Test]
        public void TestTablePrefix()
        {
            // Arrange
            AwsDynamoDbConfig dynamoDbConfig = new AwsDynamoDbConfig();

            // Act
            string actual = dynamoDbConfig.TablePrefix;

            // Assert
            Assert.AreEqual("Test-", actual);
        }


        [Test]
        public void TestProfileName()
        {
            // Arrange
            AwsDynamoDbConfig dynamoDbConfig = new AwsDynamoDbConfig();

            // Act
            string actual = dynamoDbConfig.ProfileName;

            // Assert
            Assert.AreEqual("TinamousDev", actual);
        }

        [Test]
        public void TestRegion()
        {
            // Arrange
            AwsDynamoDbConfig dynamoDbConfig = new AwsDynamoDbConfig();

            // Act
            string actual = dynamoDbConfig.Region;

            // Assert
            Assert.AreEqual("eu-west-1", actual);
        }
    }
}