using System;
using System.Collections.Generic;

namespace AnalysisUK.Tinamous.Media.Messaging.Requests
{
    /// <summary>
    /// Get the media items by ids 
    /// </summary>
    /// <remarks>
    /// Status message may have many media items associated with it.
    /// </remarks>
    public class GetMediaItemsRequest
    {
        public List<Guid> Ids { get; set; }
    }
}