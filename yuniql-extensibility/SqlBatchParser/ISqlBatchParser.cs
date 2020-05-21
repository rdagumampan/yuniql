using System.Collections.Generic;

namespace Yuniql.Extensibility.SqlBatchParser
{
    public interface ISqlBatchParser
    {
        List<SqlStatement> Parse(string sqlStatementRaw);

        List<SqlStatement> Parse(string sqlStatementRaw, SqlBatchParserOption parseOption);
    }
}
