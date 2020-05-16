using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Repositories;
using AnalysisUK.Tinamous.Media.Domain.Configuration;
using AnalysisUK.Tinamous.Media.Domain.Documents;
using AnalysisUK.Tinamous.Media.Domain.Logging;

namespace AnalysisUK.Tinamous.Media.DataAccess.Aws.Repositories
{
    public class UniqueNameRepository : IUniqueNameRepository
    {
        private readonly string _tablePrefix;
        private readonly IAmazonDynamoDB _client;
        private const string TableName = "MediaUniqueName";

        public UniqueNameRepository(AwsDynamoDbConfig config, IAwsClientFactory clientFactory)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (clientFactory == null) throw new ArgumentNullException("clientFactory");

            _tablePrefix = config.TablePrefix;
            _client = clientFactory.CreateDynamoDBClient();
        }

        public async Task<List<UniqueName>> ListAsync(Guid accountId, int start, int limit)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var config = new DynamoDBContextConfig { TableNamePrefix = _tablePrefix };
                var context = new DynamoDBContext(_client, config);

                var operationConfig = new DynamoDBOperationConfig
                {
                    IndexName = "AccountId-index",
                    TableNamePrefix = _tablePrefix,
                    ConditionalOperator = ConditionalOperatorValues.And,
                    OverrideTableName = TableName,
                    BackwardQuery = true,
                };

                // TODO: Figure out how to specify start and limit

                var hashKeyValue = accountId;
                AsyncSearch<UniqueName> asyncSearch = context.QueryAsync<UniqueName>(hashKeyValue, operationConfig);

                List<UniqueName> items = await asyncSearch.GetNextSetAsync();

                return items;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Exception loading unique names: ");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("Query unique names took: {0}ms", stopwatch.ElapsedMilliseconds);
            }
        }

        public async Task InsertAsync(UniqueName item)
        {
            try
            {
                var config = new DynamoDBContextConfig { TableNamePrefix = _tablePrefix, SkipVersionCheck = true };

                var context = new DynamoDBContext(_client, config);

                await context.SaveAsync(item);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Exception inserting Unique Name to Dynamo repository: ");
                throw;
            }
        }
    }
}