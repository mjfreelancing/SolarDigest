using AllOverIt.Helpers;
using Amazon.S3;
using Amazon.S3.Model;
using Flurl.Http;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SolarDigest.Cli.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SolarDigest.Cli.Commands.Upload
{
    // Upload references
    // https://www.altostra.com/blog/multipart-uploads-with-s3-presigned-url
    // https://docs.aws.amazon.com/sdkfornet/v3/apidocs/index.html?page=S3/MS3InitiateMultipartUploadInitiateMultipartUploadRequest.html
    // https://docs.aws.amazon.com/AmazonS3/latest/API/API_CreateMultipartUpload.html

    internal sealed class UploadFileCommand : ICommand
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UploadFileCommand> _logger;

        public UploadFileCommand(IConfiguration configuration, ILogger<UploadFileCommand> logger)
        {
            _configuration = configuration.WhenNotNull(nameof(configuration));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task Execute()
        {
            var bucketName = "solardigest-uploads";
            var qualifiedFilename = @"C:\Data\SolarDigest.zip";
            var filename = Path.GetFileName(qualifiedFilename);

            var client = new AmazonS3Client("access_key", "secret_key");

            // ------------------------------------------------------------------------------------------------

            var initRequest = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = filename
            };

            var initResponse = await client.InitiateMultipartUploadAsync(initRequest).ConfigureAwait(false);

            // ------------------------------------------------------------------------------------------------

            using (var inputStream = File.OpenRead(qualifiedFilename))
            {
                var chunkSize = 5 * 1024 * 1024;
                var inputLength = inputStream.Length;
                var partNumber = 0;

                var responses = new List<UploadPartResponse>();




                while (inputStream.Position < inputLength)
                {
                    var url = await GetPresignedUrl(filename, initResponse.UploadId, ++partNumber);



                    var memoryStream = new MemoryStream();

                    // todo: need to check how much was read
                    var bytesRead = await inputStream.CopyToStreamAsync(memoryStream, chunkSize, 8092).ConfigureAwait(false);
                    memoryStream.Position = 0;

                    var streamContent = new StreamContent(memoryStream, chunkSize);

                    var putResponse = await url.WithTimeout(TimeSpan.FromMinutes(5)).PutAsync(streamContent).ConfigureAwait(false);

                    UploadPartResponse partResponse = null;



                    if (putResponse.StatusCode == (int) HttpStatusCode.OK)
                    {
                        if (putResponse.Headers.TryGetFirst("ETag", out var etag))
                        {
                            _logger.LogDebug($"Uploaded part {partNumber} of {filename}, ETag = {etag}");

                            partResponse = new UploadPartResponse(partNumber, etag);
                            responses.Add(partResponse);
                        }
                    }
                    
                    if (partResponse == null)
                    {
                        responses.Clear();
                        break;
                    }
                }






                if (responses.Any())
                {
                    // Complete the multipart upload
                    var compRequest = new CompleteMultipartUploadRequest
                    {
                        BucketName = bucketName,
                        Key = filename,
                        UploadId = initResponse.UploadId,
                        PartETags = responses
                            .Select(item =>
                                new PartETag
                                {
                                    ETag = item.ETag,
                                    PartNumber = item.PartNumber
                                })
                            .ToList()
                    };
                    
                    var completeResponse = await client.CompleteMultipartUploadAsync(compRequest).ConfigureAwait(false);

                }
                else
                {
                    var abortRequest = new AbortMultipartUploadRequest
                    {
                        UploadId = initResponse.UploadId,
                        BucketName = bucketName,
                        Key = filename
                    };

                    /*var abortResponse =*/
                    await client.AbortMultipartUploadAsync(abortRequest).ConfigureAwait(false);
                }
            }
        }

        private async Task<string> GetPresignedUrl(string filename, string uploadId, int partNumber)
        {
            var graphqlUrl = _configuration.GetValue<string>("GraphqlUrl");         // via user secrets / environment variables
            var apiKey = _configuration.GetValue<string>("x-api-key");              // via user secrets / environment variables

            using (var graphQLClient = new GraphQLHttpClient(graphqlUrl, new NewtonsoftJsonSerializer()))
            {
                var request = new GraphQLHttpRequest
                {
                    Query = @"
                        query UploadUrl($input: UploadUrlInput!) {
                            uploadUrl(input: $input) 
                        }",
                    OperationName = "UploadUrl",
                    Variables = new
                    {
                        input = new
                        {
                            filename,
                            uploadId,
                            partNumber
                        }
                    }
                };

                try
                {
                    graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                    var response = await graphQLClient.SendQueryAsync<UploadUrlPayload>(request);

                    return response.Data.UploadUrl;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception.Message);
                }

                return string.Empty;
            }
        }
    }
}