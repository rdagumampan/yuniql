namespace Yuniql.Extensibility.SqlBatchParser
{
    public class SqlStatement
    {
        public int BatchNo { get; set; }

        public string BatchText { get; set; }

        public int Length { get; set; }

        public int Start { get; set; }

        public int End { get; set; }
    }
}
