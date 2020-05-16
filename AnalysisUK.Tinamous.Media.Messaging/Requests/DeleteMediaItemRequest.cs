using System;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Requests
{
    public class DeleteMediaItemRequest
    {
        public Guid Id { get; set; }
        public UserSummaryDto User { get; set; }
    }
}