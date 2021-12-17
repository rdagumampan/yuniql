using Yuniql.Extensibility;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Yuniql.Core
{
    /// <summary>
    /// Replaces tokens in the script using the pattern ${TOKEN_KEY}. 
    /// Throws exception and fails the migration when some tokens not replaced due to missing token values passed from the client.
    /// </summary>
    public class TokenReplacementService : ITokenReplacementService
    {
        ///<inheritdoc/>
        public TokenReplacementService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        private const string tokenPattern = @"\${([^}]+)}";
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public string Replace(List<KeyValuePair<string, string>> tokens, string sqlStatement)
        {
            //check if the sql statement has tokens in it
            var tokenParser = new System.Text.RegularExpressions.Regex(tokenPattern);
            var tokenMatches = tokenParser.Matches(sqlStatement);
            if (!tokenMatches.Any()) 
                return sqlStatement;

            //when no token values passed but sql statement has tokens, we fail the whole migration
            var errorMessage = $"Some tokens were not successfully replaced. " +
                    $"This ussually due to missing or insufficient token key/value pairs passed during migration run. " +
                    $"See the faulting script below. {Environment.NewLine}";
            if ((null == tokens || !tokens.Any()) && tokenMatches.Any())
            {
                throw new YuniqlMigrationException($"{errorMessage}{sqlStatement}");
            }

            //attempt to replace tokens in the input string or sql statement
            var processedSqlStatement = new StringBuilder(sqlStatement);
            tokenMatches
                .Select(t => t.Value.Substring(2, t.Length - 3))
                .Distinct().ToList()
                .ForEach(k =>
                {
                    var kv = tokens.Where(q => q.Key == k);
                    if(kv.Any())
                    {
                        var t = kv.Single();
                        processedSqlStatement.Replace($"${{{t.Key}}}", t.Value);
                        _traceService.Debug($"Replaced token {t.Key} with {t.Value}");
                    }
                });

            //when some tokens were not replaced because some token/value keypairs are not passed, we fail the whole migration
            //unreplaced tokens may cause unforseen production issues
            var tokenMatchesAfterReplacement = tokenParser.Matches(processedSqlStatement.ToString());
            if (tokenMatchesAfterReplacement.Any())
            {
                throw new YuniqlMigrationException($"{errorMessage}{processedSqlStatement}");
            }

            return processedSqlStatement.ToString();
        }
    }
}