namespace SolarDigest.Models
{
    public sealed class Pagination
    {
        public int Limit { get; }
        public string StartCursor { get; }

        public Pagination(int limit, string startCursor)
        {
            Limit = limit;
            StartCursor = startCursor;
        }
    }
}