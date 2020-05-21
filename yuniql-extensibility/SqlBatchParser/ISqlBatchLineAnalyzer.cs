namespace Yuniql.Extensibility.SqlBatchParser
{
    public interface ISqlBatchLineAnalyzer
    {
        SqlBatchLineAnalyzerResult Run(string sqlStatementLine);

        bool IsRequireWholeLineStripped { get; }
    }
}
