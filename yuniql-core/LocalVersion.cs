using System;

namespace Yuniql.Core
{
    /// <summary>
    /// Representions individual migration version entry.
    /// </summary>
    public class LocalVersion
    {
        public LocalVersion()
        {
        }

        /// <summary>
        /// Creates new instance of LocalVersion
        /// </summary>
        /// <param name="targetVersion">The target version in format v{Major}.{Minor}. Example v1.01 or v2.00.</param>
        public LocalVersion(string targetVersion)
        {
            Major = Convert.ToInt32(targetVersion.Substring(1, targetVersion.IndexOf(".") - 1));
            Minor = Convert.ToInt32(targetVersion.Substring(targetVersion.IndexOf(".") + 1));
        }

        /// <summary>
        /// Returns the major part of version.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// Returns the minor part of version.
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// Retuns verion in v{Major}.{Minor} format.
        /// Example v0.00 for baseline version.
        /// </summary>
        public string SemVersion
        {
            get
            {
                return $"v{Major}.{Minor.ToString("00")}";
            }
        }
    }
}
