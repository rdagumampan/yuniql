namespace Yuniql.Core
{
    /// <summary>
    /// Interface for implementing wrapper for File.See usage of <see cref="File"./>
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Opens a text file, reads all the text in the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path of the file to be created.</param>
        /// <returns>Returns the path of file created.</returns>
        string ReadAllText(string path);

        /// <summary>
        /// Opens an embedded text file, reads all the text in the file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string ReadAllEmbeddedText(string path);

        /// <summary>
        /// Determines whether the file exists.
        /// </summary>
        /// <param name="path">Returns true if file exists.</param>
        /// <returns></returns>
        bool Exists(string path);

        /// <summary>
        /// Wraps <see cref="File.AppendAllText"/>
        /// </summary>
        void AppendAllText(string path, string contents);
    }
}