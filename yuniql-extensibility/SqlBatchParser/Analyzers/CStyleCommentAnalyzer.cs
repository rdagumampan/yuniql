using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Yuniql.Extensibility.SqlBatchParser
{
    public class CStyleCommentAnalyzer : ICommentAnalyzer
    {
        public List<CommentAnalyzerResult> Run1(string sqlStatementRaw)
        {
            var resultList = new List<CommentAnalyzerResult>();

            //track all comment formats --, /*/ or multiline neested comment blocks /* /**/ */
            //https://stackoverflow.com/questions/7690380/regular-expression-to-match-all-comments-in-a-t-sql-script/33947706#33947706
            var regex = new Regex(@"/\*(?>(?:(?!\*/|/\*).)*)(?>(?:/\*(?>(?:(?!\*/|/\*).)*)\*/(?>(?:(?!\*/|/\*).)*))*).*?\*/|--.*?\r?[\n]", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var match = regex.Match(sqlStatementRaw);
            while (match.Success)
            {
                var commentBlock = new CommentAnalyzerResult { Text = match.Value, Start = match.Index, End = match.Index + match.Length };
                resultList.Add(commentBlock);
                match = match.NextMatch();
            }

            return resultList;
        }

        public List<CommentAnalyzerResult> Run(string sqlStatementRaw)
        {
            var resultList = new List<CommentAnalyzerResult>();

            //inline comments
            var inlineDashDashCommentPattern = @"--(.*?)\r?\n";
            var inlineDashDashCommentsOnLastLinePattern = @"--(.*?)$";

            //single line block comments
            var singleLineBlockCommentPattern = @"/\*(.*?)\*/?\r?[\n]";

            //multi line block comments            
            var nestedMultiLineBlockCommentPattern = @"/\*
                                (?>
                                /\*  (?<LEVEL>)     # on opening push level
                                | 
                                \*/ (?<-LEVEL>)     # on closing pop level
                                |
                                (?! /\* | \*/ ) .   # match any char unless the opening and closing strings   
                                )+                         # /* or */ in the lookahead string
                                (?(LEVEL)(?!))             # if level exists then fail
                                \*/";

            var regex = new Regex($"{nestedMultiLineBlockCommentPattern}|{singleLineBlockCommentPattern}|{inlineDashDashCommentPattern}|{inlineDashDashCommentsOnLastLinePattern}", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
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
