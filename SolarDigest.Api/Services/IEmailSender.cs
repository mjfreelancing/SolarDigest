using SolarDigest.Api.Models;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailContext emailContext);
    }
}