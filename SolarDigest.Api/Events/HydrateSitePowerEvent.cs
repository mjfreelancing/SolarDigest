namespace SolarDigest.Api.Events
{
    // when this event is processed, the handling lambda will determine the date range to hydrate based
    // on when the site was last refreshed and what the current date/time is, less 1 hour (local for that site).
    public sealed class HydrateSitePowerEvent
    {
        public string Id { get; set; }              // Site Id

        //public string StartDate { get; set; }       // hydration start date
        //public string EndDate { get; set; }         // hydration end date
    }
}