using System;
using System.Collections.Generic;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Events
{
    public class MediaItemTaggedEvent
    {
        public MediaItemDto Item { get; set; }
        public DateTime Date { get; set; }

        public List<string> TagsAdded { get; set; }
    }
}