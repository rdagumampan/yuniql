using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArdiLabs.Yuniql
{
    public class LocalVersionService : ILocalVersionService
    {
        public void Init(string workingPath)
        {
            string initDirectoryPath = Path.Combine(workingPath, "_init");
            if (!Directory.Exists(initDirectoryPath))
            {
                Directory.CreateDirectory(initDirectoryPath);
                File.AppendAllText(Path.Combine(initDirectoryPath, "README.md"), @"# The `_init` directory
Initialization scripts. Executed once. This is called the first time you do `yuniql run`.");
                TraceService.Info($"Created script directory {initDirectoryPath}");
            }

            string preDirectoryPath = Path.Combine(workingPath, "_pre");
            if (!Directory.Exists(preDirectoryPath))
            {
                Directory.CreateDirectory(preDirectoryPath);
                File.AppendAllText(Path.Combine(preDirectoryPath, "README.md"), @"# The `_pre` directory
Pre migration scripts. Executed every time before any version. 
");
                TraceService.Info($"Created script directory {preDirectoryPath}");
            }

            string defaultVersionDirectoryPath = Path.Combine(workingPath, "v0.00");
            if (!Directory.Exists(defaultVersionDirectoryPath))
            {
                Directory.CreateDirectory(defaultVersionDirectoryPath);
                File.AppendAllText(Path.Combine(defaultVersionDirectoryPath, "README.md"), @"# The `v0.00` directory
Baseline scripts. Executed once. This is called when you do `yuniql run`.");
                TraceService.Info($"Created script directory {defaultVersionDirectoryPath}");
            }

            string draftDirectoryPath = Path.Combine(workingPath, "_draft");
            if (!Directory.Exists(draftDirectoryPath))
            {
                Directory.CreateDirectory(draftDirectoryPath);
                File.AppendAllText(Path.Combine(draftDirectoryPath, "README.md"), @"# The `_draft` directory
Scripts in progress. Scripts that you are currently working and have not moved to specific version directory yet. Executed every time after the latest version.");
                TraceService.Info($"Created script directory {draftDirectoryPath}");
            }

            string postDirectoryPath = Path.Combine(workingPath, "_post");
            if (!Directory.Exists(postDirectoryPath))
            {
                Directory.CreateDirectory(postDirectoryPath);
                File.AppendAllText(Path.Combine(postDirectoryPath, "README.md"), @"# The `_post` directory
Post migration scripts. Executed every time and always the last batch to run.");
                TraceService.Info($"Created script directory {postDirectoryPath}");
            }

            string eraseDirectoryPath = Path.Combine(workingPath, "_erase");
            if (!Directory.Exists(eraseDirectoryPath))
            {
                Directory.CreateDirectory(eraseDirectoryPath);
                File.AppendAllText(Path.Combine(eraseDirectoryPath, "README.md"), @"# The `_erase` directory
Database cleanup scripts. Executed once only when you do `yuniql erase`.");
                TraceService.Info($"Created script directory {eraseDirectoryPath}");
            }

            var readMeFile = Path.Combine(workingPath, "README.md");
            if (!File.Exists(readMeFile))
            {
                File.AppendAllText(readMeFile, @"

## Database migration project
This database migration project is created and to be executed thru `yuniql`. 
For more how-to guides and deep-divers, please visit yuniql [wiki page on github](https://github.com/rdagumampan/yuniql/wiki).

## Run this migration with yuniql on docker
Open command prompt in current folder.

For simplified run
```
docker build -t your-project-name .
docker run your-project-name -c ""your-connection-string""
```

For running with token replacement
```
docker run your-project-name -c ""your-connection-string\"" -k \""VwColumnPrefix1=App1,VwColumnPrefix2=App2,VwColumnPrefix3=App3,VwColumnPrefix4=App4\""
```

## Found bugs?

Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).
");
                TraceService.Info($"Created file {readMeFile}");
            }

            var dockerFile = Path.Combine(workingPath, "Dockerfile");
            if (!File.Exists(dockerFile))
            {
                File.AppendAllText(dockerFile, @"
FROM rdagumampan/yuniql:nightly
COPY . ./db                
");
                TraceService.Info($"Created file {dockerFile}");
            }

            var gitIgnoreFile = Path.Combine(workingPath, ".gitignore");
            if (!File.Exists(gitIgnoreFile))
            {
                File.AppendAllText(gitIgnoreFile, @"
yuniql.exe
yuniql.pdb
yuniqlx.exe
");
                TraceService.Info($"Created file {gitIgnoreFile}");
            }

        }

        private List<LocalVersion> GetLocalVersions(string workingPath)
        {
            var localVersions = Directory.GetDirectories(workingPath, "v*.*")
                .Select(x => new DirectoryInfo(x).Name)
                .Select(x =>
                {
                    int majorVersion = Convert.ToInt32(x.Substring(1, x.IndexOf(".") - 1));
                    int minorVersion = Convert.ToInt32(x.Substring(x.IndexOf(".") + 1));
                    var r = new LocalVersion
                    {
                        Major = majorVersion,
                        Minor = minorVersion
                    };

                    return r;
                })
                .OrderBy(x => x.SemVersion)
                .Reverse()
                .ToList();

            return localVersions;
        }

        public string GetLatestVersion(string workingPath)
        {
            return GetLocalVersions(workingPath).First().SemVersion;
        }

        public string IncrementMajorVersion(string workingPath, string sqlFileName)
        {
            var localVersions = GetLocalVersions(workingPath);

            var nextMajorVersion = new LocalVersion { Major = localVersions.First().Major + 1, Minor = 0 };
            localVersions.Add(nextMajorVersion);

            string nextVersionPath = Path.Combine(workingPath, nextMajorVersion.SemVersion);
            Directory.CreateDirectory(nextVersionPath);
            TraceService.Info($"Created script directory {nextVersionPath}");

            if (!string.IsNullOrEmpty(sqlFileName))
            {
                var sqlFilePath = Path.Combine(nextVersionPath, sqlFileName);
                File.AppendAllText(sqlFilePath, @"");
                TraceService.Info($"Created file {sqlFilePath}");
            }

            return nextMajorVersion.SemVersion;
        }

        public string IncrementMinorVersion(string workingPath, string sqlFileName)
        {
            var localVersions = GetLocalVersions(workingPath);

            var nextMinorVersion = new LocalVersion { Major = localVersions.First().Major, Minor = localVersions.First().Minor + 1 };
            localVersions.Add(nextMinorVersion);

            string nextVersionPath = Path.Combine(workingPath, nextMinorVersion.SemVersion);
            Directory.CreateDirectory(nextVersionPath);
            TraceService.Info($"Created script directory {nextVersionPath}");

            if (!string.IsNullOrEmpty(sqlFileName))
            {
                var sqlFilePath = Path.Combine(nextVersionPath, sqlFileName);
                File.AppendAllText(sqlFilePath, @"");
                TraceService.Info($"Created file {sqlFilePath}");
            }

            return nextMinorVersion.SemVersion;
        }
    }
}
