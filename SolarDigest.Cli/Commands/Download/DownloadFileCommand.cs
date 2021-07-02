using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Flurl.Http;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SolarDigest.Cli.Commands.Download
{
    // Command: download --file <filename> [--destination <folder>]
    // ------------------------------------------------------------
    // Where  : <filename> must exist in the S3 downloads folder
    //          [--destination <folder>] is optional. The current location will be the default destination if not provided.
    //
    internal sealed class DownloadFileCommand : ICommand
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DownloadFileCommand> _logger;

        public static string Identifier => "download";

        public DownloadFileCommand(IConfiguration configuration, ILogger<DownloadFileCommand> logger)
        {
            _configuration = configuration.WhenNotNull(nameof(configuration));
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task Execute()
        {
            var graphqlUrl = _configuration.GetValue<string>("GraphqlUrl");         // via user secrets / environment variables
            var apiKey = _configuration.GetValue<string>("x-api-key");              // via user secrets / environment variables
            var filename = _configuration.GetValue<string>("file");                 // via command line
            var destination = _configuration.GetValue<string>("destination");       // via command line

            _logger.LogInformation($"Requesting to download {filename} from {graphqlUrl}");

            using (var graphQLClient = new GraphQLHttpClient(graphqlUrl, new NewtonsoftJsonSerializer()))
            {
                var request = new GraphQLHttpRequest
                {
                    Query = @"
                        query DownloadUrl($filename: String!) {
                            downloadUrl(filename: $filename) 
                        }",
                    OperationName = "DownloadUrl",
                    Variables = new
                    {
                        filename
                    }
                };

                try
                {
                    graphQLClient.HttpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                    var response = await graphQLClient.SendQueryAsync<DownloadUrlPayload>(request);
                    var downloadUrl = response.Data.DownloadUrl;

                    _logger.LogDebug($"Pre-signed Url: {downloadUrl}");

                    var folderPath = destination.IsNullOrEmpty()
                        ? Directory.GetCurrentDirectory()
                        : destination;

                    _logger.LogDebug($"Downloading to: {folderPath}");

                    var filePath = await downloadUrl.DownloadFileAsync(folderPath).ConfigureAwait(false);

                    _logger.LogInformation($"Downloaded: {filePath}");
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception.Message);
                }
            }
        }
    }
}