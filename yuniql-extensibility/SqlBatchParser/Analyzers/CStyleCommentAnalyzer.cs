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

            //all comments
            //https://stackoverflow.com/questions/7690380/regular-expression-to-match-all-comments-in-a-t-sql-script/33947706#33947706
            var regex = new Regex(@"/\*(?>(?:(?!\*/|/\*).)*)(?>(?:/\*(?>(?:(?!\*/|/\*).)*)\*/(?>(?:(?!\*/|/\*).)*))*).*?\*/|--.*?\r?[\n]", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

            ////block comments
            //var regex = new Regex(@"/\*(.*?)\*/?\r?[\n]", RegexOptions.Singleline | RegexOptions.CultureInvariant);

            ////inline comments
            ////var regex = new Regex(@"--(.*?)\r?\n", RegexOptions.Singleline | RegexOptions.CultureInvariant);

            var match = regex.Match(sqlStatementRaw);
            while (match.Success)
            {
                var commentBlock = new CommentAnalyzerResult { Text = match.Value, Start = match.Index, End = match.Index + match.Length };
                resultList.Add(commentBlock);

                match = match.NextMatch();
            }

            return resultList;
        }
    }

}
