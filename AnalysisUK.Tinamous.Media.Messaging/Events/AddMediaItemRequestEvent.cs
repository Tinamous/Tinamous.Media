using System;
using System.Collections.Generic;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Events
{
    /// <summary>
    /// New media item uploaded. Event to request addition to the system.
    /// </summary>
    public class AddMediaItemRequestEvent
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        public UserSummaryDto User { get; set; }

        /// <summary>
        /// A caption to go with the item. This will be used as status post text if
        /// the CreateStatusPost option is set to true
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Option description to go with the photo
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A Unique name that is associated with media items. 
        /// When media is requested using the UniqueMediaName it can show the latest
        /// or a stream of the media associated with the name.
        /// </summary>
        /// <remarks>
        /// For example, use the UniqueMesuaName of "CoffeeMachine" then upload
        /// a series of time separated photos with that name and it will
        /// show the latest image for the coffee machine (e.g. allowing you to see
        /// if theirs coffee in it).
        /// </remarks>
        public string UniqueMediaName { get; set; }

        /// <summary>
        /// Location this photo was taken (or of)
        /// </summary>
        public LocationDto Location { get; set; }

        /// <summary>
        /// Tags to associate with the photo
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Item location (bucket and filename)
        /// </summary>
        public MediaItemStorageLocationDto StorageLocation { get; set; }

        /// <summary>
        /// Time to live. Media gets deleted after this time.
        /// </summary>
        public int? TTL { get; set; }
    }
}