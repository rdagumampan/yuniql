namespace Yuniql.Extensibility.SqlBatchParser
{
    public class SqlBatchLineAnalyzerResult
    {
        public bool IsMatched { get; set; }

        public int LineNumber { get; set; }

        public int Start { get; set; }

        public int End { get; set; }
    }
}
