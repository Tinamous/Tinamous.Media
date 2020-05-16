using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Repositories;
using AnalysisUK.Tinamous.Media.Domain.Documents;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Media.Messaging.Events;
using AutoMapper;
using EasyNetQ;
using MediaItemType = AnalysisUK.Tinamous.Media.Messaging.Dtos.MediaItemType;

namespace AnalysisUK.Tinamous.Media.BL.Processors
{
    /// <summary>
    /// Process the media items requested
    /// 
    /// NB: This may be run as a service by it's self
    /// </summary>
    public class ProcessMediaItemRequestEventProcessor : IDisposable
    {
        private readonly IBus _bus;
        private readonly IFileStore _fileStore;
        private ISubscriptionResult _consumer;
        private readonly string _imageBucket;

        /// <summary>
        /// Just need S3 interface to load/save the item
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="fileStore"></param>
        /// <param name="processedImagesBucket"></param>
        public ProcessMediaItemRequestEventProcessor(IBus bus, 
            IFileStore fileStore, 
            string processedImagesBucket)
        {
            _bus = bus;
            _fileStore = fileStore;

            // Using a dedicated processed image bucket allows
            // the item 
            _imageBucket = processedImagesBucket;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            _consumer = _bus.SubscribeAsync<ProcessMediaItemRequestEvent>("Media", OnRequestAsync);
        }

        public async Task OnRequestAsync(ProcessMediaItemRequestEvent request)
        {
            Logger.LogMessage("Process Media Item Request. Id: {0}", request.Id);

            Stopwatch stopWatch = Stopwatch.StartNew();
            try
            {
                List<MediaItemStorageLocationDto> storageLocations = new List<MediaItemStorageLocationDto>();

                // If the content type is an image then create a scaled version for each of the 
                // image type options
                if (IsImage(request.StorageLocation.ContentType))
                {
                    // Load the image at the original storage location
                    Image image = await LoadImage(request.StorageLocation);
                    Logger.LogMessage("Got image. Width: {0}, Height: {1}", image.Width, image.Height);

                    IList<MediaItemType> mediaItemTypes = GetImageMediaItems();

                    foreach (var mediaItemType in mediaItemTypes)
                    {
                        var itemStorageLocation = await CreateMediaItem(request, mediaItemType, image);
                        storageLocations.Add(itemStorageLocation);
                    }
                }
                else
                {
                    // TODO: For non-image media item just move them 
                    // to the appropriate bucket/folder
                    Logger.LogWarn("Media item was not an image. Unable to process.");
                }

                await PublishMediaItemProcessedAsync(request.Id, storageLocations);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to convert image: " + request.Id);
                throw;
            }
            finally
            {
                stopWatch.Stop();
                Logger.LogMessage("Image processing for {0} took {1}ms", request.Id, stopWatch.ElapsedMilliseconds);
            }
        }

        private async Task<Image> LoadImage(MediaItemStorageLocationDto storageLocationDto)
        {
            Logger.LogMessage("Loading media data from {0}", storageLocationDto);

            var storageLocation = Mapper.Map<MediaItemStorageLocation>(storageLocationDto);
            using (Stream stream = await _fileStore.LoadStreamAsync(storageLocation))
            {
                return Image.FromStream(stream);
            }
        }

        /// <summary>
        /// Create a scaled image based on mediaItemType
        /// </summary>
        /// <param name="request"></param>
        /// <param name="mediaItemType"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        private async Task<MediaItemStorageLocationDto> CreateMediaItem(ProcessMediaItemRequestEvent request,
            MediaItemType mediaItemType, 
            Image image)
        {
            using (Image scaled = ScaleImage(mediaItemType, image))
            {
                return await SaveImage(request, mediaItemType, scaled);
            }
        }

        private Image ScaleImage(MediaItemType mediaItemType, Image image)
        {
            if (image == null) throw new ArgumentNullException("image");

            if (mediaItemType == MediaItemType.OriginalItem)
            {
                return image;
            }

            Size newSize = GetSize(mediaItemType, image.Width, image.Height);
            return new Bitmap(image, newSize);
        }

