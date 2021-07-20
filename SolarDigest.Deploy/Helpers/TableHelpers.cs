namespace SolarDigest.Deploy.Helpers
{
    internal static class TableHelpers
    {
        public static string GetExportTableName(string tableName)
        {
            return $"{Shared.Helpers.GetAppVersionName()}-Data-Table-{tableName}";
        }

        public static string GetExportStreamName(string tableName)
        {
            return $"{Shared.Helpers.GetAppVersionName()}Data-Stream-{tableName}";
        }
    }
}