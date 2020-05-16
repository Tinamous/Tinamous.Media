using System;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Events
{
    /// <summary>
    /// Event raised to indicate that a media item needs processing from 
    /// it's original location to a set of scaled images/video/audio etc.
    /// </summary>
    public class ProcessMediaItemRequestEvent
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Item location (bucket and filename)
        /// </summary>
        public MediaItemStorageLocationDto StorageLocation { get; set; }

        /// <summary>
        /// If the original content type should be preserved, otherwise
        /// images are re-rendered as pngs
        /// </summary>
        public bool PreserveImageFormat { get; set; }
    }
}