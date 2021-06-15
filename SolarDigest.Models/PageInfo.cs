namespace SolarDigest.Models
{
    public sealed class PageInfo
    {
        public string PreviousPageCursor { get; }
        public string NextPageCursor { get; }

        public PageInfo()
        {
        }

        // based on the page size (limit) of the current query
        public PageInfo(string previousPageCursor, string nextPageCursor)
        {
            PreviousPageCursor = previousPageCursor;
            NextPageCursor = nextPageCursor;
        }
    }
}