namespace AnalysisUK.Tinamous.Media.Domain.Documents
{
    public class MediaItemStorageLocation
    {
        public string Bucket { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public int ContentLength { get; set; }

        public string ItemType { get; set; }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Bucket, Filename);
        }
    }
}