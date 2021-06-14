namespace SolarDigest.Models
{
    public sealed class PageInfo
    {
        public string StartCursor { get; }
        public bool HasNextPage { get; }

        public PageInfo(string startCursor, bool hasNextPage)
        {
            StartCursor = startCursor;
            HasNextPage = hasNextPage;
        }
    }
}