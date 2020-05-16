using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Requests
{
    /// <summary>
    /// Gets the latest media item published by a specific user.
    /// </summary>
    public class GetLatestMediaItemRequest
    {
        /// <summary>
        /// The user 
        /// </summary>
        public UserSummaryDto PublishedBy { get; set; }

        public MediaItemType MediaItemType { get; set; }

        public UserSummaryDto RequestingUser { get; set; }
    }
}