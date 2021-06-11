using AllOverIt.Extensions;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SolarDigest.Api.Events;
using SolarDigest.Api.Models;
using SolarDigest.Api.Services;
using System;
using System.Linq;
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

                    // check to make sure it isn't a deleted item
                    if (newImage.Any())
                    {
                        var imageJson = Document.FromAttributeMap(newImage).ToJson();

                        var exceptionEvent = JsonConvert.DeserializeObject<ExceptionEvent>(imageJson);

                        var fullMessage = exceptionEvent!.StackTrace.IsNullOrEmpty()
                            ? $"{exceptionEvent!.Message}"
                            : $"{exceptionEvent!.Message}{Environment.NewLine}{Environment.NewLine}{exceptionEvent.StackTrace}";

                        logger.LogDebug($"Exception event: {fullMessage}");

                        var emailSender = context.ScopedServiceProvider.GetRequiredService<IEmailSender>();

                        var emailContext = new EmailContext
                        {
                            SourceEmail = "malcolm@mjfreelancing.com",
                            ToEmail = "malcolm@mjfreelancing.com",
                            Subject = "SolarDigest Exception (AWS)",
                            PlainMessage = fullMessage,
                            HtmlMessage = fullMessage
                        };

                        await emailSender.SendEmailAsync(emailContext).ConfigureAwait(false);
                    }
                    else
                    {
                        logger.LogDebug("Exception document manually deleted");
                    }
                }
            }

            return true;
        }
    }
}