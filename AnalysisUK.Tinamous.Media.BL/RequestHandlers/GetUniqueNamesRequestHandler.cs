using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Requests;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.RequestHandlers
{
    public class GetUniqueNamesRequestHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private IDisposable _consumer;

        public GetUniqueNamesRequestHandler(IBus bus, 
            IMediaService mediaService)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _mediaService = mediaService;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            _consumer = _bus.RespondAsync<GetUniqueNamesRequest, GetUniqueNamesResponse>(GetUniqueNamesAsync);
        }

        public async Task<GetUniqueNamesResponse> GetUniqueNamesAsync(GetUniqueNamesRequest request)
        {
            Logger.LogWarn("GetUniqueNames");

            var stopwatch = Stopwatch.StartNew();
            Logger.LogMessage("GetUniqueNames : AccountId: {0}", request.RequestingUser.AccountId);

            try
            {
                var uniqueNames =  await _mediaService.GetUniqueNamesAsync(request.RequestingUser.AccountId);

                List<string> names = new List<string>();

                if (uniqueNames.Count > 0)
                {
                    names = uniqueNames.Select(x => x.Name).Distinct().ToList();
                }

                var response = new GetUniqueNamesResponse
                {
                    UniqueNames = names
                };
                return response;                
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to get unique names.");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("GetUniqueNames took: {0}", stopwatch.ElapsedMilliseconds);
            }

        }

        public void Dispose()
        {
            if (_consumer != null)
            {
                _consumer.Dispose();
                _consumer = null;
            }
        }
    }
}