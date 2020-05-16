using AnalysisUK.Tinamous.Messaging.Common.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Requests
{
    public class GetMediaItemsByUniqueNameRequest
    {
        /// <summary>
        /// Requesting user. Items are filtered on user account only
        /// </summary>
        public UserSummaryDto User { get; set; }

        /// <summary>
        /// Get the items by the unique name
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// If to get the latest item by unique name only
        /// </summary>
        public bool LatestOnly { get; set; }

        public int Start { get; set; }

        public int Limit { get; set; }
    }
}