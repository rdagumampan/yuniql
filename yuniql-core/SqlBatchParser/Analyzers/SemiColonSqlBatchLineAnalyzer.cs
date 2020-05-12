using System.Text.RegularExpressions;

namespace Yuniql.Core.SqlBatchParser
{
    public class SemiColonSqlBatchLineAnalyzer : ISqlBatchLineAnalyzer
    {
        Regex regex = new Regex(";", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public SqlBatchLineAnalyzerResult Run(string sqlStatementLine)
        {
            var match = regex.Match(sqlStatementLine);

            return new SqlBatchLineAnalyzerResult
            {
                Start = match.Index,
                End = match.Index + match.Length,

                //check if the batch separator is inside valid sql statement ex. SELECT 'This is not a terminator!!;'
                IsMatched = match.Success && (match.Index + match.Length == sqlStatementLine.Length)
            };
        }

        public bool IsRequireWholeLineStripped => false;
    }
}
