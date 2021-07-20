namespace SolarDigest.Shared
{
    public static class Constants
    {
        public const string AppName = "SolarDigest";
        public const int ServiceVersion = 2;
        public const int DataVersion = 2;

        public static class Table
        {
            public const string Site = nameof(Site);
            public const string Exception = nameof(Exception);
            public const string Power = nameof(Power);
            public const string PowerMonthly = nameof(PowerMonthly);
            public const string PowerYearly = nameof(PowerYearly);
            public const string PowerUpdateHistory = nameof(PowerUpdateHistory);
        }

        public static class Users
        {
            public const string BucketDownloadUser = nameof(BucketDownloadUser);
            public const string BucketUploadUser = nameof(BucketUploadUser);
        }

        public static class S3Buckets
        {
            public const string UploadsBucketName = "uploads";
            public const string DownloadsBucketName = "downloads";
        }
    }
}