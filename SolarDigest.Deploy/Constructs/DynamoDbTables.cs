using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;

namespace SolarDigest.Deploy.Constructs
{
    internal class DynamoDbTables : Construct
    {
        internal ITable ExceptionTable { get; }
        internal ITable SiteTable { get; }
        internal ITable EnergyCostsTable { get; }
        internal ITable PowerTable { get; }
        internal ITable PowerMonthlyTable { get; }
        internal ITable PowerYearlyTable { get; }
        internal ITable PowerUpdateHistoryTable { get; }

        public DynamoDbTables(Construct scope)
            : base(scope, "DynamoDB")
        {
            ExceptionTable = CreateTable("Exception", false, StreamViewType.NEW_IMAGE);
            SiteTable = CreateTable("Site");
            EnergyCostsTable = CreateTable("EnergyCosts");
            PowerTable = CreateTable("Power", true);
            PowerMonthlyTable = CreateTable("PowerMonthly", true);
            PowerYearlyTable = CreateTable("PowerYearly", true);
            PowerUpdateHistoryTable = CreateTable("PowerUpdateHistory");
        }

        private ITable CreateTable(string tableName, bool hasSortKey = false, StreamViewType? streamViewType = default)
        {
            return new Table(this, tableName, new TableProps
            {
                TableName = tableName,
                PartitionKey = new Attribute { Name = "Id", Type = AttributeType.STRING },
                SortKey = hasSortKey ? new Attribute { Name = "Sort", Type = AttributeType.STRING } : default,
                Stream = streamViewType
            });
        }
    }
}