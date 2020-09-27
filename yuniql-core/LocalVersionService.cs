using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// Service responsible for initializing and managing the local workspace. A local workspace is a directory where
    /// yuniql operations are executed from. When user calls yuniql-init, a directory structure is created in the target
    /// workspace directory.
    /// </summary>
    public class LocalVersionService : ILocalVersionService
    {
        private readonly ITraceService _traceService;

        ///<inheritdoc/>
        public LocalVersionService(ITraceService traceService) { _traceService = traceService; }


        ///<inheritdoc/>
        public void Init(string workingPath)
        {
            string initDirectoryPath = Path.Combine(workingPath, "_init");
            if(!Directory.Exists(initDirectoryPath))
            {
                Directory.CreateDirectory(initDirectoryPath);
                File.AppendAllText(Path.Combine(initDirectoryPath, "README.md"),
                                   @"# The `_init` directory
Initialization scripts. Executed once. This is called the first time you do `yuniql run`.");
                _traceService.Info($"Created script directory {initDirectoryPath}");
            }

            string preDirectoryPath = Path.Combine(workingPath, "_pre");
            if(!Directory.Exists(preDirectoryPath))
            {
                Directory.CreateDirectory(preDirectoryPath);
                File.AppendAllText(Path.Combine(preDirectoryPath, "README.md"),
                                   @"# The `_pre` directory
Pre migration scripts. Executed every time before any version. 
");
                _traceService.Info($"Created script directory {preDirectoryPath}");
            }

            string defaultVersionDirectoryPath = Path.Combine(workingPath, "v0.00");
            if(!Directory.Exists(defaultVersionDirectoryPath))
            {
                Directory.CreateDirectory(defaultVersionDirectoryPath);
                File.AppendAllText(Path.Combine(defaultVersionDirectoryPath, "README.md"),
                                   @"# The `v0.00` directory
Baseline scripts. Executed once. This is called when you do `yuniql run`.");
                _traceService.Info($"Created script directory {defaultVersionDirectoryPath}");
            }

            string draftDirectoryPath = Path.Combine(workingPath, "_draft");
            if(!Directory.Exists(draftDirectoryPath))
            {
                Directory.CreateDirectory(draftDirectoryPath);
                File.AppendAllText(Path.Combine(draftDirectoryPath, "README.md"),
                                   @"# The `_draft` directory
Scripts in progress. Scripts that you are currently working and have not moved to specific version directory yet. Executed every time after the latest version.");
                _traceService.Info($"Created script directory {draftDirectoryPath}");
            }

            string postDirectoryPath = Path.Combine(workingPath, "_post");
            if(!Directory.Exists(postDirectoryPath))
            {
                Directory.CreateDirectory(postDirectoryPath);
                File.AppendAllText(Path.Combine(postDirectoryPath, "README.md"),
                                   @"# The `_post` directory
Post migration scripts. Executed every time and always the last batch to run.");
                _traceService.Info($"Created script directory {postDirectoryPath}");
            }

            string eraseDirectoryPath = Path.Combine(workingPath, "_erase");
            if(!Directory.Exists(eraseDirectoryPath))
            {
                Directory.CreateDirectory(eraseDirectoryPath);
                File.AppendAllText(Path.Combine(eraseDirectoryPath, "README.md"),
                                   @"# The `_erase` directory
Database cleanup scripts. Executed once only when you do `yuniql erase`.");
                _traceService.Info($"Created script directory {eraseDirectoryPath}");
            }

            string dropDirectoryPath = Path.Combine(workingPath, "_drop");
            if(!Directory.Exists(dropDirectoryPath))
            {
                Directory.CreateDirectory(dropDirectoryPath);
                File.AppendAllText(Path.Combine(dropDirectoryPath, "README.md"),
                                   @"# The `_drop` directory
Drop database scripts. Executed once only when you do `yuniql drop --force`.");
                _traceService.Info($"Created script directory {dropDirectoryPath}");
            }

            var readMeFile = Path.Combine(workingPath, "README.md");
            if(!File.Exists(readMeFile))
            {
                File.AppendAllText(readMeFile,
                                   @"

## Yuniql-based Database Migration Project
This database migration project is created and to be executed thru `yuniql`. 
For more how-to guides and deep-divers, please visit yuniql [wiki page on github](https://github.com/rdagumampan/yuniql/wiki).

## Run this migration with yuniql on docker
Open command prompt in current folder.

For simplified run
```
docker build -t <your-project-name> .
docker run your-project-name -c ""<your-connection-string>""
```

For running with token replacement
```
docker run <your-project-name> -c ""<your-connection-string>\"" -k \""<Token1=TokenValue1,Token2=TokebValue2,Token3=TokenValue3,Token4=TokenValue4\>""
```

## How does this works?
When you call `docker build`, we pull the base image containing the nightly build of `yuniql` and all of your local structure is copied into the image. When you call `docker run`, `yuniql run` is executed internally on your migration directory.

>NOTE: The container must have access to the target database. You may need to configure a firewall rule to accept login requests from the container hosts esp for cloud-based databases.


## Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
");
                _traceService.Info($"Created file {readMeFile}");
            }

            var dockerFile = Path.Combine(workingPath, "Dockerfile");
            if(!File.Exists(dockerFile))
            {
                File.AppendAllText(dockerFile,
                                   @"
FROM rdagumampan/yuniql:latest
COPY . ./db                
");
                _traceService.Info($"Created file {dockerFile}");
            }

            var gitIgnoreFile = Path.Combine(workingPath, ".gitignore");
            if(!File.Exists(gitIgnoreFile))
            {
                File.AppendAllText(gitIgnoreFile,
                                   @"
.plugins
yuniql.exe
yuniql.pdb
yuniqlx.exe
");
                _traceService.Info($"Created file {gitIgnoreFile}");
            }
        }

        private List<LocalVersion> GetLocalVersions(string workingPath)
        {
            var localVersions = Directory.GetDirectories(workingPath, "v*.*")
                .Select(x => new DirectoryInfo(x).Name)
                .Select(x =>
                {
                    var r = new LocalVersion(x);
                    return r;
                })
                .OrderBy(x => x.SemVersion)
                .Reverse()
                .ToList();

            return localVersions;
        }

        ///<inheritdoc/>
        public string GetLatestVersion(string workingPath) { return GetLocalVersions(workingPath).First().SemVersion; }

        ///<inheritdoc/>
        public string IncrementMajorVersion(string workingPath, string sqlFileName)
        {
            var localVersions = GetLocalVersions(workingPath);

            var nextMajorVersion = new LocalVersion { Major = localVersions.First().Major + 1, Minor = 0 };
            localVersions.Add(nextMajorVersion);

            string nextVersionPath = Path.Combine(workingPath, nextMajorVersion.SemVersion);
            Directory.CreateDirectory(nextVersionPath);
            _traceService.Info($"Created script directory {nextVersionPath}");

            if(!string.IsNullOrEmpty(sqlFileName))
            {
                var sqlFilePath = Path.Combine(nextVersionPath, sqlFileName);
                File.AppendAllText(sqlFilePath, string.Empty);
                _traceService.Info($"Created file {sqlFilePath}");
            }

            return nextMajorVersion.SemVersion;
        }

        ///<inheritdoc/>
        public string IncrementMinorVersion(string workingPath, string sqlFileName)
        {
            var localVersions = GetLocalVersions(workingPath);

            var nextMinorVersion = new LocalVersion
            { Major = localVersions.First().Major, Minor = localVersions.First().Minor + 1 };
            localVersions.Add(nextMinorVersion);

            string nextVersionPath = Path.Combine(workingPath, nextMinorVersion.SemVersion);
            Directory.CreateDirectory(nextVersionPath);
            _traceService.Info($"Created script directory {nextVersionPath}");

            if(!string.IsNullOrEmpty(sqlFileName))
            {
                var sqlFilePath = Path.Combine(nextVersionPath, sqlFileName);
                File.AppendAllText(sqlFilePath, string.Empty);
                _traceService.Info($"Created file {sqlFilePath}");
            }

            return nextMinorVersion.SemVersion;
        }

        ///<inheritdoc/>
        public void Validate(string workingPath)
        {
            string versionZeroDirectory = Directory.GetDirectories(workingPath, "v0.00*").FirstOrDefault();

            var directories = new List<KeyValuePair<string, bool>>
            {
                new KeyValuePair<string, bool>(Path.Combine(workingPath, "_init"),
                                               Directory.Exists(Path.Combine(workingPath, "_init"))),
                new KeyValuePair<string, bool>(Path.Combine(workingPath, "_pre"),
                                               Directory.Exists(Path.Combine(workingPath, "_pre"))),
                new KeyValuePair<string, bool>(Path.Combine(workingPath, "v0.00*"), versionZeroDirectory != null),
                new KeyValuePair<string, bool>(Path.Combine(workingPath, "_draft"),
                                               Directory.Exists(Path.Combine(workingPath, "_draft"))),
                new KeyValuePair<string, bool>(Path.Combine(workingPath, "_post"),
                                               Directory.Exists(Path.Combine(workingPath, "_post"))),
                new KeyValuePair<string, bool>(Path.Combine(workingPath, "_erase"),
                                               Directory.Exists(Path.Combine(workingPath, "_erase"))),
            };

            if(directories.Any(t => !t.Value))
            {
                var message = new StringBuilder();
                directories.ForEach(t => message.AppendLine($"{t.Key} / {(t.Value ? "Found" : "Missing!")}"));

                throw new YuniqlMigrationException($"At least one Yuniql directory is missing in your project. " +
                    $"See validation results below.{Environment.NewLine}{message.ToString()}");
            }
        }
    }
}
