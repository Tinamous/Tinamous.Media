using AnalysisUK.Tinamous.Messaging.Common.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Requests
{
    /// <summary>
    /// Get the list of unique media names.
    /// </summary>
    public class GetUniqueNamesRequest
    {
        public UserSummaryDto RequestingUser { get; set; }
    }
}