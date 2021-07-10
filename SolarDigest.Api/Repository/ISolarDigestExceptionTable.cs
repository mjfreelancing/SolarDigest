using System;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Api.Repository
{
    public interface ISolarDigestExceptionTable
    {
        Task AddExceptionAsync(Exception exception, CancellationToken cancellationToken = default);
    }
}