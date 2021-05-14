using AllOverIt.Extensions;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Newtonsoft.Json;
using SolarDigest.Api.Events;
using SolarDigest.Api.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class EmailException : FunctionBase<DynamoDBEvent, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<DynamoDBEvent> context)
        {
            var logger = context.Logger;
            var records = context.Payload.Records;

            foreach (var item in records)
            {
                var dbRecord = item.Dynamodb;

                if (dbRecord.StreamViewType == StreamViewType.NEW_IMAGE)
                {
                    var newImage = dbRecord.NewImage;
                    var imageJson = Document.FromAttributeMap(newImage).ToJson();

                    var exceptionEvent = JsonConvert.DeserializeObject<ExceptionEvent>(imageJson);

                    var fullMessage = exceptionEvent!.StackTrace.IsNullOrEmpty()
                        ? $"{exceptionEvent!.Message}"
                        : $"{exceptionEvent!.Message}{Environment.NewLine}{Environment.NewLine}{exceptionEvent.StackTrace}";

                    logger.LogDebug($"Exception event: {fullMessage}");

                    await SendEmail(fullMessage, logger).ConfigureAwait(false);
                }
            }

            return true;
        }

        private static async Task SendEmail(string message, IFunctionLogger logger)
        {
            using (var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.APSoutheast2))
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = "malcolm@mjfreelancing.com",
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> {"malcolm@mjfreelancing.com"}
                    },
                    Message = new Message
                    {
                        Subject = new Content("Exception in AWS"),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = message
                            },
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = message
                            }
                        }
                    }
                };

                try
                {
                    logger.LogDebug("Sending email...");

                    var response = await client.SendEmailAsync(sendRequest).ConfigureAwait(false);

                    logger.LogDebug($"Email status response: {response.HttpStatusCode}");
                }
                catch (Exception exception)
                {
                    logger.LogDebug($"Failed to send the email: {exception.Message}");
                }
            }
        }
    }

}