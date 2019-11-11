using System.Collections.Generic;

namespace Yuniql.Core
{
    public interface ITokenReplacementService
    {
        string Replace(List<KeyValuePair<string, string>> tokens, string sqlStatement);
    }
}