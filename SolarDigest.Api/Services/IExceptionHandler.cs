using System;
using System.Threading.Tasks;

namespace SolarDigest.Api.Services
{
    public interface IExceptionHandler
    {
        Task HandleAsync(Exception exception);
    }
}