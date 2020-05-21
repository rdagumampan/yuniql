using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Yuniql.Extensibility.SqlBatchParser
{
    public class CommentAnalyzer : ICommentAnalyzer
    {
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

            //skip single and double quoted expressions
            var literalsAndQuotedValues = @"('(('')|[^'])*')";

            var regex = new Regex($"{nestedMultiLineBlockCommentPattern}|{singleLineBlockCommentPattern}|{inlineDashDashCommentPattern}|{inlineDashDashCommentsOnLastLinePattern}|{literalsAndQuotedValues}", RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //var regex = new Regex($"{allPattern}|{literalsAndQuotedValues}", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
            var match = regex.Match(sqlStatementRaw);
            while (match.Success)
            {
                if (!match.Value.StartsWith("--") && !match.Value.StartsWith("/*"))
                {
                    match = match.NextMatch();
                }
                else
                {
                    var commentBlock = new CommentAnalyzerResult { Text = match.Value, Start = match.Index, End = match.Index + match.Length };
                    resultList.Add(commentBlock);

                    match = match.NextMatch();
                }
            }

            return resultList;
        }
    }

}
