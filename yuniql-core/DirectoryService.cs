using System.IO;
using System.Linq;

namespace Yuniql.Core
{
    /// <summary>
    /// Wraps usage of <see cref="Directory"./>
    /// </summary>
    public class DirectoryService : IDirectoryService
    {

        /// <summary>
        /// Wraps <see cref="Directory.GetDirectories"./>
        /// </summary>
        public string[] GetDirectories(string path, string searchPattern)
        {
            return Directory.GetDirectories(path, searchPattern);
        }

        /// <summary>
        /// Wraps <see cref="Directory.GetDirectories"./>
        /// </summary>
        public string[] GetAllDirectories(string path, string searchPattern)
        {
            return Directory.GetDirectories(path, searchPattern, SearchOption.AllDirectories);
        }

        /// <summary>
        /// Wraps <see cref="Directory.GetFiles"./>
        /// </summary>
        public string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Wraps <see cref="Directory.GetFiles"./>
        /// </summary>
        public string[] GetAllFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        /// <summary>
        /// Wraps <see cref="Directory.Exists"./>
        /// </summary>
        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Returns case insensitive file fullname
        /// </summary>
        public string GetFileCaseInsensitive(string path, string fileName)
        {
            return Directory.GetFiles(path, "*.dll")
                .ToList()
                .FirstOrDefault(f => new FileInfo(f).Name.ToLower() == fileName.ToLower());
        }
    }
}
