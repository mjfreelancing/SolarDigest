namespace SolarDigest.Models
{
    public sealed class PageInfo
    {
        public string StartCursor { get; }
        public bool HasNextPage { get; }
        public bool HasPreviousPage { get; }

        public PageInfo(string startCursor, bool hasNextPage, bool hasPreviousPage)
        {
            StartCursor = startCursor;
            HasNextPage = hasNextPage;
            HasPreviousPage = hasPreviousPage;
        }
    }
}