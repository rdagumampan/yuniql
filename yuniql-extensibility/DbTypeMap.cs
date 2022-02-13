using System;

namespace Yuniql.Extensibility
{
    public class DbTypeMap
    {
        public string ColumnName { get; set; }

        public string SqlTypeName { get; set; }

        public Type DotnetType { get; set; }
    }
}
