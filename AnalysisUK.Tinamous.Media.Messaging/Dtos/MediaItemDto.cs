using System;
using System.Collections.Generic;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Dtos
{
    public class MediaItemDto
    {
        private List<MediaItemStorageLocationDto> _storageLocations = new List<MediaItemStorageLocationDto>();

        /// <summary>
        /// Media itme id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Owner of the media item
        /// </summary>
        public UserSummaryDto User { get; set; }

        /// <summary>
        /// If it is private for the user.
        /// </summary>
        public bool Private { get; set; }

        public bool Deleted { get; set; }

        public int Version { get; set; }

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
        /// Tags to associate with the media
        /// </summary>
        public List<string> Tags { get; set; }

        public DateTime DateAdded { get; set; }

        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Versions of the media (i.e. resized)
        /// </summary>
        public List<MediaItemStorageLocationDto> StorageLocations
        {
            get { return _storageLocations; }
            set { _storageLocations = value; }
        }
    }
}