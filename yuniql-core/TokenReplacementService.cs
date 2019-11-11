using Yuniql.Extensibility;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yuniql.Core
{
    public class TokenReplacementService : ITokenReplacementService
    {
        public TokenReplacementService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        private const string tokenPattern = @"\${([^}]+)}";
        private readonly ITraceService _traceService;

        public string Replace(List<KeyValuePair<string, string>> tokens, string sqlStatement)
        {
            if (null == tokens || !tokens.Any()) return sqlStatement;

            var procssedSqlStatement = new StringBuilder(sqlStatement);
            tokens.ForEach(t =>
            {
                procssedSqlStatement.Replace($"${{{t.Key}}}", t.Value);
                _traceService.Debug($"Replaced {t.Key} with {t.Value}");
            });

            return procssedSqlStatement.ToString();
        }
    }
}