using Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Yuniql.Core
{
    /// <summary>
    /// Service responsible for initializing and managing the local workspace. A local workspace is a directory where yuniql operations are executed from.
    /// When user calls yuniql-init, a directory structure is created in the target workspace directory.
    /// </summary>
    public class WorkspaceService : IWorkspaceService
    {
        private readonly ITraceService _traceService;
        private readonly IDirectoryService _directoryService;
        private readonly IFileService _fileService;

        ///<inheritdoc/>
        public WorkspaceService(
            ITraceService traceService,
            IDirectoryService directoryService,
            IFileService fileService)
        {
            this._traceService = traceService;
            this._directoryService = directoryService;
            this._fileService = fileService;
        }

        ///<inheritdoc/>
        public void Init(string workspace)
        {
            string initDirectoryPath = Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT);
            if (!_directoryService.Exists(initDirectoryPath))
            {
                _directoryService.CreateDirectory(initDirectoryPath);
                _fileService.AppendAllText(Path.Combine(initDirectoryPath, RESERVED_FILE_NAME.README), @$"# The `{RESERVED_DIRECTORY_NAME.INIT}` directory
Initialization scripts. Executed once. This is called the first time you do `yuniql run`.");
                _traceService.Info($"Created script directory {initDirectoryPath}");
            }

            string preDirectoryPath = Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE);
            if (!_directoryService.Exists(preDirectoryPath))
            {
                _directoryService.CreateDirectory(preDirectoryPath);
                _fileService.AppendAllText(Path.Combine(preDirectoryPath, RESERVED_FILE_NAME.README), @$"# The `{RESERVED_DIRECTORY_NAME.PRE}` directory
Pre migration scripts. Executed every time before any version. 
");
                _traceService.Info($"Created script directory {preDirectoryPath}");
            }

            string defaultVersionDirectoryPath = Path.Combine(workspace, RESERVED_DIRECTORY_NAME.BASELINE);
            if (!_directoryService.Exists(defaultVersionDirectoryPath))
            {
                _directoryService.CreateDirectory(defaultVersionDirectoryPath);
                _fileService.AppendAllText(Path.Combine(defaultVersionDirectoryPath, RESERVED_FILE_NAME.README), @"# The `v0.00` directory
Baseline scripts. Executed once. This is called when you do `yuniql run`.");
                _traceService.Info($"Created script directory {defaultVersionDirectoryPath}");
            }

            string draftDirectoryPath = Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT);
            if (!_directoryService.Exists(draftDirectoryPath))
            {
                _directoryService.CreateDirectory(draftDirectoryPath);
                _fileService.AppendAllText(Path.Combine(draftDirectoryPath, RESERVED_FILE_NAME.README), $@"# The `{RESERVED_DIRECTORY_NAME.DRAFT}` directory
Scripts in progress. Scripts that you are currently working and have not moved to specific version directory yet. Executed every time after the latest version.");
                _traceService.Info($"Created script directory {draftDirectoryPath}");
            }

            string postDirectoryPath = Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST);
            if (!_directoryService.Exists(postDirectoryPath))
            {
                _directoryService.CreateDirectory(postDirectoryPath);
                _fileService.AppendAllText(Path.Combine(postDirectoryPath, RESERVED_FILE_NAME.README), $@"# The `{RESERVED_DIRECTORY_NAME.POST}` directory
Post migration scripts. Executed every time and always the last batch to run.");
                _traceService.Info($"Created script directory {postDirectoryPath}");
            }

            string eraseDirectoryPath = Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE);
            if (!_directoryService.Exists(eraseDirectoryPath))
            {
                _directoryService.CreateDirectory(eraseDirectoryPath);
                _fileService.AppendAllText(Path.Combine(eraseDirectoryPath, RESERVED_FILE_NAME.README), $@"# The `{RESERVED_DIRECTORY_NAME.ERASE}` directory
Database cleanup scripts. Executed once only when you do `yuniql erase`.");
                _traceService.Info($"Created script directory {eraseDirectoryPath}");
            }

            var readMeFile = Path.Combine(workspace, RESERVED_FILE_NAME.README);
            if (!_fileService.Exists(readMeFile))
            {
                var assembly = typeof(WorkspaceService).Assembly;
                var embededReadMeFile = $"{assembly.GetName().Name}.TemplateReadMe.md";
                _fileService.AppendAllText(readMeFile, _fileService.ReadAllEmbeddedText(embededReadMeFile));
                _traceService.Info($"Created file {readMeFile}");
            }

            var dockerFile = Path.Combine(workspace, RESERVED_FILE_NAME.DOCKER_FILE);
            if (!_fileService.Exists(dockerFile))
            {
                _fileService.AppendAllText(dockerFile, @"FROM yuniql/yuniql:latest
COPY . ./db                
");
                _traceService.Info($"Created file {dockerFile}");
            }

            var gitIgnoreFile = Path.Combine(workspace, RESERVED_FILE_NAME.GIT_IGNORE_FILE);
            if (!_fileService.Exists(gitIgnoreFile))
            {
                _fileService.AppendAllText(gitIgnoreFile, @"
.plugins
yuniql.exe
yuniql.pdb
yuniqlx.exe
yuniql-log-*.txt
");
                _traceService.Info($"Created file {gitIgnoreFile}");
            }

        }

        private List<LocalVersion> GetLocalVersions(string workspace)
        {
            var localVersions = _directoryService.GetDirectories(workspace, "v*.*")
                .Select(x => new DirectoryInfo(x).Name)
                .Select(x =>
                {
                    var r = new LocalVersion(x, Path.Combine(workspace, x));
                    return r;
                })
                .OrderBy(x => x.Major)
                .ThenBy(x => x.Minor)
                .ThenBy(x => x.Revision)
                .ThenBy(x => x.Label)
                .Reverse()
                .ToList();

            return localVersions;
        }

        ///<inheritdoc/>
        public string GetLatestVersion(string workspace)
        {
            return GetLocalVersions(workspace).First().Name;
        }

        ///<inheritdoc/>
        public string IncrementMajorVersion(string workspace, string sqlFileName)
        {
            var localVersions = GetLocalVersions(workspace);

            var nextMajorVersion = new LocalVersion { Major = localVersions.First().Major + 1, Minor = 0 };
            localVersions.Add(nextMajorVersion);

            string nextVersionPath = Path.Combine(workspace, nextMajorVersion.SemVersion);
            _directoryService.CreateDirectory(nextVersionPath);
            _traceService.Info($"Created script directory {nextVersionPath}");

            if (!string.IsNullOrEmpty(sqlFileName))
            {
                var sqlFilePath = Path.Combine(nextVersionPath, sqlFileName);
                _fileService.AppendAllText(sqlFilePath, @"");
                _traceService.Info($"Created file {sqlFilePath}");
            }

            return nextMajorVersion.SemVersion;
        }

        ///<inheritdoc/>
        public string IncrementMinorVersion(string workspace, string sqlFileName)
        {
            var localVersions = GetLocalVersions(workspace);

            var nextMinorVersion = new LocalVersion { Major = localVersions.First().Major, Minor = localVersions.First().Minor + 1 };
            localVersions.Add(nextMinorVersion);

            string nextVersionPath = Path.Combine(workspace, nextMinorVersion.SemVersion);
            _directoryService.CreateDirectory(nextVersionPath);
            _traceService.Info($"Created script directory {nextVersionPath}");

            if (!string.IsNullOrEmpty(sqlFileName))
            {
                var sqlFilePath = Path.Combine(nextVersionPath, sqlFileName);
                _fileService.AppendAllText(sqlFilePath, @"");
                _traceService.Info($"Created file {sqlFilePath}");
            }

            return nextMinorVersion.SemVersion;
        }

        ///<inheritdoc/>
        public void Validate(string workspace)
        {
            var baselineVersionDirectory = _directoryService.GetDirectories(workspace, "v0.00*").FirstOrDefault();
            var validationResults = new List<KeyValuePair<string, bool>> {
                new KeyValuePair<string, bool>(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT), _directoryService.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.INIT))),
                new KeyValuePair<string, bool>(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE), _directoryService.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.PRE))),
                new KeyValuePair<string, bool>(Path.Combine(workspace, "v0.00*"), baselineVersionDirectory != null),
                new KeyValuePair<string, bool>(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT), _directoryService.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.DRAFT))),
                new KeyValuePair<string, bool>(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST), _directoryService.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.POST))),
                new KeyValuePair<string, bool>(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE), _directoryService.Exists(Path.Combine(workspace, RESERVED_DIRECTORY_NAME.ERASE))),
            };

            if (validationResults.Any(t => !t.Value))
            {
                var message = new StringBuilder();
                validationResults.ForEach(t => message.AppendLine($"{t.Key} / {(t.Value ? "Found" : "Missing!")}"));

                throw new YuniqlMigrationException($"At least one required yuniql directory/folder is missing in your workspace {workspace}." +
                    $"See validation results below.{Environment.NewLine}{message.ToString()}");
            }
        }
    }
}
