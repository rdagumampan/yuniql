using System.Text.RegularExpressions;

namespace Yuniql.Extensibility.SqlBatchParser
{
    public class SemiColonSqlBatchLineAnalyzer : ISqlBatchLineAnalyzer
    {
        Regex regex = new Regex(";", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public SqlBatchLineAnalyzerResult Run(string sqlStatementLine)
        {
            var match = regex.Match(sqlStatementLine);
            while (match.Success)
            {
                //check if the batch separator is inside valid sql statement ex. SELECT 'This is not a terminator!!;'
                if ((match.Index + match.Length == sqlStatementLine.Length))
                {
                    return new SqlBatchLineAnalyzerResult
                    {
                        Start = match.Index,
                        End = match.Index + match.Length,
                        IsMatched = true
                    };
                }

                match = match.NextMatch();
            }

            return new SqlBatchLineAnalyzerResult { IsMatched = false };
        }

        public bool IsRequireWholeLineStripped => false;
    }
}
