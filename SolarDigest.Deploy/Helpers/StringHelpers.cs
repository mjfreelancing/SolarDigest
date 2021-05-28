using System.Text;

namespace SolarDigest.Deploy.Helpers
{
    internal static class StringHelpers
    {
        public static string AppendAsLines(params string[] lines)
        {
            var builder = new StringBuilder();

            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }

            return builder.ToString();
        }
    }
}