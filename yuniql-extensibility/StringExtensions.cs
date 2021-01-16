using System;

namespace Yuniql.Extensibility
{
    //TODO: Cover this with unit tests
    public static class StringExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Quote(this string str)
        {
            return $"'{str}'";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsSingleQuoted(this string str)
        {
            return str.ToString().StartsWith("'") && str.ToString().EndsWith("'");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DoubleQuote(this string str)
        {
            return $"\"{str}\"";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDoubleQuoted(this string str)
        {
            return str.ToString().StartsWith("\"") && str.ToString().EndsWith("\"");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Escape(this string str)
        {
            return str.Replace(@"\", @"\\");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Unescape(this string str)
        {
            return str.Replace(@"\\", @"\");
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="defaultSchema"></param>
        /// <returns></returns>
        public static Tuple<string, string, string> SplitBulkFileName(this string objectName, string defaultSchema = null)
        {
            var temp = objectName.Split('.');
            if(temp.Length > 3)
            {
                throw new ArgumentException(
                    "Bulk file name must have maximum 3 segments. " +
                    "These are the valid file name patterns: 1.myschema.mytable, 01.myschema.mytable, myschema.mytable, 1.mytable, 01.mytable, mytable." +
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
                if(int.TryParse(temp[0], out int sequenceNo))
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
    }
}
