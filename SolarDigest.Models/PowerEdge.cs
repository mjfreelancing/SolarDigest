using AllOverIt.Helpers;

namespace SolarDigest.Models
{
    public sealed class PowerEdge
    {
        public TimeWatts Node { get; }
        public string Cursor { get; }

        public PowerEdge(TimeWatts node, string cursor)
        {
            Node = node.WhenNotNull(nameof(node));
            Cursor = cursor.WhenNotNullOrEmpty(cursor);
        }
    }
}