using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// Wraps usage of <see cref="Directory"/>
    /// </summary>
    public class DirectoryService : IDirectoryService
    {
        private ITraceService _traceService;

        public DirectoryService(ITraceService traceService)
        {
            _traceService = traceService;
        }

        ///<inheritdoc/>
        public string[] GetDirectories(string path, string searchPattern)
        {
            return Directory.GetDirectories(path, searchPattern);
        }

        ///<inheritdoc/>
        public string[] GetAllDirectories(string path, string searchPattern)
        {
            return Directory.GetDirectories(path, searchPattern, SearchOption.AllDirectories);
        }

        ///<inheritdoc/>
        public string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        ///<inheritdoc/>
        public string[] GetAllFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        ///<inheritdoc/>
        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        ///<inheritdoc/>
        public string GetFileCaseInsensitive(string path, string fileName)
        {
            return Directory.GetFiles(path, "*.dll")
                .ToList()
                .FirstOrDefault(f => new FileInfo(f).Name.ToLower() == fileName.ToLower());
        }

        ///<inheritdoc/>
        public string[] FilterFiles(string workingPath, string environmentCode, List<string> files)
        {
            var reservedDirectories = new List<string>
            {
                RESERVED_DIRECTORY_NAME.INIT,
                RESERVED_DIRECTORY_NAME.PRE,
                RESERVED_DIRECTORY_NAME.DRAFT,
                RESERVED_DIRECTORY_NAME.POST,
                RESERVED_DIRECTORY_NAME.ERASE,
                RESERVED_DIRECTORY_NAME.DROP,
                RESERVED_DIRECTORY_NAME.TRANSACTION,
            };

            var directoryPathParts = Split(new DirectoryInfo(workingPath)).ToList();
            directoryPathParts.Reverse();

            //check for any presence of an environment-specific directory
            //those are those that starts with "_" such as "_dev", "_test", "_prod" but not the known reserved names
            var hasEnvironmentAwareDirectories = files.Any(f =>
            {
                var filePathParts = Split(new DirectoryInfo(Path.GetDirectoryName(f)))
                    .Where(x => !x.Equals(RESERVED_DIRECTORY_NAME.TRANSACTION, System.StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                filePathParts.Reverse();

                return filePathParts.Skip(directoryPathParts.Count).Any(a =>
                    a.StartsWith(RESERVED_DIRECTORY_NAME.PREFIX)
                    && !reservedDirectories.Exists(x => x.Equals(a, System.StringComparison.InvariantCultureIgnoreCase))
                );
            });

            if (string.IsNullOrEmpty(environmentCode) && !hasEnvironmentAwareDirectories)
                return files.ToArray();

            //throws exception when no environment code passed but environment-aware directories are present
            if (string.IsNullOrEmpty(environmentCode) && hasEnvironmentAwareDirectories)
                throw new YuniqlMigrationException("Found environment aware directories but no environment code passed. " +
                    "See https://github.com/rdagumampan/yuniql/wiki/environment-aware-scripts.");

            //remove all script files from environment-aware directories except the target environment
            var sqlScriptFiles = new List<string>(files);
            files.ForEach(f =>
            {
                var fileParts = Split(new DirectoryInfo(Path.GetDirectoryName(f))).Where(x => !x.Equals(RESERVED_DIRECTORY_NAME.TRANSACTION, System.StringComparison.InvariantCultureIgnoreCase)).ToList();
                fileParts.Reverse();

                var foundFile = fileParts.Skip(directoryPathParts.Count).FirstOrDefault(a => a.StartsWith(RESERVED_DIRECTORY_NAME.PREFIX) && a.ToLower() != $"{RESERVED_DIRECTORY_NAME.PREFIX}{environmentCode}");
                if (null != foundFile)
                    sqlScriptFiles.Remove(f);
            });

            return sqlScriptFiles.ToArray();
        }

        ///<inheritdoc/>
        public string[] SortFiles(string scriptDirectoryPath, string environmentCode, List<string> files)
        {
            //we override the default os-based file names based sort-order
            var sortManifestFilePath = Path.Combine(scriptDirectoryPath, "_sequence.ini");
            var sortManifestExists = File.Exists(sortManifestFilePath);
            if (sortManifestExists)
            {
                var sortedFiles = new List<string>();
                var sortManifestList = File.ReadAllLines(sortManifestFilePath)
                    .Where(f =>
                        !string.IsNullOrEmpty(f) &&
                        !string.IsNullOrWhiteSpace(f) &&
                        !f.Equals(Environment.NewLine))
                    .ToList();

                _traceService.Info("A custom execution sequence manifest is detected. Scripts will run as listed in the content of _sequence.ini file. " +
                    $"Any scripts not listed in the manifest will be skipped and will not be committed in the version where it is placed. " +
                    $"Skipped scripts can only be executed by moving them to the next version. Expected sequence: { Environment.NewLine}" +
                    $"{string.Join(Environment.NewLine, sortManifestList.Select(s => "  + " + s))}");

                sortManifestList.ForEach(ff =>
                {
                    //we use the file name or relative path to check file exists in the directory being processed
                    var file = files.FirstOrDefault(f => f.EndsWith(ff));
                    if (null != file)
                        sortedFiles.Add(file);
                });

                return sortedFiles.ToArray();
            }
            else
            {
                files.Sort();
                return files.ToArray();
            }
        }

        ///<inheritdoc/>
        public string[] FilterDirectories(string workingPath, string environmentCode, List<string> directories)
        {
            throw new System.NotImplementedException();
        }

        private IEnumerable<string> Split(DirectoryInfo directory)
        {
            while (directory != null)
            {
                yield return directory.Name;
                directory = directory.Parent;
            }
        }

        ///<inheritdoc/>
        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }
    }
}
