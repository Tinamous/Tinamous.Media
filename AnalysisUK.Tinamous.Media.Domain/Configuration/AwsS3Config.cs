using System.Configuration;
using AnalysisUK.Tinamous.Media.Domain.Helpers;

namespace AnalysisUK.Tinamous.Media.Domain.Configuration
{
    public class AwsS3Config : AwsConfig
    {
        public string ProcessedImagesBucket
        {
            get
            {
                return ConfigurationManager.AppSettings["Aws.S3.ProcessedImagesBucket"];
            }
        }

        public string UploadedImagesBucket
        {
            get
            {
                string bucket = ConfigurationManager.AppSettings["Aws.S3.MediaUploadBucket"];
                int year = SystemDate.UtcNow.Year;
                int month = SystemDate.UtcNow.Month;
                int day = SystemDate.UtcNow.Day;
                return string.Format(bucket, year, month, day);
            }
        }
    }
}