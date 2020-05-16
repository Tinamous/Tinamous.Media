namespace AnalysisUK.Tinamous.Media.Messaging.Dtos
{
    public class MediaItemStorageLocationDto
    {
        public string Bucket { get; set; }
        public string Filename { get; set; }

        public string ContentType { get; set; }
        public int ContentLength { get; set; }

        public MediaItemType ItemType { get; set; }
    }
}