using System;
using System.Threading.Tasks;

namespace SolarDigest.Api
{
    public interface IExceptionHandler
    {
        Task HandleAsync(Exception exception);
    }
}