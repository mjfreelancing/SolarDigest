using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Flurl.Http;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SolarDigest.Cli.Extensions;
using SolarDigest.Models;
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
    // https://insecurity.blog/2021/03/06/securing-amazon-s3-presigned-urls/

    // https://aws.amazon.com/s3/faqs/
    // The total volume of data and number of objects you can store are unlimited. Individual Amazon S3 objects can range in size from a
    // minimum of 0 bytes to a maximum of 5 terabytes. The largest object that can be uploaded in a single PUT is 5 gigabytes. For objects
    // larger than 100 megabytes, customers should consider using the Multipart Upload capability.

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
            var qualifiedFilename = @"C:\Data\GitHub\mjfreelancing\Projects\SolarDigest.zip";
            var filename = Path.GetFileName(qualifiedFilename);

            using (var inputStream = File.OpenRead(qualifiedFilename))
            {
                var chunkSize = 5 * 1024 * 1024;
                var inputLength = inputStream.Length;

                var partCount = (int)Math.Floor((double)inputLength / chunkSize);

                if (partCount * chunkSize != inputLength)
                {
                    partCount++;
                }

                var uploadMultiParts = await GetUploadMultiParts(filename, partCount);

                var responses = new List<UploadPartResponse>();
                var partNumber = 0;

                while (inputStream.Position < inputLength)
                {
                    var url = uploadMultiParts.Parts.ElementAt(partNumber).Url;
                    partNumber++;

                    var memoryStream = new MemoryStream();

                    // todo: need to check how much was read
                    var bytesRead = await inputStream.CopyToStreamAsync(memoryStream, chunkSize, 8092).ConfigureAwait(false);

                    memoryStream.Position = 0;

                    var streamContent = new StreamContent(memoryStream, chunkSize);

                    var putResponse = await url
                        .WithTimeout(TimeSpan.FromMinutes(5))
                        .PutAsync(streamContent)
                        .ConfigureAwait(false);

                    UploadPartResponse partResponse = null;


                    if (putResponse.StatusCode == (int)HttpStatusCode.OK)
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
                    await CompleteUploadMultiParts(filename, uploadMultiParts.UploadId, responses).ConfigureAwait(false);
                }
                else
                {
                    // todo: implement the abort code - similar to CompleteUploadMultiParts()
                }
            }
        }

        private async Task<UploadMultiParts> GetUploadMultiParts(string filename, int partCount)
        {
            var graphqlUrl = _configuration.GetValue<string>("GraphqlUrl");         // via user secrets / environment variables
            var apiKey = _configuration.GetValue<string>("x-api-key");              // via user secrets / environment variables

            using (var graphQLClient = new GraphQLHttpClient(graphqlUrl, new NewtonsoftJsonSerializer()))
            {
                var request = new GraphQLHttpRequest
                {
                    Query = @"
                        query UploadMultiPartUrls($input: UploadMultiPartInput!) {
                            uploadMultiPartUrls(input: $input) {
                                uploadId
                                parts {
                                      partNumber
                                      url
                                }
                            }
                        }",
                    OperationName = "UploadMultiPartUrls",
                    Variables = new
                    {
                        input = new
                        {
                            filename,
                            partCount
                        }
                    }
                };

                graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var response = await graphQLClient.SendQueryAsync<UploadMultiPartUrlsPayload>(request).ConfigureAwait(false);

                return response.Data.UploadMultiPartUrls;
            }
        }

        private async Task<bool> CompleteUploadMultiParts(string filename, string uploadId, IEnumerable<UploadPartResponse> parts)
        {
            var graphqlUrl = _configuration.GetValue<string>("GraphqlUrl");         // via user secrets / environment variables
            var apiKey = _configuration.GetValue<string>("x-api-key");              // via user secrets / environment variables

            using (var graphQLClient = new GraphQLHttpClient(graphqlUrl, new NewtonsoftJsonSerializer()))
            {
                var request = new GraphQLHttpRequest
                {
                    Query = @"
                        query UploadMultiPartComplete($input: UploadMultiPartCompleteInput!) {
                            uploadMultiPartComplete(input: $input)
                        }",
                    OperationName = "UploadMultiPartComplete",
                    Variables = new
                    {
                        input = new
                        {
                            filename,
                            uploadId,
                            eTags = parts.Select(item => new{item.PartNumber, item.ETag}).AsReadOnlyCollection()
                        }
                    }
                };

                graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var response = await graphQLClient.SendQueryAsync<UploadMultiPartCompletePayload>(request).ConfigureAwait(false);

                return response.Data.UploadMultiPartComplete;
            }
        }
    }
}