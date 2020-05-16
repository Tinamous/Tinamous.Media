namespace AnalysisUK.Tinamous.Media.Domain.Configuration
{
    public static class AwsConfigFactory
    {
        public static AwsS3Config GetS3Config()
        {
            return new AwsS3Config();
        }

        public static AwsDynamoDbConfig GetDynamoDbConfig()
        {
            return new AwsDynamoDbConfig();
        }
    }
}