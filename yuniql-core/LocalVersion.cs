using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Yuniql.Core
{
    /// <summary>
    /// Representions individual migration version entry.
    /// </summary>
    public class LocalVersion
    {
        private const int Maxlength = 190;

        /// <summary>
        /// Creates new instance of LocalVersion
        /// </summary>
        public LocalVersion()
        {
        }

        /// <summary>
        /// Creates new instance of LocalVersion
        /// </summary>
        /// <param name="version">The version in format v{Major}.{Minor}. Example v1.01 or v2.00.</param>
        /// <param name="path">The full path of version directory</param>
        public LocalVersion(string version, string path) : this()
        {
            if (version.Length > Maxlength)
                throw new Exception(@$"Invalid format of version directory ""{version}"". Exceeded maxlength {Maxlength}");

            var versionPattern = @"^v(?:(\d+)\.(\d+)\.(\d+)|(\d+)\.(\d+)|(\d+))?(?:\.\*)?";
            var versionMatch = Regex.Match(version, versionPattern);
            if (!versionMatch.Success)
                throw new Exception(@$"Invalid format of version directory ""{version}"". Expected format is ""v<xx|major>.<xx|minor>*"" or ""v<xx|major>.<xx|minor><any-label>"". Some working examples can be like ""v0.00"", ""v1.01"", ""v2.01-big-index-rebuild"".");

            var versionSegments = new List<string>();
            for (int i = 1; i < versionMatch.Groups.Count; i++)
            {
                if (!string.IsNullOrEmpty(versionMatch.Groups[i].Value))
                    versionSegments.Add(versionMatch.Groups[i].Value);
            }

            if (versionSegments.Count == 1)
            {
                Major = Convert.ToInt32(versionSegments[0]);
                Label = version.Replace($"v{versionSegments[0]}", string.Empty);
            }
            else if (versionSegments.Count == 2)
            {
                Major = Convert.ToInt32(versionSegments[0]);
                Minor = Convert.ToInt32(versionSegments[1]);
                Label = version.Replace($"v{versionSegments[0]}.{versionSegments[1]}", string.Empty);
            }
            else if (versionSegments.Count == 3)
            {
                Major = Convert.ToInt32(versionSegments[0]);
                Minor = Convert.ToInt32(versionSegments[1]);
                Revision = Convert.ToInt32(versionSegments[2]);
                Label = version.Replace($"v{versionSegments[0]}.{versionSegments[1]}.{versionSegments[2]}", string.Empty);
            }

            Name = version;
            Path = path;
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
        /// Returns the revision part of version.
        /// </summary>
        public int Revision { get; set; }

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

        /// <summary>
        /// Returns the original version name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns the full path of local directory
        /// </summary>
        public string Path { get; set; }
    }
}
