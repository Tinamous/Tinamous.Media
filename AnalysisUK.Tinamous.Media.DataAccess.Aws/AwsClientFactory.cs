using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using AnalysisUK.Tinamous.Media.Domain.Configuration;

namespace AnalysisUK.Tinamous.Media.DataAccess.Aws
{
    /// <summary>
    /// AWS Client Factory.
    ///
    /// For development looks for a named profile.
    /// For production uses roles associated with EC2 instance.
    /// </summary>
    public class AwsClientFactory : IAwsClientFactory
    {
        public IAmazonS3 CreateS3Client()
        {
            var config = AwsConfigFactory.GetS3Config();
            RegionEndpoint region = config.GetRegionEndpoint();

            CredentialProfileStoreChain credentialProfileStoreChain = new CredentialProfileStoreChain();

            CredentialProfile profile;
            if (credentialProfileStoreChain.TryGetProfile(config.ProfileName , out profile))
            {
                AWSCredentials awsCredentials;
                if (AWSCredentialsFactory.TryGetAWSCredentials(profile, null, out awsCredentials))
                {
                    return new AmazonS3Client(awsCredentials, region);
                }
            }

            // Production. Uses roles.
            return new AmazonS3Client(region);
        }

        public IAmazonDynamoDB CreateDynamoDBClient()
        {
            var config = AwsConfigFactory.GetDynamoDbConfig();
            RegionEndpoint region = config.GetRegionEndpoint();

            CredentialProfileStoreChain credentialProfileStoreChain = new CredentialProfileStoreChain();

            CredentialProfile profile;
            if (credentialProfileStoreChain.TryGetProfile(config.ProfileName, out profile))
            {
                AWSCredentials awsCredentials;
                if (AWSCredentialsFactory.TryGetAWSCredentials(profile, null, out awsCredentials))
                {
                    return new AmazonDynamoDBClient(awsCredentials, region);
                }
            }

            // Production. Uses roles.
            
            return new AmazonDynamoDBClient(region);
        }
    }
}