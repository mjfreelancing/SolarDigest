namespace AllOverIt.Aws.Cdk.AppSync.Extensions
{
    internal static class StringExtensions
    {
        public static string GetGraphqlName(this string name)
        {
            return string.Concat(name[..1].ToLower(), name[1..]);
        }
    }
}