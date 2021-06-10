using AllOverIt.Helpers;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using SolarDigest.Api.Logging;
using SolarDigest.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    internal sealed class EmailSender : IEmailSender
    {
        private readonly IFunctionLogger _logger;

        public EmailSender(IFunctionLogger logger)
        {
            _logger = logger.WhenNotNull(nameof(logger));
        }

        public async Task SendEmailAsync(EmailContext emailContext)
        {
            using (var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.APSoutheast2))
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = emailContext.SourceEmail,
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { emailContext.ToEmail }
                    },
                    Message = new Message
                    {
                        Subject = new Content(emailContext.Subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = emailContext.HtmlMessage
                            },
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = emailContext.PlainMessage
                            }
                        }
                    }
                };

                try
                {
                    _logger.LogDebug("Sending email...");

                    var response = await client.SendEmailAsync(sendRequest).ConfigureAwait(false);

                    _logger.LogDebug($"Email status response: {response.HttpStatusCode}");
                }
                catch (Exception exception)
                {
                    _logger.LogDebug($"Failed to send the email: {exception.Message}");
                }
            }
        }
    }
}