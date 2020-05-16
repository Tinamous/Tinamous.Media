using System;
using System.Configuration;
using System.Reflection;
using System.ServiceProcess;
using AnalysisUK.Tinamous.Media.BL;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using EasyNetQ;
using Exceptionless;

namespace AnalysisUK.Tinamous.Media.ServiceHost
{
    public partial class MediaServce : ServiceBase
    {
        private IBus _bus;
        private IBus _eventProcessorBus;
        private Initialisor _initialisor;

        public MediaServce()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Logger.LogMessage("======================================================");
            Logger.LogMessage("Tinamous Media Service Starting. " + version);
            Logger.LogMessage("======================================================");
            ExceptionlessClient.Default.Startup();
            ExceptionlessClient.Default.SubmitLog("Media Service starting up. Version: " + version);

            InitializeMqBus();

            Start();
        }

        private void Start()
        {
            try
            {

                _initialisor = new Initialisor(_bus, _eventProcessorBus);
                _initialisor.Initialize();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Initialisor failed to start correctly for Media Service.");
                ExceptionlessClient.Default.SubmitLog("Media service did not start correctly.");
                throw;
            }
        }

        private void InitializeMqBus()
        {
            string connectionString = GetConnectionString("RabbitMQConnectionString");
            _bus = RabbitHutch.CreateBus(connectionString, reg => { reg.EnableLegacyTypeNaming(); });
            _eventProcessorBus = RabbitHutch.CreateBus(connectionString, reg => { reg.EnableLegacyTypeNaming(); });
        }


        protected override void OnStop()
        {
            Logger.LogMessage("Stopping Media Service");
            ExceptionlessClient.Default.SubmitLog("Media Service stopping.");

            _initialisor.Dispose();
            _bus.Dispose();
            _bus = null;

            _eventProcessorBus.Dispose();
            _eventProcessorBus = null;

            Logger.LogMessage("Media Service Stopped");
            Logger.LogMessage("========================================================");
        }

        private static string GetConnectionString(string name)
        {
            return ConfigurationManager
                .ConnectionStrings[name]
                .ConnectionString;
        }
    }
}
