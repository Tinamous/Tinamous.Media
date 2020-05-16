using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Exceptions;
using AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Repositories;
using AnalysisUK.Tinamous.Media.Domain.Documents;
using AnalysisUK.Tinamous.Media.Domain.Logging;

namespace AnalysisUK.Tinamous.Media.DataAccess.Aws.Repositories
{
    public class S3FileStore : IFileStore
    {
        private readonly IAmazonS3 _client;

        public S3FileStore(IAwsClientFactory clientFactory)
        {           
            _client = clientFactory.CreateS3Client();
        }

        public async Task SaveAsync(MediaItemStorageLocation storageLocation, Stream stream)
        {
            Logger.LogMessage("Saving media item to bucket: '{0}', path: {1}", storageLocation.Bucket, storageLocation.Filename);
            var request = new PutObjectRequest
            {
                BucketName = storageLocation.Bucket,
                ContentType = storageLocation.ContentType,
                InputStream = stream,
                Key = storageLocation.Filename
            };

            await _client.PutObjectAsync(request);
        }

        public async Task DeleteAsync(MediaItemStorageLocation storageLocation)
        {
            try
            {
                await _client.DeleteObjectAsync(storageLocation.Bucket, storageLocation.Filename);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to deleted file from S3. Bucket: " + storageLocation + ", file: " + storageLocation.Filename);
                // Just sink the exception.
            }
        }

        public async Task<Stream> LoadStreamAsync(MediaItemStorageLocation storageLocation)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = storageLocation.Bucket,
                    Key = storageLocation.Filename,
                };

                GetObjectResponse response = await _client.GetObjectAsync(request);

                return response.ResponseStream;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to load attachment from S3. Url: " + storageLocation);
                throw;
            }
        }

        public void EnsureBucketExists(string bucketName)
        {
            try
            {
                Logger.LogMessage("Creating bucket '{0}'", bucketName);

                var response = _client.PutBucket(bucketName);

                Debug.WriteLine("RequestId: " + response.ResponseMetadata.RequestId);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "error creating bucket: " + bucketName);
                throw new StoreFileException("Failed to create S3 bucket: " + bucketName, ex);
            }
        }
    }
}