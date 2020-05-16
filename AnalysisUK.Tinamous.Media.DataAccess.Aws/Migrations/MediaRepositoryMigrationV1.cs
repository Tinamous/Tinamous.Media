using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AnalysisUK.Tinamous.Media.Domain.Configuration;
using AnalysisUK.Tinamous.Media.Domain.Logging;

namespace AnalysisUK.Tinamous.Media.DataAccess.Aws.Migrations
{
    /// <summary>
    /// V1 media repository migration. 
    /// </summary>
    public class MediaRepositoryMigrationV1
    {
        private readonly string _tablePrefix;
        private readonly IAmazonDynamoDB _client;

        public MediaRepositoryMigrationV1(AwsDynamoDbConfig config, IAwsClientFactory clientFactory)
        {
            if (config == null) throw new ArgumentNullException("config");

            _tablePrefix = config.TablePrefix;
            _client = clientFactory.CreateDynamoDBClient();
        }

        // One or more parameter values were invalid: 
        // Some index key attributes are not defined in AttributeDefinitions. Keys: [UserId], 
        //AttributeDefinitions: [AccountId, Id, UniqueMediaName]
        public async Task CreateMediaItemTable(string tableName = "MediaItem")
        {
            Table dbTable;
            if (Table.TryLoadTable(_client, _tablePrefix + tableName, DynamoDBEntryConversion.V2, out dbTable))
            {
                return;
            }
            //if (await MigrationHelper.TableExists(tableName))
            //{
            //    Logger.LogMessage("Table {0} already exists, skipping", tableName);
            //    return;
            //}

            var attributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition {AttributeName = "Id", AttributeType = ScalarAttributeType.S},
                new AttributeDefinition {AttributeName = "UserId", AttributeType = ScalarAttributeType.S},
                new AttributeDefinition {AttributeName = "AccountId", AttributeType = ScalarAttributeType.S},
                new AttributeDefinition {AttributeName = "UniqueMediaName", AttributeType = ScalarAttributeType.S}
            };

            var keySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("Id", KeyType.HASH),
            };

            var globalSecondaryIndexs = new List<GlobalSecondaryIndex>
            {
                new GlobalSecondaryIndex
                {
                    IndexName = "AccountId-UniqueMediaName-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement {AttributeName = "AccountId", KeyType = KeyType.HASH},
                        new KeySchemaElement {AttributeName = "UniqueMediaName", KeyType = KeyType.RANGE}
                    },
                    ProvisionedThroughput = new ProvisionedThroughput(1, 2),
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    }
                },
                new GlobalSecondaryIndex
                {
                    IndexName = "UserId-index",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement {AttributeName = "UserId", KeyType = KeyType.HASH},
                    },
                    ProvisionedThroughput = new ProvisionedThroughput(1, 2),
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    }
                },
            };

            var request = new CreateTableRequest
            {
                TableName = _tablePrefix + tableName,
                AttributeDefinitions = attributeDefinitions,
                KeySchema = keySchema,
                GlobalSecondaryIndexes = globalSecondaryIndexs,
                ProvisionedThroughput = new ProvisionedThroughput(1, 2),

            };

            Logger.LogWarn("Creating Table: {0} ", tableName);
            var response = await _client.CreateTableAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                Trace.WriteLine("Not created!!");
            }
        }
    }
}