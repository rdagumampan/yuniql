using System.Collections.Generic;

namespace Yuniql.AspNetCore
{
    public class YuniqlConfiguration
    {
        public string Platform { get; set; } = "sqlserver";

        public string WorkspacePath { get; set; }

        public string ConnectionString { get; set; }

        public bool AutoCreateDatabase { get; set; } = false;

        public string TargetVersion { get; set; }

        public List<KeyValuePair<string, string>> Tokens { get; set; } = new List<KeyValuePair<string, string>>();

        public bool VerifyOnly { get; set; }

        public string Delimiter { get; set; } = ",";

        public bool DebugTraceMode { get; set; } = false;
    }
}
