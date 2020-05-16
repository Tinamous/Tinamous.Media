using System;
using System.Collections.Generic;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Requests
{
    /// <summary>
    /// Publish request only, no response.
    /// </summary>
    public class TagMediaItemRequest
    {
        /// <summary>
        /// Tags to add
        /// </summary>
        public List<string> AddTags { get; set; }

        /// <summary>
        /// Tags to remove
        /// </summary>
        public List<string> RemoveTags { get; set; }

        public Guid Id { get; set; }

        public UserSummaryDto User { get; set; }
    }
}