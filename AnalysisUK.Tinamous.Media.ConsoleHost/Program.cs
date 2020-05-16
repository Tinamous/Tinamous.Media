using System;
using System.Configuration;
using AnalysisUK.Tinamous.Media.BL;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.LogMessage("Starting Tinamous Media Service.");

            IBus bus = CreateBus();
            // Second bus for event processing
            IBus eventProcessorBus = CreateBus();

            using (var initialisor = new Initialisor(bus, eventProcessorBus ))
            {
                initialisor.Initialize();

                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }

            bus.Dispose();
            eventProcessorBus .Dispose();
            
        }

        private static IBus CreateBus()
        {
            string connectionString = GetConnectionString("RabbitMQConnectionString");

            Logger.Trace("Using RabbitMQ connection String: {0}", connectionString);

            var bus = RabbitHutch.CreateBus(connectionString, reg => { reg.EnableLegacyTypeNaming(); });
            return bus;
        }

        private static string GetConnectionString(string name)
        {
            return ConfigurationManager
                .ConnectionStrings[name]
                .ConnectionString;
        }
    }
}