        private Size GetSize(MediaItemType mediaItemType, int width, int height)
        {
            double aspectRatio = height / (double) width;

            switch (mediaItemType)
            {
                case MediaItemType.TinyProfileImage:
                    return GetSize(75, aspectRatio);
                case MediaItemType.SmallProfileImage:
                    return GetSize(200, aspectRatio);
                case MediaItemType.MediumProfileImage:
                    return GetSize(500, aspectRatio);
                case MediaItemType.LargeProfileImage:
                    return GetSize(1200, aspectRatio);
                case MediaItemType.TinyImage:
                    return GetSize(75, aspectRatio);
                case MediaItemType.SmallImage:
                    return GetSize(200, aspectRatio);
                case MediaItemType.MediumImage:
                    return GetSize(500, aspectRatio);
                case MediaItemType.LargeImage:
                    return GetSize(1200, aspectRatio);
                case MediaItemType.OriginalItem:
                    return new Size(width, height);
                default:
                    return GetSize(500, aspectRatio);
            }
        }

        private Size GetSize(int width, double aspectRatio)
        {
            return new Size(width, (int)(width * aspectRatio));
        }

        private IList<MediaItemType> GetImageMediaItems()
        {
            return new List<MediaItemType>
            {
                MediaItemType.TinyImage,
                MediaItemType.SmallImage,
                MediaItemType.MediumImage,
                MediaItemType.LargeImage,
                MediaItemType.OriginalItem
            };
        }

        private bool IsImage(string contentType)
        {
            if (contentType.ToLower().Contains("image/"))
            {
                return true;
            }
            Logger.LogWarn("Unexpected content type: {0}", contentType);
            return false;
        }

        /// <summary>
        /// Save the image as a png in a public bucket.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="mediaItemType"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        private async Task<MediaItemStorageLocationDto> SaveImage(ProcessMediaItemRequestEvent request,
            MediaItemType mediaItemType, 
            Image image)
        {
            Guid id = request.Id;

            // Default encoding as png for everything 
            // except the original image.
            ImageFormat format = ImageFormat.Jpeg; // ImageFormat.Png; // TODO: Use gif if original was gif.
            

            // Try to preserve the original content type.
            if (mediaItemType == MediaItemType.OriginalItem || request.PreserveImageFormat)
            {
                format = GetFormat(request.StorageLocation.ContentType);
            }
            
            DateTime date = DateTime.UtcNow;
            string contentType = GetContentType(format);

            string imageName = string.Format("{0}/{1}/{2}/{3}/{4}.{5}",
                date.Year,
                date.Month,
                date.Day,
                mediaItemType,
                id,
                format);

            using (MemoryStream stream = new MemoryStream())
            {
                // Save to png...
                // unless the original was gif to preserve animated gifs?
                image.Save(stream, format);
                stream.Position = 0;

                var mediaItemStorageLocation = new MediaItemStorageLocation
                {
                    Bucket = _imageBucket,
                    ContentType = contentType,
                    ItemType = mediaItemType.ToString(),
                    ContentLength = (int)stream.Length,
                    Filename = imageName,
                };

                await _fileStore.SaveAsync(mediaItemStorageLocation, stream);

                return Mapper.Map<MediaItemStorageLocationDto>(mediaItemStorageLocation);
            }
        }

        private ImageFormat GetFormat(string contentType)
        {
            switch (contentType)
            {
                case "image/png":
                    return ImageFormat.Png;
                case "image/bmp":
                    return ImageFormat.Bmp;
                case "image/gif":
                    return ImageFormat.Gif;
                case "image/jpeg":
                    return ImageFormat.Jpeg;
                default:
                    return ImageFormat.Png;

            }
        }

        private string GetContentType(ImageFormat format)
        {
            if (Equals(format, ImageFormat.Png))
            {
                return "image/png";
            }

            if (Equals(format, ImageFormat.Bmp))
            {
                return "image/bmp";
            }

            if (Equals(format, ImageFormat.Gif))
            {
                return "image/gif";
            }

            if (Equals(format, ImageFormat.Jpeg))
            {
                return "image/jpeg";
            }

            return "image/png";
        }

        /// <summary>
        /// Publish an event to notify media service that the image processing
        /// has been completed.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="storageLocations"></param>
        /// <returns></returns>
        private async Task PublishMediaItemProcessedAsync(Guid id, List<MediaItemStorageLocationDto> storageLocations)
        {
            MediaItemProcessedEvent processedEvent = new MediaItemProcessedEvent
            {
                Id = id,
                StorageLocations = storageLocations,
            };
            await _bus.PublishAsync(processedEvent);
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