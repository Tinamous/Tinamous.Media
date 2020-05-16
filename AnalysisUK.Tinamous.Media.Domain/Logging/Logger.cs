using System;
using System.ComponentModel;
using System.Diagnostics;
using Exceptionless;

namespace AnalysisUK.Tinamous.Media.Domain.Logging
{
    public static class Logger
    {
        private static readonly NLog.Logger LogManager = NLog.LogManager.GetLogger("Tinamous");

        [Localizable(false)]
        public static void LogMessage(string message)
        {
            Debug.WriteLine(message);
            LogManager.Info(message);
        }

        [Localizable(false)]
        public static void LogMessage(string message, params object[] args)
        {
            Debug.WriteLine(message, args);
            LogManager.Info(message, args);
        }

        [Localizable(false)]
        public static void LogException(Exception ex, string message, params object[] args)
        {
            LogManager.Error(message, ex, args);
            ex.ToExceptionless().Submit();

            System.Diagnostics.Trace.WriteLine(string.Format(message, args));
            System.Diagnostics.Trace.WriteLine(ex.ToString());
        }

        [Localizable(false)]
        public static void LogWarn(string message)
        {
            Debug.WriteLine(message);
            LogManager.Warn(message);
        }

        [Localizable(false)]
        public static void LogWarn(string message, params object[] args)
        {
            Debug.WriteLine(message, args);
            LogManager.Warn(message, args);
        }

        [Localizable(false)]
        public static void LogError(string message)
        {
            Debug.WriteLine(message);
            LogManager.Error(message);
        }

        [Localizable(false)]
        public static void LogError(string message, params object[] args)
        {
            Debug.WriteLine(message, args);
            LogManager.Error(message, args);
        }

        [Localizable(false)]
        public static void Trace(string message, params object[] args)
        {
            Debug.WriteLine(message, args);
            LogManager.Trace(message, args);
        }
    }
}