using System;
using System.Linq;

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
            //check if a non-default dbo schema is used
            var schemaName = defaultSchema;
            var newObjectName = objectName;
            if (objectName.IndexOf('.') > 0)
            {
                schemaName = objectName.Split('.')[0];
                newObjectName = objectName.Split('.')[1];
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
        /// Returns string without single or double quote enclosure
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UnQuote(this string str)
        {
            return $"{str.Substring(1, str.Length - 2)}";
        }
    }
}
