namespace AnalysisUK.Tinamous.Media.Domain.Documents
{
    public enum MediaItemType
    {
        /// <summary>
        /// Timeline profile image
        /// </summary>
        TinyProfileImage,

        SmallProfileImage,

        MediumProfileImage,

        LargeProfileImage,

        /// <summary>
        /// Tiny version of the original image
        /// </summary>
        TinyImage,

        /// <summary>
        /// Small version of the original image. Useful for mobile timeline
        /// </summary>
        SmallImage,

        /// <summary>
        /// Medium version of the original image. Useful for standard timeline
        /// </summary>
        MediumImage,

        /// <summary>
        /// Large version of the original image. Useful for view image page.
        /// </summary>
        LargeImage,


        /// <summary>
        /// Where the original item wasn't a picture, a thumbnail representation of the original item
        /// </summary>
        SmallThumbnailImage,

        /// <summary>
        /// Where the original item wasn't a picture, a thumbnail representation of the original item
        /// </summary>
        MediumThumbnailImage,

        /// <summary>
        /// Where the original item wasn't a picture, a thumbnail representation of the original item
        /// </summary>
        LargeThumbnailImage,

        /// <summary>
        /// The original item as uploaded by the user.
        /// </summary>
        OriginalItem
    }
}