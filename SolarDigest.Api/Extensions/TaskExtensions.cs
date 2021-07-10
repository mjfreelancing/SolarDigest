using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarDigest.Api.Extensions
{
    internal static class TaskExtensions
    {
        public static async Task InvokeTasksSequentially(this IEnumerable<Task> tasks)
        {
            foreach (var task in tasks)
            {
                await task.ConfigureAwait(false);
            }
        }
    }
}