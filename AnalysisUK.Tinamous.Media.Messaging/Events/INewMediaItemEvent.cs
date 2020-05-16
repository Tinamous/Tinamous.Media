using System;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Events
{
    public interface INewMediaItemEvent
    {
        /// <summary>
        /// Id of the media item that has been added.
        /// </summary>
        Guid MediaItemId { get; set; }

        string UniqueMediaName { get; set; }

        DateTime Date { get; set; }

        UserSummaryDto PublishedBy { get; set; }

        MediaItemDto Item { get; set; }
    }
}