using System.Configuration;

namespace AnalysisUK.Tinamous.Media.Domain.Configuration
{
    public static class ServerSettings
    {
        public static string ServerName
        {
            get { return ConfigurationManager.AppSettings["Octopus.Machine.Name"]; }
        }
    }
}