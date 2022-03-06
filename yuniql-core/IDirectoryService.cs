using System.Collections.Generic;
using System.IO;

namespace Yuniql.Core
{
    /// <summary>
    /// Interface for implementing wrapper for Directory.See usage of <see cref="Directory"./>
    /// </summary>
    public interface IDirectoryService
    {
        /// <summary>
        /// Wraps <see cref="Directory.GetDirectories"/>
        /// </summary>
        string[] GetDirectories(string path, string searchPattern);

        /// <summary>
        /// Wraps <see cref="Directory.GetDirectories"/>
        /// </summary>
        string[] GetAllDirectories(string path, string searchPattern);

        /// <summary>
        /// Wraps <see cref="Directory.GetFiles"/>
        /// </summary>
        string[] GetFiles(string path, string searchPattern);

        /// <summary>
        /// Wraps <see cref="Directory.GetFiles"/>
        /// </summary>
        string[] GetAllFiles(string path, string searchPattern);

        /// <summary>
        /// Wraps <see cref="Directory.Exists"/>
        /// </summary>
        public bool Exists(string path);

        /// <summary>
        /// Wraps <see cref="Directory.GetFiles"/>
        /// </summary>
        string GetFileCaseInsensitive(string path, string fileName);

        /// <summary>
        /// Returns list of files in the target environment specific directory
        /// </summary>
        /// <param name="path"></param>
        /// <param name="environmentCode"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        string[] FilterFiles(string path, string environmentCode, List<string> files);

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="path"></param>
        /// <param name="environmentCode"></param>
        /// <param name="directories"></param>
        /// <returns></returns>
        string[] FilterDirectories(string path, string environmentCode, List<string> directories);

        /// <summary>
        /// Return sorted files based on default sort order or using in-placed sorting manifest _manifest.ini
        /// </summary>
        /// <param name="path"></param>
        /// <param name="environmentCode"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        string[] SortFiles(string path, string environmentCode, List<string> files);

        /// <summary>
        /// Wraps <see cref="Directory.CreateDirectory"/>
        /// </summary>
        DirectoryInfo CreateDirectory(string path);
    }
}