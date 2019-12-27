using System;

namespace Yuniql.Core
{
    public class LocalVersion
    {
        public LocalVersion()
        {
        }

        public LocalVersion(string targetVersion)
        {
            Major = Convert.ToInt32(targetVersion.Substring(1, targetVersion.IndexOf(".") - 1));
            Minor = Convert.ToInt32(targetVersion.Substring(targetVersion.IndexOf(".") + 1));
        }

        public int Major { get; set; }

        public int Minor { get; set; }

        public string SemVersion
        {
            get
            {
                return $"v{Major}.{Minor.ToString("00")}";
            }
        }
    }
}
