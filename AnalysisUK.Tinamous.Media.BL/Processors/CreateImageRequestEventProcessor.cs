using AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Repositories;
using AnalysisUK.Tinamous.Media.Domain.Documents;
using AnalysisUK.Tinamous.Media.Domain.Helpers;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Media.Messaging.Events;
using AutoMapper;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using MediaItemType = AnalysisUK.Tinamous.Media.Messaging.Dtos.MediaItemType;

namespace AnalysisUK.Tinamous.Media.BL.Processors
{
    /// <summary>
    /// First stage in the addition of media items workflow, taking raw data to create the original S3 file.
    /// 
    /// 1) AddMediaItemRequest -> Saves initial Media properties -> ProcessMediaItemRequestEvent
    /// 2) ProcessMediaItemRequestEvent -> Transforms media -> MediaItemProcessedEvent
    /// 3) MediaItemProcessedEvent -> Saves transformations -> INewMediaItemEvent (subclass based on media)
    /// </summary>
    public class CreateImageRequestEventProcessor : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private readonly IFileStore _fileStore;
        private readonly string _rawImageBucket;
        private ISubscriptionResult _consumer;

        public CreateImageRequestEventProcessor(IBus bus,
            IMediaService mediaService,
            IFileStore fileStore,
            string rawImageBucket)
        {
            _bus = bus;
            _mediaService = mediaService;
            _fileStore = fileStore;
            _rawImageBucket = rawImageBucket;
            InitializeMessaging();
        }

        // CreateImageRequestEvent
        private void InitializeMessaging()
        {
            _consumer = _bus.SubscribeAsync<CreateImageRequestEvent>("Media", OnRequest);
        }

        public async Task OnRequest(CreateImageRequestEvent request)
        {
            Logger.LogMessage("Creating image from data. Width: {0}, Height: {1}", request.Width, request.Height);

            try
            {
                request.UniqueMediaName = request.UniqueMediaName ?? "";

                MediaItemStorageLocation storageLocation = await CreateS3FileAsync(request);

                var mediaItem = new MediaItem
                {
                    Id = request.Id,
                    AccountId = request.User.AccountId,
                    UserId = request.User.UserId,
                    Caption = request.Caption,
                    //ContentType = request. .ContentType,
                    Location = Mapper.Map<LocationDetails>(request.Location),
                    Tags = request.Tags,
                    Description = request.Description,
                    //OriginalStorageLocation = request.StorageLocation
                    UniqueMediaName = request.UniqueMediaName,
                    UniqueMediaKey = string.Format("{0}-{1}", request.User.AccountId, request.UniqueMediaName.ToLower()),
                    StorageLocations = new List<MediaItemStorageLocation> { storageLocation },
                    HistoryType = MediaHistoryType.Pending,
                };

                // After status post so the status post Id can be included for reference
                await _mediaService.SaveAsync(mediaItem);

                await PublishProcessMediaItemAsync(mediaItem, storageLocation, true); //request.PreserveImageFormat);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to add new image from raw data.");
            }
        }

        private async Task<MediaItemStorageLocation> CreateS3FileAsync(CreateImageRequestEvent request)
        {
            if (request.Height == 0 || request.Width == 0 || request.Data == null || request.Data.Length == 0)
            {
                throw new Exception("Invalid image size");
            }

            if (request.Height * request.Width != request.Data.Length)
            {
                throw new Exception("Image data does not match specified size.");
            }

            int factor = 1;
            if (request.Width < 100)
            {
                factor = 10;
            }

            Bitmap bitmap = new Bitmap(request.Width * factor, request.Height * factor, System.Drawing.Imaging.PixelFormat.Format32bppArgb);


            for (int x = 0; x < request.Width; x++)
            {
                for (int y = 0; y < request.Height; y++)
                {
                    int pixel = request.Data[(y * request.Width) + x];
                    Color pixelColor = Color.FromArgb(pixel);

                    if (factor > 1)
                    {
                        FillRectangle(factor, pixel, bitmap, x * factor, y * factor);
                    }
                    else
                    {
                        bitmap.SetPixel(x, y, pixelColor);
                    }
                }
            }


            return await SaveImage(bitmap, request.Id, request.User.AccountId);
        }

        /// <summary>
        /// Create a rectangle version of a pixel.
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="pixel"></param>
        /// <param name="bitmap"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        private static void FillRectangle(int factor, int pixel, Bitmap bitmap, int xOffset, int yOffset)
        {
            for (int subX = 0; subX < factor; subX++)
            {
                for (int subY = 0; subY < factor; subY++)
                {
                    Color pixelColor = Color.FromArgb(pixel);
                    bitmap.SetPixel(xOffset + subX, yOffset + subY, pixelColor);
                }
            }
        }

        /// <summary>
        /// Save the image as a png in a public bucket.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="id"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        private async Task<MediaItemStorageLocation> SaveImage(Image image, Guid id, Guid accountId)
        {
            ImageFormat format = ImageFormat.Bmp;
            DateTime date = SystemDate.UtcNow;

            string imageName = string.Format("{0}/{1}/{2}/{3}/{4}.bmp",
                accountId,
                date.Year,
                date.Month,
                date.Day,
                id);

            using (MemoryStream stream = new MemoryStream())
            {
                // Save to png...
                // unless the original was gif to preserve animated gifs?
                image.Save(stream, format);
                stream.Position = 0;

                var mediaItemStorageLocation = new MediaItemStorageLocation
                {
                    Bucket = _rawImageBucket,
                    ContentType = "image/bmp",
                    ItemType = MediaItemType.UploadedItem.ToString(),
                    ContentLength = (int)stream.Length,
                    Filename = imageName,
                };

                await _fileStore.SaveAsync(mediaItemStorageLocation, stream);

                return mediaItemStorageLocation;
            }
        }

        /// <summary>
        /// Publish a request to convert the media at the storage location
        /// into the appropriate media items (scaled down etc)
        /// </summary>
        /// <param name="mediaItem"></param>
        /// <param name="storageLocation"></param>
        /// <returns></returns>
        private async Task PublishProcessMediaItemAsync(MediaItem mediaItem, MediaItemStorageLocation storageLocation, bool preserveImageFormat)
        {
            ProcessMediaItemRequestEvent requestEvent = new ProcessMediaItemRequestEvent
            {
                Id = mediaItem.Id,
                StorageLocation = new MediaItemStorageLocationDto
                {
                    Bucket = storageLocation.Bucket,
                    ContentLength = storageLocation.ContentLength,
                    ContentType = storageLocation.ContentType,
                    Filename = storageLocation.Filename,
                    ItemType = (MediaItemType)Enum.Parse(typeof(MediaItemType), storageLocation.ItemType),
                },
                PreserveImageFormat = preserveImageFormat
            };
            await _bus.PublishAsync(requestEvent);
        }

        public void Dispose()
        {
            if (_consumer != null)
            {
                _consumer.Dispose();
                _consumer = null;
            }
        }
    }
}