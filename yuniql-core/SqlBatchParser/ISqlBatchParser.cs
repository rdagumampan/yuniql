using System.Collections.Generic;

namespace Yuniql.Core.SqlBatchParser
{
    public interface ISqlBatchParser
    {
        List<SqlStatement> Parse(string sqlStatementRaw);

        List<SqlStatement> Parse(string sqlStatementRaw, SqlBatchParserOption parseOption);
    }
}
