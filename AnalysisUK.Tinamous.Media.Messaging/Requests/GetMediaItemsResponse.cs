using System.Collections.Generic;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Requests
{
    public class GetMediaItemsResponse
    {
        public List<MediaItemDto> MediaItems { get; set; }
    }
}