using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public class MediaRepository : IMediaRepository
    {
        private readonly string _tablePrefix;
        private readonly IAmazonDynamoDB _client;
        private const string TableName = "MediaItem";

        public MediaRepository(AwsDynamoDbConfig config, IAwsClientFactory clientFactory)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (clientFactory == null) throw new ArgumentNullException("clientFactory");

            _tablePrefix = config.TablePrefix;
            _client = clientFactory.CreateDynamoDBClient();
        }

        public async Task SaveAsync(MediaItem item)
        {
            try
            {
                var config = new DynamoDBContextConfig { TableNamePrefix = _tablePrefix, SkipVersionCheck = true };

                var context = new DynamoDBContext(_client, config);

                await context.SaveAsync(item);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Exception writing media item to DynamoDB media item repository");
                throw;
            }
        }

        public async Task<MediaItem> LoadAsync(Guid id)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var config = new DynamoDBContextConfig { TableNamePrefix = _tablePrefix };
                var context = new DynamoDBContext(_client, config);

                return await context.LoadAsync<MediaItem>(id);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Exception loading media item by id");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("MediaItem LoadAsync took: {0}ms", stopwatch.ElapsedMilliseconds);
            }
        }

        public async Task<List<MediaItem>> LoadByUserAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var config = new DynamoDBContextConfig { TableNamePrefix = _tablePrefix };
                var context = new DynamoDBContext(_client, config);

                //QueryFilter filter = new QueryFilter();
                //filter.AddCondition("UserId", QueryOperator.Equal, userId);

                var operationConfig = new DynamoDBOperationConfig
                {
                    IndexName = "UserId-DateAdded-index",
                    TableNamePrefix = _tablePrefix,
                    ConditionalOperator = ConditionalOperatorValues.And,
                    OverrideTableName = TableName,
                    BackwardQuery = true,
                };

                var hashKeyValue = userId;
                var operatorType = QueryOperator.Between;
                IEnumerable<object> values = new List<object> { startDate, endDate };
                AsyncSearch<MediaItem> asyncSearch = context.QueryAsync<MediaItem>(hashKeyValue, operatorType, values, operationConfig);

                List<MediaItem> items = await asyncSearch.GetNextSetAsync();

                Logger.LogMessage("LoadByUserAsync loaded {0} items", items.Count);

                return items;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Exception loading media items (LoadByUserAsync)");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("LoadByUserAsync took: {0} ms", stopwatch.ElapsedMilliseconds);
            }
        }

        public async Task<List<MediaItem>> LoadByUniqueNameAsync(Guid accountId, string uniqueName, bool decending)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var config = new DynamoDBContextConfig { TableNamePrefix = _tablePrefix };
                var context = new DynamoDBContext(_client, config);

                var operationConfig = new DynamoDBOperationConfig
                {
                    IndexName = "UniqueMediaKey-DateAdded-index",
                    TableNamePrefix = _tablePrefix,
                    ConditionalOperator = ConditionalOperatorValues.And,
                    OverrideTableName = TableName,
                    BackwardQuery = decending
                };

                // Pagination doesn't work to well at present (or at-least not start/limit type for Dynamo).
                // Pagination needs to use datetime range.

                var hashKeyValue = string.Format("{0}-{1}", accountId, uniqueName.ToLower());
                var operatorType = QueryOperator.Between;
                IEnumerable<object> values = new List<object>
                {
                    DateTime.MinValue,
                    DateTime.UtcNow,
                };

                AsyncSearch<MediaItem> asyncSearch = context.QueryAsync<MediaItem>(hashKeyValue, operatorType, values, operationConfig);

                List<MediaItem> items = await asyncSearch.GetNextSetAsync();

                // Assume one batch of query items is enough.
                // really needs to be paginated on date range.
                return items.ToList();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Exception loading media items");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("LoadByUniqueNameAsync took: {0}ms", stopwatch.ElapsedMilliseconds);
            }
        }

        public async Task DeleteAsync(MediaItem item)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var config = new DynamoDBContextConfig { TableNamePrefix = _tablePrefix };
                var context = new DynamoDBContext(_client, config);

                await context.DeleteAsync<MediaItem>(item.Id);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Exception deleting MediaItem by id: " + item.Id);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("Delete MediaItem took: {0}ms for {1}", stopwatch.ElapsedMilliseconds, item.Id);
            }
        }
    }
}