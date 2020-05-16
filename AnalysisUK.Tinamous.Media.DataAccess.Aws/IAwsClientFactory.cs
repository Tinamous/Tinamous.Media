using Amazon.DynamoDBv2;
using Amazon.S3;

namespace AnalysisUK.Tinamous.Media.DataAccess.Aws
{
    public interface IAwsClientFactory
    {
        IAmazonS3 CreateS3Client();
        IAmazonDynamoDB CreateDynamoDBClient();
    }
}