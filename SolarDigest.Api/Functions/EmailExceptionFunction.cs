using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.DynamoDBEvents;
using Newtonsoft.Json;
using SolarDigest.Api.Events;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    //public sealed class EmailExceptionFunction : FunctionBase<ExceptionPayload, bool>
    //{
    //    protected override async Task<bool> InvokeHandlerAsync(FunctionContext<ExceptionPayload> context)
    //    {
    //        var message = context.Payload.Detail.Message;

    //        context.Logger.LogDebug($"Error: {message}");

    //        using (var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.APSoutheast2))
    //        {
    //            var sendRequest = new SendEmailRequest
    //            {
    //                Source = "malcolm@mjfreelancing.com",
    //                Destination = new Destination
    //                {
    //                    ToAddresses = new List<string> { "malcolm@mjfreelancing.com" }
    //                },
    //                Message = new Message
    //                {
    //                    Subject = new Content("Exception in AWS"),
    //                    Body = new Body
    //                    {
    //                        Html = new Content
    //                        {
    //                            Charset = "UTF-8",
    //                            Data = message
    //                        },
    //                        Text = new Content
    //                        {
    //                            Charset = "UTF-8",
    //                            Data = message
    //                        }
    //                    }
    //                }
    //            };

    //            try
    //            {
    //                context.Logger.LogDebug("Sending email using Amazon SES...");

    //                var response = await client.SendEmailAsync(sendRequest);

    //                context.Logger.LogDebug($"Email status response: {response.HttpStatusCode}");
    //            }
    //            catch (Exception exception)
    //            {
    //                context.Logger.LogDebug($"Failed to send the email: {exception.Message}");
    //            }
    //        }

    //        return true;
    //    }
    //}

    public sealed class EmailExceptionFunction : FunctionBase<DynamoDBEvent, bool>
    {
        protected override Task<bool> InvokeHandlerAsync(FunctionContext<DynamoDBEvent> context)
        {
            var records = context.Payload.Records;

            foreach (var item in records)
            {
                var dbRecord = item.Dynamodb;

                var json = JsonConvert.SerializeObject(dbRecord);
                context.Logger.LogDebug($"Error record: {json}");

                if (dbRecord.StreamViewType == StreamViewType.NEW_IMAGE)
                {
                    var newImage = dbRecord.NewImage;
                    var doc = Document.FromAttributeMap(newImage);
                    var asJson = doc.ToJson();

                    context.Logger.LogDebug($"doc as JSON: {asJson}");

                    var exceptionEvent = JsonConvert.DeserializeObject<ExceptionEvent>(asJson);

                    context.Logger.LogDebug($"Message from doc: {exceptionEvent!.Message ?? string.Empty}");
                    context.Logger.LogDebug($"Stack Trace from doc: {exceptionEvent.StackTrace ?? string.Empty}");
                    

                    // todo: email the information

                }
            }

            //using (var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.APSoutheast2))
            //{
            //    var sendRequest = new SendEmailRequest
            //    {
            //        Source = "malcolm@mjfreelancing.com",
            //        Destination = new Destination
            //        {
            //            ToAddresses = new List<string> { "malcolm@mjfreelancing.com" }
            //        },
            //        Message = new Message
            //        {
            //            Subject = new Content("Exception in AWS"),
            //            Body = new Body
            //            {
            //                Html = new Content
            //                {
            //                    Charset = "UTF-8",
            //                    Data = message
            //                },
            //                Text = new Content
            //                {
            //                    Charset = "UTF-8",
            //                    Data = message
            //                }
            //            }
            //        }
            //    };

            //    try
            //    {
            //        context.Logger.LogDebug("Sending email using Amazon SES...");

            //        var response = await client.SendEmailAsync(sendRequest);

            //        context.Logger.LogDebug($"Email status response: {response.HttpStatusCode}");
            //    }
            //    catch (Exception exception)
            //    {
            //        context.Logger.LogDebug($"Failed to send the email: {exception.Message}");
            //    }
            //}

            return Task.FromResult(true);
        }
    }

}