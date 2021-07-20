namespace SolarDigest.Shared
{
    public static class Constants
    {
        public const string AppName = "SolarDigest";
        public const int ServiceVersion = 2;
        public const int DataVersion = 2;

        public static class Table
        {
            public const string Site = "Site";
            public const string Exception = "Exception";
            public const string Power = "Power";
            public const string PowerMonthly = "PowerMonthly";
            public const string PowerYearly = "PowerYearly";
            public const string PowerUpdateHistory = "PowerUpdateHistory";
        }

        public static class Users
        {
            public const string BucketDownloadUser = "BucketDownloadUser";
            public const string BucketUploadUser = "BucketUploadUser";
        }

        public static class S3Buckets
        {
            public const string UploadsBucketName = "uploads";
            public const string DownloadsBucketName = "downloads";
        }
    }
}