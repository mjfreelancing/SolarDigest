using System.Text.RegularExpressions;

namespace SolarDigest.Cli.Extensions
{
    internal static class StringExtensions
    {
        private static readonly Regex SplitWordsRegex = new("((?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z]))", RegexOptions.Compiled);

        public static string ToUpperSnakeCase(this string value)
        {
            return SplitWordsRegex.Replace(value, "_$1").ToUpper();
        }
    }
}