using System;

namespace AnalysisUK.Tinamous.Media.Messaging.Events
{
    /// <summary>
    /// The media item with the id specified has been deleted.
    /// </summary>
    public class MediaItemDeletedEvent
    {
        public Guid id { get; set; }
        public Guid AccountId { get; set; }
    }
}