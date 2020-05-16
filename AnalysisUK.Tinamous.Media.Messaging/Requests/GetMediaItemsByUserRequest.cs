using System;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Requests
{
    public class GetMediaItemsByUserRequest
    {
        public UserSummaryDto User { get; set; }
        public UserSummaryDto RequestingUser { get; set; }
        public int Start { get; set; }
        public int Limit { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}