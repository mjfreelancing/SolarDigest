using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SolarDigest.Cli.Extensions
{
    // todo: Move to AllOverIt

    public static class StreamExtensions
    {
        public static async Task<int> CopyToStreamAsync(this Stream fromStream, Stream toStream, int bytesToCopy, int bufferSize,
            CancellationToken cancellationToken = default)
        {
            var bytes = new byte[bufferSize];
            var bytesToRead = bufferSize;
            var totalRead = 0;
            int bytesRead;

            while (!cancellationToken.IsCancellationRequested &&
                   (bytesRead = await fromStream.ReadAsync(bytes, 0, bytesToRead, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await toStream.WriteAsync(bytes, 0, bytesRead, cancellationToken).ConfigureAwait(false);

                totalRead += bytesRead;

                if (totalRead + bytesToRead > bytesToCopy)
                {
                    bytesToRead = bytesToCopy - totalRead;
                }
            }

            return totalRead;
        }
    }
}