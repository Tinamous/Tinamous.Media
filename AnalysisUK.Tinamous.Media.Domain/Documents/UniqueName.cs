using System;
using Amazon.DynamoDBv2.DataModel;

namespace AnalysisUK.Tinamous.Media.Domain.Documents
{
    [DynamoDBTable("MediaUniqueName")]
    public class UniqueName
    {
        private Guid _id = Guid.NewGuid();

        [DynamoDBHashKey]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// Unique names are per-account unique.
        /// </summary>
        [DynamoDBGlobalSecondaryIndexHashKey("AccountId-index")]
        public Guid AccountId { get; set; }

        public string Name { get; set; }

        public string LowerName { get; set; }

        public DateTime DateAdded { get; set; }

        [DynamoDBVersion]
        public int? Version { get; set; }
    }
}