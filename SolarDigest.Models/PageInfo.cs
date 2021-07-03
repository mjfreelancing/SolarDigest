namespace SolarDigest.Models
{
    public sealed class PageInfo
    {
        public string PreviousPageCursor { get; set; }
        public string NextPageCursor { get; set; }

        // required for when there's no page info and for graphql responses
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