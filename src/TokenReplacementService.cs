using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
            });

            return procssedSqlStatement.ToString();
        }
    }
}