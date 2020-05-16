using System;
using System.Collections.Generic;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Events
{
    /// <summary>
    /// Indicates that the media item has been processed (scaled/transformed) and 
    /// is available as the set up size/locations
    /// </summary>
    public class MediaItemProcessedEvent
    {
        /// <summary>
        /// Media item id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Versions of the media (i.e. resized)
        /// </summary>
        public List<MediaItemStorageLocationDto> StorageLocations { get; set; }
    }
}