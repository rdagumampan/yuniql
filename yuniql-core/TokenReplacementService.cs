using Yuniql.Extensibility;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

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
            //when no token values passed, do nothing
            if (null == tokens || !tokens.Any()) return sqlStatement;

            //when no tokens found in sql statement, do nothing
            var tokenParser = new System.Text.RegularExpressions.Regex(tokenPattern);
            var tokenMatches = tokenParser.Matches(sqlStatement);
            if (!tokenMatches.Any()) return sqlStatement;

            var processedSqlStatement = new StringBuilder(sqlStatement);
            tokens.ForEach(t =>
            {
                processedSqlStatement.Replace($"${{{t.Key}}}", t.Value);
                _traceService.Debug($"Replaced {t.Key} with {t.Value}");
            });

            //when some tokens were not replaced because no values was passed, we fail the whole migration
            //unreplaced tokens may cause unforseen production issues
            var tokenMatchesAfterReplacement = tokenParser.Matches(processedSqlStatement.ToString());
            if (tokenMatchesAfterReplacement.Any())
            {
                throw new YuniqlMigrationException($"Some tokens were not successfully replaced. " +
                    $"This ussually due to missing or insufficient token key/value pairs passed during migration run. " +
                    $"See the faulting script below. {Environment.NewLine}" +
                    $"{processedSqlStatement.ToString()}");
            }

            return processedSqlStatement.ToString();
        }
    }
}