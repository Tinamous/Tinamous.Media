using System;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Events
{
    public class NewVideoEvent : INewMediaItemEvent
    {
        public Guid MediaItemId { get; set; }
        public string UniqueMediaName { get; set; }
        public DateTime Date { get; set; }

        public UserSummaryDto PublishedBy { get; set; }

        public MediaItemDto Item { get; set; }
    }
}