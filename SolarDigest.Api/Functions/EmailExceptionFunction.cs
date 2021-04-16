using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using SolarDigest.Api.Payloads.EventBridge;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public sealed class EmailExceptionFunction : FunctionBase<ExceptionPayload, bool>
    {
        protected override async Task<bool> InvokeHandlerAsync(FunctionContext<ExceptionPayload> context)
        {
            var message = context.Payload.Detail.Message;

            context.Logger.LogDebug($"Error: {message}");

            using (var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.APSoutheast2))
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = "malcolm@mjfreelancing.com",
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { "malcolm@mjfreelancing.com" }
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
                    },
                };
                try
                {
                    context.Logger.LogDebug("Sending email using Amazon SES...");
                    
                    var response = await client.SendEmailAsync(sendRequest);

                    context.Logger.LogDebug($"Email status response: {response.HttpStatusCode}");
                }
                catch (Exception exception)
                {
                    context.Logger.LogDebug($"Failed to send the email: {exception.Message}");
                }
            }

            return true;
        }
    }
}