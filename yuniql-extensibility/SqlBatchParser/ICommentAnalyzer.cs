using System.Collections.Generic;

namespace Yuniql.Extensibility.SqlBatchParser
{
    public interface ICommentAnalyzer
    {
        List<CommentAnalyzerResult> Run(string sqlStatementRaw);
    }

}
