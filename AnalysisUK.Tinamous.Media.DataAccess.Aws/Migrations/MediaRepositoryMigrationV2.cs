using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AnalysisUK.Tinamous.Media.Domain.Configuration;

namespace AnalysisUK.Tinamous.Media.DataAccess.Aws.Migrations
{
    public class MediaRepositoryMigrationV2
    {
        private readonly string _tablePrefix;
        private readonly IAmazonDynamoDB _client;

        public MediaRepositoryMigrationV2(AwsDynamoDbConfig config, IAwsClientFactory clientFactory)
        {
            if (config == null) throw new ArgumentNullException("config");

            _tablePrefix = config.TablePrefix;
            _client = clientFactory.CreateDynamoDBClient();
        }

        public async Task AddSecondaryIndexAsync()
        {
            var attributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition {AttributeName = "Id", AttributeType = ScalarAttributeType.S},
                new AttributeDefinition {AttributeName = "UserId", AttributeType = ScalarAttributeType.S},
                new AttributeDefinition {AttributeName = "AccountId", AttributeType = ScalarAttributeType.S},
                new AttributeDefinition {AttributeName = "UniqueMediaName", AttributeType = ScalarAttributeType.S},
                new AttributeDefinition {AttributeName = "UniqueMediaKey", AttributeType = ScalarAttributeType.S},
                new AttributeDefinition {AttributeName = "DateAdded", AttributeType = ScalarAttributeType.S},
            };
           

            var request = new CreateGlobalSecondaryIndexAction
            { 
                IndexName = "UniqueMediaKey-DateAdded-index",
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement {AttributeName = "UniqueMediaKey", KeyType = KeyType.HASH},
                    new KeySchemaElement {AttributeName = "DateAdded", KeyType = KeyType.RANGE}
                },
                Projection = new Projection
                {
                    ProjectionType = ProjectionType.ALL
                },
                ProvisionedThroughput = new ProvisionedThroughput(1, 1),

            };

            UpdateTableRequest updateTableRequest = new UpdateTableRequest
            {
                AttributeDefinitions = attributeDefinitions,
                GlobalSecondaryIndexUpdates = new List<GlobalSecondaryIndexUpdate>
                {
                    new GlobalSecondaryIndexUpdate
                    {
                        Create = request
                    }
                },
                TableName = _tablePrefix + "MediaItem",
            };

            var response = await _client.UpdateTableAsync(updateTableRequest);
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                Trace.WriteLine("Not updated!!");
            }
        }
    }
}