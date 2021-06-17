namespace SolarDigest.Api.Functions.Payloads
{
    public abstract class AppSyncPayloadBase
    {
        protected string NormaliseEnumValue(string value)
        {
            return value.Replace("_", string.Empty);
        }
    }
}