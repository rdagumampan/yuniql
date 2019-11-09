using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArdiLabs.Yuniql
{
    public class TokenReplacementService : ITokenReplacementService
    {
        private const string tokenPattern = @"\${([^}]+)}";
        public string Replace(List<KeyValuePair<string, string>> tokens, string sqlStatement)
        {
            if (null == tokens || !tokens.Any()) return sqlStatement;

            var procssedSqlStatement = new StringBuilder(sqlStatement);
            tokens.ForEach(t =>
            {
                procssedSqlStatement.Replace($"${{{t.Key}}}", t.Value);
                TraceService.Debug($"Replaced {t.Key} with {t.Value}");
            });

            return procssedSqlStatement.ToString();
        }
    }
}