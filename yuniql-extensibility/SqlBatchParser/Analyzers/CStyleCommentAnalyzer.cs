using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Yuniql.Extensibility.SqlBatchParser
{
    public class CStyleCommentAnalyzer : ICommentAnalyzer
    {
        public List<CommentAnalyzerResult> Run(string sqlStatementRaw)
        {
            var resultList = new List<CommentAnalyzerResult>();
            try
            {
                var regex = new Regex(@"/\*(?>(?:(?!\*/|/\*).)*)(?>(?:/\*(?>(?:(?!\*/|/\*).)*)\*/(?>(?:(?!\*/|/\*).)*))*).*?\*/|--.*?\r?[\n]", RegexOptions.Singleline | RegexOptions.CultureInvariant);
                var match = regex.Match(sqlStatementRaw);
                while (match.Success)
                {
                    var commentBlock = new CommentAnalyzerResult { Text = match.Value, Start = match.Index, End = match.Index + match.Length };
                    resultList.Add(commentBlock);

                    match = match.NextMatch();
                }
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }

            return resultList;
        }
    }

}
