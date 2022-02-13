using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Yuniql.Extensibility.SqlBatchParser;

namespace Yuniql.Oracle
{
    /// <summary>
    /// 
    /// </summary>
    public class OracleSqlBatchParser : ISqlBatchParser
    {
        private const string CustomStatementStartRegEx = "create( )*(or( )*replace )?( )*(procedure|package|trigger)";
        private const string CustomStatementEndRegEx = ";(\n|\r| )*/( )*$";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlStatementRaw"></param>
        /// <returns></returns>
        public List<SqlStatement> Parse(string sqlStatementRaw)
        {
            return Parse(sqlStatementRaw, new SqlBatchParserOption());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlStatementRaw"></param>
        /// <param name="parseOption"></param>
        /// <returns></returns>
        public List<SqlStatement> Parse(string sqlStatementRaw, SqlBatchParserOption parseOption)
        {
            var results = new List<SqlStatement>();
            var sqlStatement = string.Empty;
            var sqlStatementLine2 = string.Empty; byte lineNo = 0;
            bool isCustomStatement = false;

            using (var sr = new StringReader(sqlStatementRaw))
            {
                while ((sqlStatementLine2 = sr.ReadLine()) != null)
                {
                    if (sqlStatementLine2.Length > 0 && !sqlStatementLine2.StartsWith("--"))
                    {
                        if (!isCustomStatement && Regex.IsMatch(sqlStatementLine2, CustomStatementStartRegEx, RegexOptions.IgnoreCase))
                        {
                            isCustomStatement = true;
                        }

                        sqlStatement += (sqlStatement.Length > 0 ? Environment.NewLine : string.Empty) + sqlStatementLine2;

                        if (IsCommandEnded(sqlStatement, isCustomStatement))
                        {
                            results.Add(new SqlStatement { BatchText = RemoveEndCommand(sqlStatement, isCustomStatement) });
                            isCustomStatement = false;
                            sqlStatement = string.Empty;
                        }
                    }
                    ++lineNo;
                }

                //pickup the last formed sql statement
                if (!string.IsNullOrEmpty(sqlStatement.Trim()))
                {
                    results.Add(new SqlStatement { BatchText = sqlStatement });
                }
            }

            return results;
        }

        private string RemoveEndCommand(string sqlStatement, bool isCustomStatement)
        {
            if (isCustomStatement)
            {
                return sqlStatement.TrimEnd('\n', '\r', ' ', '/');
            }
            else
            {
                return sqlStatement.TrimEnd('\n', '\r', ' ', ';');
            }
        }

        private bool IsCommandEnded(string sqlStatement, bool isCustomStatement)
        {
            if (isCustomStatement)
            {
                return Regex.IsMatch(sqlStatement, CustomStatementEndRegEx);
            }
            else
            {
                return sqlStatement.TrimEnd('\n', '\r', ' ').EndsWith(";");
            }
        }

    }
}
