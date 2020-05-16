using System;
using AnalysisUK.Tinamous.Media.Domain.Configuration;
using AnalysisUK.Tinamous.Media.Domain.Helpers;
using NUnit.Framework;

namespace AnalysisUK.Tinamous.Media.Domain.IntegrationTests.Configuration
{
    [TestFixture]
    public class AwsS3ConfigTest
    {
        [TearDown]
        public void TearDown()
        {
            SystemDate.Reset();
        }

        [Test]
        public void ProcessedImagesBucket_GetsBucketName()
        {
            // Arrange
            AwsS3Config awsS3Config = new AwsS3Config();

            // Act
            string actal = awsS3Config.ProcessedImagesBucket;

            // Assert
            Assert.AreEqual("tinamous-dev-images-eu", actal);
        }

        [Test]
        public void UploadedImagesBucket_WithDatePlaceHolders_HasCurrentValues()
        {
            // Arrange
            AwsS3Config awsS3Config = new AwsS3Config();
            SystemDate.Set(new DateTime(2020, 03, 01));

            // Act
            string actal = awsS3Config.UploadedImagesBucket;

            // Assert
            string expected = "tinamous-debug-media-upload-2-2020-3-1";
            Assert.AreEqual(expected, actal);
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