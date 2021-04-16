namespace SolarDigest.Api.Events
{
    public sealed class HydrateSitePowerEvent
    {
        public string Id { get; set; }              // Site Id
        public string StartDate { get; set; }       // hydration start date
        public string EndDate { get; set; }         // hydration end date
    }
}