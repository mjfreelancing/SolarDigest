namespace SolarDigest.Shared.Utils
{
    public static class Helpers
    {
        public static string GetAppVersionName()
        {
            return $"{Constants.AppName}V{Constants.ServiceVersion}";
        }
    }
}