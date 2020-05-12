using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yuniql.Core.SqlBatchParser
{
    public class SqlBatchParser : ISqlBatchParser
    {
        private readonly ISqlBatchLineAnalyzer _sqlBatchLineAnalyzer;

        public SqlBatchParser(ISqlBatchLineAnalyzer sqlBatchLineAnalyzer)
        {
            this._sqlBatchLineAnalyzer = sqlBatchLineAnalyzer;
        }

        public List<SqlStatement> Parse(string sqlStatementRaw)
        {
            return Parse(sqlStatementRaw, new SqlBatchParserOption());
        }

        public List<SqlStatement> Parse(string sqlStatementRaw, SqlBatchParserOption parseOption)
        {
            var sqlStatements = new List<SqlStatement>();

            using (var sr = new StringReader(sqlStatementRaw))
            {
                var sqlStatementBuilder = new StringBuilder();
                var currentLineNumber = 1;
                var currentBatchNumber = 1;
                var currentPositionNumber = 0;

                string line = String.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    //check if current line has the batch separator in it
                    //else we collect the line to build up an sql statement block
                    var result = _sqlBatchLineAnalyzer.Run(line);
                    if (!result.IsMatched)
                    {
                        sqlStatementBuilder.AppendLine(line);
                    }
                    else
                    {
                        //strip entire line for cases such as GO in sql server
                        //but keep the line to cases like semi-colon (;) in snowflake and other platforms
                        var stripLineLength = (_sqlBatchLineAnalyzer.IsRequireWholeLineStripped ? line.Length + Environment.NewLine.Length : 0);
                        if (!_sqlBatchLineAnalyzer.IsRequireWholeLineStripped)
                        {
                            sqlStatementBuilder.AppendLine(line);
                        }

                        sqlStatements.Add(new SqlStatement
                        {
                            BatchNo = currentBatchNumber,
                            BatchText = sqlStatementBuilder.ToString().Trim(),
                            Length = sqlStatementBuilder.Length,
                            Start = currentPositionNumber,
                            End = currentPositionNumber + sqlStatementBuilder.Length - Environment.NewLine.Length
                        });

                        currentBatchNumber++;
                        currentPositionNumber = currentPositionNumber + sqlStatementBuilder.Length + stripLineLength;
                        sqlStatementBuilder.Clear();
                    }

                    currentLineNumber++;
                }
            }

            return sqlStatements;
        }
    }
}
