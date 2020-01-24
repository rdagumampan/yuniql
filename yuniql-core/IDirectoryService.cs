using System.IO;

namespace Yuniql.Core
{
    /// <summary>
    /// Interface for implementing wrapper for Directory.See usage of <see cref="Directory"./>
    /// </summary>
    public interface IDirectoryService
    {
        /// <summary>
        /// Wraps <see cref="Directory.GetDirectories"./>
        /// </summary>
        string[] GetDirectories(string path, string searchPattern);

        /// <summary>
        /// Wraps <see cref="Directory.GetDirectories"./>
        /// </summary>
        string[] GetDirectories(string path, string searchPattern, SearchOption searchOption);

        /// <summary>
        /// Wraps <see cref="Directory.GetFiles"./>
        /// </summary>
        string[] GetFiles(string path, string searchPattern);

        /// <summary>
        /// Wraps <see cref="Directory.Exists"./>
        /// </summary>
        public bool Exists(string path);

        /// <summary>
        /// Wraps <see cref="Directory.GetFiles"./>
        /// </summary>
        string GetFileCaseInsensitive(string path, string fileName);
    }
}