using AllOverIt.Helpers;

namespace SolarDigest.Models
{
    public sealed class PowerEdge
    {
        public TimeWatts Node { get; set; }
        public string Cursor { get; set; }

        public static PowerEdge Create(TimeWatts node, string cursor)
        {
            return new PowerEdge
            {
                Node = node.WhenNotNull(nameof(node)),
                Cursor = cursor.WhenNotNullOrEmpty(cursor)
            };
        }
    }
}