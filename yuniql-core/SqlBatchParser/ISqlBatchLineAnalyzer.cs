namespace Yuniql.Core.SqlBatchParser
{
    public interface ISqlBatchLineAnalyzer
    {
        SqlBatchLineAnalyzerResult Run(string sqlStatementLine);

        bool IsRequireWholeLineStripped { get; }
    }
}
