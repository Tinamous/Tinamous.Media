using System;
using System.Collections.Generic;
using AnalysisUK.Tinamous.Media.BL.EventHandlers;
using AnalysisUK.Tinamous.Media.BL.Processors;
using AnalysisUK.Tinamous.Media.BL.RequestHandlers;
using AnalysisUK.Tinamous.Media.DataAccess.Aws;
using AnalysisUK.Tinamous.Media.DataAccess.Aws.Migrations;
using AnalysisUK.Tinamous.Media.DataAccess.Aws.Repositories;
using AnalysisUK.Tinamous.Media.Domain.Configuration;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL
{
    public class Initialisor : IDisposable
    {
        private IBus _bus;
        private IBus _eventProcessingBus;
        private readonly List<IDisposable> _handlers = new List<IDisposable>();
        private readonly IMembershipService _membershipService;
        private readonly IMediaService _mediaService;
        private IHeartbeatService _heartbeatService;
        private S3FileStore _fileStore;
        private AwsS3Config _s3Config;

        public Initialisor(IBus bus, IBus eventProcessingBus)
        {
            if (bus == null) throw new ArgumentNullException("bus");
            
            _bus = bus;
            _eventProcessingBus = eventProcessingBus;
            _membershipService = new MembershipService(_bus);

            _s3Config = AwsConfigFactory.GetS3Config();
            var awsClientFactory = new AwsClientFactory();

            _fileStore = new S3FileStore(awsClientFactory);
            
            var dynamoDbConfig = AwsConfigFactory.GetDynamoDbConfig();
            var mediaRepository = new MediaRepository(dynamoDbConfig, awsClientFactory);
            var uniquNameRepository = new UniqueNameRepository(dynamoDbConfig, awsClientFactory);
            _mediaService = new MediaService(_bus, mediaRepository, uniquNameRepository, _fileStore);


            _heartbeatService = new HeartbeatService(_bus, ServerSettings.ServerName);
            _heartbeatService.Start();

            AutoMapperConfiguration.Configure();
        }

        public void Initialize()
        {
            ApplyDbTransforms();
            SetupProcessors();
            SetupPersistors();
            SetupRequestHandlers();
            SetupEventWatchers();

            Logger.LogMessage("Initialize Complete");
        }

        private void ApplyDbTransforms()
        {
            try
            {
                // Ensure that the bucket exists on start-up rather than rely on 
                try
                {
                    Logger.LogMessage("Create Bucket for processed images.");
                    var bucket = _s3Config.ProcessedImagesBucket;
                    _fileStore.EnsureBucketExists(bucket);
                    Logger.LogMessage("Create Bucket - Completed.");
                }
                catch (Exception ex)
                {
                    // Failed occasionally when bucket exists.
                    // Sink
                    Logger.LogException(ex, "Failed to create bucked for processed images on startup");
                }

                var awsClientFactory = new AwsClientFactory();

                // Create DynamoDB tables...
                Logger.LogMessage("Applying DB Transformations");
                var config = AwsConfigFactory.GetDynamoDbConfig();
                MediaRepositoryMigrationV1 v1Migations = new MediaRepositoryMigrationV1(config,awsClientFactory);
                v1Migations.CreateMediaItemTable().Wait();
                Logger.LogMessage("Apply Media V1 Transforms - Completed.");

                Logger.LogMessage("Apply Unique Name V1 Transforms.");
                UniqueNameRepositoryMigrationV1 uniqueNameV1Migrations = new UniqueNameRepositoryMigrationV1(config, awsClientFactory);
                uniqueNameV1Migrations.CreateTable().Wait();
                Logger.LogMessage("Apply Unique Name V1 Transforms - Completed.");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed during ApplyDbTransforms");
            }
        }

        private void SetupProcessors()
        {
            Logger.LogMessage("Setup Processors");

            // These are all likely to be slow (i.e. S3 storage / processing).
            // so handle them on a different bus to the RPC query bus.

            // Add the already stored uploaded image and request process.
            AddHandler(new AddMediaItemRequestEventProcessor(_eventProcessingBus, _membershipService, _mediaService));

            // Create initial image from raw image data and request process.
            AddHandler(new CreateImageRequestEventProcessor(_eventProcessingBus, _mediaService, _fileStore, _s3Config.UploadedImagesBucket));
            
            AddHandler(new ProcessMediaItemRequestEventProcessor(_eventProcessingBus, _fileStore, _s3Config.ProcessedImagesBucket));

            AddHandler(new MediaItemProcessedEventProcessor(_eventProcessingBus, _mediaService, _fileStore));
        }

        private void SetupPersistors()
        {
            Logger.LogMessage("Setup Persistors");
        }

        private void SetupRequestHandlers()
        {
            Logger.LogMessage("Setup Request Handlers");

            AddHandler(new DeleteMediaItemRequestHandler(_bus, _mediaService));
            AddHandler(new GetMediaItemByIdRequestHandler(_bus, _mediaService));
            AddHandler(new GetMediaItemsByUserRequestHandler(_bus, _mediaService));
            AddHandler(new GetMediaItemsByUniqueNameRequestHandler(_bus, _mediaService));
            AddHandler(new GetMediaItemsRequestHandler(_bus, _mediaService));
            AddHandler(new GetLatestMediaItemRequestHandler(_bus, _mediaService));
            AddHandler(new GetUniqueNamesRequestHandler(_bus, _mediaService));
            AddHandler(new TagMediaItemRequestHandler(_bus, _mediaService));

            AddHandler(new PurgeOldMediaRequestEventHandler(_eventProcessingBus, _mediaService, _fileStore));
        }

        private void SetupEventWatchers()
        {
            Logger.LogMessage("Setup Event Watchers");

            AddHandler(new UserUpdatedEventHandler(_bus, _membershipService));
            AddHandler(new AccountUpdatedEventHandler(_bus, _membershipService));
            AddHandler(new NewMediaItemEventHandler(_bus, _mediaService));
            AddHandler(new DeviceDeletedEventHandler(_bus));
            AddHandler(new MemberDeletedEventHandler(_bus));
        }

        private void AddHandler(IDisposable disposable)
        {
            Logger.LogMessage("Adding handler: {0}",disposable.GetType().FullName);
            _handlers.Add(disposable);
        }

        public void Dispose()
        {
            foreach (var disposable in _handlers)
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            _handlers.Clear();

            if (_heartbeatService != null)
            {
                _heartbeatService.Stop();
                _heartbeatService.Dispose();
                _heartbeatService = null;
            }

            _bus = null;
            _eventProcessingBus = null;
        }
    }
}
