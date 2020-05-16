using System;

namespace AnalysisUK.Tinamous.Media.BL
{
    public interface IHeartbeatService : IDisposable
    {
        void Start();
        void Stop();
    }
}