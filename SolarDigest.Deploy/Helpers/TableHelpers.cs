namespace SolarDigest.Deploy.Helpers
{
    internal static class TableHelpers
    {
        public static string GetExportTableName(string tableName)
        {
            return $"{Constants.AppName}Data-Table-{tableName}";
        }

        public static string GetExportStreamName(string tableName)
        {
            return $"{Constants.AppName}Data-Stream-{tableName}";
        }
    }
}