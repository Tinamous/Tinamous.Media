using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;

namespace AnalysisUK.Tinamous.Media.Domain.Documents
{
    public class MediaItem
    {
        private DateTime _dateAdded = DateTime.UtcNow;
        private DateTime _lastUpdated = DateTime.UtcNow;
        private List<MediaItemStorageLocation> _storageLocations = new List<MediaItemStorageLocation>();

        [DynamoDBHashKey]
        public Guid Id { get; set; }

        public bool Deleted { get; set; }

        public bool Private { get; set; }

        [DynamoDBVersion]
        public int? Version { get; set; }

        [DynamoDBGlobalSecondaryIndexRangeKey("UniqueMediaKey-DateAdded-index", "UserId-DateAdded-index", "HistoryType-DateAdded-index")]
        public DateTime DateAdded
        {
            get { return _dateAdded; }
            set { _dateAdded = value; }
        }

        public DateTime LastUpdated
        {
            get { return _lastUpdated; }
            set { _lastUpdated = value; }
        }

        [DynamoDBGlobalSecondaryIndexHashKey("UserId-DateAdded-index")]
        public Guid UserId { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey("AccountId-UniqueMediaName-index")]
        public Guid AccountId { get; set; }

        public string Caption { get; set; }

        public string Description { get; set; }

        [DynamoDBGlobalSecondaryIndexRangeKey(new[] { "AccountId-UniqueMediaName-index" })]
        public string UniqueMediaName { get; set; }

        /// <summary>
        /// Unique hash for account-unique name/date query
        /// </summary>
        [DynamoDBGlobalSecondaryIndexHashKey("UniqueMediaKey-DateAdded-index")]
        public string UniqueMediaKey { get; set; }

        public string ContentType { get; set; }

        ///// <summary>
        ///// Location Filename (bucket + filename).
        ///// </summary>
        //[Obsolete("Use StorageLocations with MediaItemType OriginalStorageLocation")]
        //public MediaItemStorageLocation OriginalStorageLocation { get; set; }

        public LocationDetails Location { get; set; }

        public List<string> Tags { get; set; }

        /// <summary>
        /// Versions of the media (i.e. resized)
        /// </summary>
        public List<MediaItemStorageLocation> StorageLocations
        {
            get { return _storageLocations; }
            set { _storageLocations = value; }
        }

        /// <summary>
        /// The date (unix seconds) when this record will expire.
        ///
        /// For the latest document this will be null.
        /// For historical documents this will be x days in the future.
        ///
        /// Need to implement handling of DynamoDB item deletion before
        /// implementing this. Needs a Lambda function to delete the associated
        /// S3 objects.
        /// </summary>
        public long? DeleteAfter { get; set; }

        /// <summary>
        /// StringStatus type (Current/History) to identify the
        /// status entry type. Also used as the hash value
        /// when indexing by NotReportingAfter to allow
        /// for a know hash value
        /// </summary>
        [DynamoDBGlobalSecondaryIndexHashKey("HistoryType-DateAdded-index")]
        //[DynamoDBHashKey]
        public MediaHistoryType HistoryType { get; set; }
    }
}