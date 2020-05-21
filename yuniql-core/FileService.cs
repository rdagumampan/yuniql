using System.IO;
using System.Text;

namespace Yuniql.Core
{
    /// <summary>
    /// Wraps usage of <see cref="File"/>.
    /// </summary>
    public class FileService : IFileService
    {
        /// <summary>
        /// Opens a text file, reads all the text in the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path of the file to be created.</param>
        /// <returns>Returns the path of file created.</returns>
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }

        /// <summary>
        /// Determines whether the file exists.
        /// </summary>
        /// <param name="path">Returns true if file exists.</param>
        /// <returns></returns>
        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }
}
