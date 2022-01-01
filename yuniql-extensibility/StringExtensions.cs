using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Yuniql.Extensibility
{
    /// <summary>
    /// Extensions to String class
    /// </summary>
    public static class StringExtensions
    {

        /// <summary>
        /// Returns 6-char fixed lengh string and removed - 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Fixed(this string str)
        {
            return str.Substring(0, 6).ToUpper().Replace("-", "");
        }

        /// <summary>
        /// Retursn a single qouted string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Quote(this string str)
        {
            return $"'{str}'";
        }

        /// <summary>
        /// Returns true when string is enclosed in single quote
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsSingleQuoted(this string str)
        {
            return str.ToString().StartsWith("'") && str.ToString().EndsWith("'");
        }

        /// <summary>
        /// Retursn a double qouted string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DoubleQuote(this string str)
        {
            return $"\"{str}\"";
        }

        /// <summary>
        /// Returns true when string is enclosed in double quote
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDoubleQuoted(this string str)
        {
            return str.ToString().StartsWith("\"") && str.ToString().EndsWith("\"");
        }

        /// <summary>
        /// Replaces \ with \\ in string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Escape(this string str)
        {
            return str.Replace(@"\", @"\\");
        }

        /// <summary>
        /// Replaces \\ with \ in string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Unescape(this string str)
        {
            return str.Replace(@"\\", @"\");
        }

        /// <summary>
        /// Returns schema name and object name
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="defaultSchema"></param>
        /// <returns></returns>
        public static Tuple<string, string> SplitSchema(this string objectName, string defaultSchema)
        {
            return objectName.SplitSchema(defaultSchema, CaseSenstiveOption.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="defaultSchema"></param>
        /// <param name="caseSensitive"></param>
        /// <param name="caseSenstiveOption"></param>
        /// <returns></returns>
        public static Tuple<string, string> SplitSchema(this string objectName, string defaultSchema, CaseSenstiveOption caseSenstiveOption)
        {
            //check if a non-default dbo schema is used
            var schemaName = defaultSchema;
            var newObjectName = objectName;
            if (objectName.IndexOf('.') > 0)
            {
                schemaName = objectName.Split('.')[0];
                newObjectName = objectName.Split('.')[1];
            }

            if (caseSenstiveOption == CaseSenstiveOption.None)
            {
                return new Tuple<string, string>(schemaName, newObjectName);
            }
            else if (caseSenstiveOption == CaseSenstiveOption.QuouteWhenAnyLowerCase)
            {
                //we do this because snowflake, oracle always converts unquoted names into upper case
                schemaName = schemaName.HasLower() ? schemaName.DoubleQuote() : schemaName;
                newObjectName = newObjectName.HasLower() ? newObjectName.DoubleQuote() : newObjectName;
            }
            else if (caseSenstiveOption == CaseSenstiveOption.QuouteWhenAnyUpperCase)
            {
                //we do this because postgres always converts unquoted names into small case
                schemaName = schemaName.HasUpper() ? schemaName.DoubleQuote() : schemaName;
                newObjectName = newObjectName.HasUpper() ? newObjectName.DoubleQuote() : newObjectName;
            }

            else if (caseSenstiveOption == CaseSenstiveOption.LowerCaseWhenAnyUpperCase)
            {
                //we do this because reshift, mysql, mariadb always converts all names into small case
                //this is regardless if the names is double qouted names, it still ends up as lower case
                schemaName = schemaName.HasUpper() ? schemaName.ToLower() : schemaName;
                newObjectName = newObjectName.HasUpper() ? newObjectName.ToLower() : newObjectName;
            }

            return new Tuple<string, string>(schemaName, newObjectName);
        }

        /// <summary>
        /// Returns segments of a file to represent sequence no, schema name and table name
        /// These are the valid file name patterns: 1.myschema.mytable, 01.myschema.mytable, myschema.mytable, 1.mytable, 01.mytable, mytable
        /// If you dont specify the schema, the default schema will derived from specific database platform
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="defaultSchema"></param>
        /// <returns></returns>
        public static Tuple<string, string, string> SplitBulkFileName(this string objectName, string defaultSchema = null)
        {
            var temp = objectName.Split('.');
            if (temp.Length > 3)
            {
                throw new ArgumentException(
                    "Bulk file name must have maximum 3 segments. " +
                    "These are the valid file name patterns: 1.myschema.mytable.csv, 01.myschema.mytable.csv, 1.mytable.csv, 01.mytable.csv, myschema.mytable.csv, mytable.csv. " +
                    "If you dont specify the schema, the default schema will derived from specific database platform.");
            }

            if (temp.Length == 3)
            {
                //covers 1.myschema.mytable
                return new Tuple<string, string, string>(temp[0], temp[1], temp[2]);
            }
            else if (temp.Length == 2)
            {
                //check if first part is inetended as sequence number of schema
                //example input: myschema.mytable, 1.mytable or 01.mytable
                if (int.TryParse(temp[0], out int sequenceNo))
                {
                    //covers 1.mytable or 01.mytable
                    return new Tuple<string, string, string>(temp[0], defaultSchema, temp[1]);
                }
                else
                {
                    //covers myschema.mytable
                    return new Tuple<string, string, string>(string.Empty, temp[0], temp[1]);
                }
            }
            else if (temp.Length == 1)
            {
                //covers mytable
                return new Tuple<string, string, string>(string.Empty, defaultSchema, temp[0]);
            }

            return null;
        }

        /// <summary>
        /// Returns true when string has capital letters
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasUpper(this string str)
        {
            return str.Any(c => char.IsUpper(c));
        }

        /// <summary>
        /// Returns true when all charactercs are capital letters
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasLower(this string str)
        {
            return str.Any(c => char.IsLower(c));
        }

        /// <summary>
        /// Returns string without single or double quote enclosure
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UnQuote(this string str)
        {
            return $"{str.Substring(1, str.Length - 2)}";
        }

        /// <summary>
        /// Returns string with token replaced
        /// </summary>
        /// <param name="str">The string where tokens can be present</param>
        /// <param name="tokens">List of token/value pairs</param>
        /// <returns></returns>
        public static string ReplaceTokens(this string str, ITraceService traceService, List<KeyValuePair<string, string>> tokens = null)
        {
            //check if the sql statement has tokens in it
            var tokenPattern = @"\${([^}]+)}";
            var tokenParser = new Regex(tokenPattern);
            var tokenMatches = tokenParser.Matches(str);
            if (!tokenMatches.Any())
                return str;

            //when no token values passed but sql statement has tokens, we fail the whole migration
            var errorMessage = $"Some tokens were not successfully replaced. " +
                    $"This ussually due to missing or insufficient token key/value pairs passed during migration run. " +
                    $"See the faulting script below. {Environment.NewLine}";
            if ((null == tokens || !tokens.Any()) && tokenMatches.Any())
            {
                throw new ApplicationException($"{errorMessage}{str}");
            }

            //attempt to replace tokens in the input string or sql statement
            var processedSqlStatement = new StringBuilder(str);
            tokenMatches
                .Select(t => t.Value.Substring(2, t.Length - 3))
                .Distinct().ToList()
                .ForEach(k =>
            {
                var kv = tokens.Where(q => q.Key == k);
                if (kv.Any())
                {
                    var t = kv.Single();
                    processedSqlStatement.Replace($"${{{t.Key}}}", t.Value);
                    traceService.Debug($"Replaced token {t.Key} with {t.Value}");
                }
            });

            //when some tokens were not replaced because some token/value keypairs are not passed, we fail the whole migration
            //unreplaced tokens may cause unforseen production issues and better abort entire migration
            var tokenMatchesAfterReplacement = tokenParser.Matches(processedSqlStatement.ToString());
            if (tokenMatchesAfterReplacement.Any())
            {
                throw new ApplicationException($"{errorMessage}{processedSqlStatement}");
            }

            return processedSqlStatement.ToString();
        }
    }

    /// <summary>
    /// Defines the qouting and case behaviour when splitting object and schema
    /// </summary>
    public enum CaseSenstiveOption
    {
        None,                       //applies to sqlserver
        QuouteWhenAnyLowerCase,     //applies to snowflake, oracle
        QuouteWhenAnyUpperCase,     //applies to postgres
        LowerCaseWhenAnyUpperCase,  //applies to redshift, mysql, mariadb
    }
}
