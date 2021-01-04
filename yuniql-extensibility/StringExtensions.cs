using System;

namespace Yuniql.Extensibility
{
    public static class StringExtensions
    {
        public static string Quote(this string str)
        {
            return "'{str}'";
        }

        public static string DoubleQuote(this string str)
        {
            return $"\"{str}\"";
        }

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

        public static bool IsDoubleQuoted(this string str) {
            return str.ToString().StartsWith("\"") && str.ToString().EndsWith("\"");
        }
    }
}
