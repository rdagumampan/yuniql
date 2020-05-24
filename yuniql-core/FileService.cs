using System.IO;

namespace Yuniql.Core
{
    /// <summary>
    /// Wraps usage of <see cref="File"/>.
    /// </summary>
    public class FileService : IFileService
    {
        ///<inheritdoc/>
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        ///<inheritdoc/>
        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }
}
