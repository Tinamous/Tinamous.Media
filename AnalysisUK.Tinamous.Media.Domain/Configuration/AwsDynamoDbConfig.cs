using System.Configuration;

namespace AnalysisUK.Tinamous.Media.Domain.Configuration
{
    public class AwsDynamoDbConfig : AwsConfig
    {
        public string TablePrefix
        {
            get { return ConfigurationManager.AppSettings["Aws.DynamoDb.TablePrefix"]; }
        }
    }
}