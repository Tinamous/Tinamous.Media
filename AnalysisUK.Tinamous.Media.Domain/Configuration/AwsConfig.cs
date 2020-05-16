using System.Configuration;
using Amazon;

namespace AnalysisUK.Tinamous.Media.Domain.Configuration
{
    public abstract class AwsConfig
    {
        public RegionEndpoint GetRegionEndpoint()
        {
            return RegionEndpoint.GetBySystemName(Region);
        }

        public string Region
        {
            get
            {
                return ConfigurationManager.AppSettings["Aws.Region"];
            }
        }

        public string ProfileName
        {
            get { return ConfigurationManager.AppSettings["Aws.ProfileName"]; }
        }
    }
}