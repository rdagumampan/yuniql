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
        public string ReadAllEmbeddedText(string path)
        {
            var assembly = typeof(WorkspaceService).Assembly;
            var resource = assembly.GetManifestResourceStream(path);
            using (var reader = new StreamReader(resource))
            {
                return reader.ReadToEnd();
            }
        }

        ///<inheritdoc/>
        public void AppendAllText(string path, string contents) {
            File.AppendAllText(path, contents);
        }

        ///<inheritdoc/>
        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }
}
