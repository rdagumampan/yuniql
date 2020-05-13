using System;
using System.Text.RegularExpressions;

namespace Yuniql.Core
{
    /// <summary>
    /// Representions individual migration version entry.
    /// </summary>
    public class LocalVersion
    {
        private const int Maxlength = 190;

        public LocalVersion()
        {
        }

        /// <summary>
        /// Creates new instance of LocalVersion
        /// </summary>
        /// <param name="targetVersion">The target version in format v{Major}.{Minor}. Example v1.01 or v2.00.</param>
        public LocalVersion(string targetVersion)
        {
            if (targetVersion.Length > Maxlength)
            {
                throw new YuniqlMigrationException(@$"Invalid format of version directory ""{targetVersion}"". Exceeded maxlength {Maxlength}");
            }

            string versionPattern = @"^v(?<major>\d+)\.(?<minor>\d\d)(?<label>.*)$";
            Match versionMatch = Regex.Match(targetVersion, versionPattern);

            if (!versionMatch.Success)
            {
                throw new YuniqlMigrationException(@$"Invalid format of version directory ""{targetVersion}"". Expected format is ""vx.xx*""");
            }

            Major = Convert.ToInt32(versionMatch.Groups["major"].Value);
            Minor = Convert.ToInt32(versionMatch.Groups["minor"].Value);
            Label = versionMatch.Groups["label"].Value;
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
        /// Returns the label part of version.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Retuns verion in v{Major}.{Minor} format.
        /// Example v0.00 for baseline version.
        /// </summary>
        public string SemVersion
        {
            get
            {
                return $"v{Major}.{Minor.ToString("00")}{Label}";
            }
        }
    }
}
