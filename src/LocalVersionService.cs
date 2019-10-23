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
            string initFolderLocation = Path.Combine(workingPath, "_init");
            if (!Directory.Exists(initFolderLocation))
            {
                Directory.CreateDirectory(initFolderLocation);
                TraceService.Info($"Created script directory {initFolderLocation}");
            }

            string preFolderLocation = Path.Combine(workingPath, "_pre");
            if (!Directory.Exists(preFolderLocation))
            {
                Directory.CreateDirectory(preFolderLocation);
                TraceService.Info($"Created script directory {preFolderLocation}");
            }

            string defaultVersion = Path.Combine(workingPath, "v0.00");
            if (!Directory.Exists(defaultVersion))
            {
                Directory.CreateDirectory(defaultVersion);
                TraceService.Info($"Created script directory {defaultVersion}");
            }

            string draftFolderLocation = Path.Combine(workingPath, "_draft");
            if (!Directory.Exists(draftFolderLocation))
            {
                Directory.CreateDirectory(draftFolderLocation);
                TraceService.Info($"Created script directory {draftFolderLocation}");
            }

            string postFolderLocation = Path.Combine(workingPath, "_post");
            if (!Directory.Exists(postFolderLocation))
            {
                Directory.CreateDirectory(postFolderLocation);
                TraceService.Info($"Created script directory {postFolderLocation}");
            }

            var readMeFile = Path.Combine(workingPath, "README.md");
            if (!File.Exists(readMeFile))
            {
                File.AppendAllText(readMeFile, @"
# Database migrations

This database migration project is created and to be executed thru `yuniql` tool. 
For documentation and how-to guides, please visit yuniql [github page](https://github.com/rdagumampan/yuniql).
");
                TraceService.Info($"Created file {readMeFile}");
            }

            var dockerFile = Path.Combine(workingPath, "Dockerfile");
            if (!File.Exists(dockerFile))
            {
                File.AppendAllText(dockerFile, "");
                TraceService.Info($"Created file {dockerFile}");
            }

            var gitIgnoreFile = Path.Combine(workingPath, ".gitignore");
            if (!File.Exists(gitIgnoreFile))
            {
                File.AppendAllText(gitIgnoreFile, $"yuniql.exe{Environment.NewLine}yuniql.pdb");
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
                CreateTemplateSqlFile(sqlFilePath);
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
                CreateTemplateSqlFile(sqlFilePath);
                TraceService.Info($"Created file {sqlFilePath}");
            }

            return nextMinorVersion.SemVersion;
        }

        private static void CreateTemplateSqlFile(string sqlFilePath)
        {
            using (var sw = File.CreateText(sqlFilePath))
            {
                sw.WriteLine(@"
--this is a demo comment
CREATE TABLE [dbo].[_DemoTable](        
	[Id][int] IDENTITY(1, 1) NOT NULL,        
)
GO

CREATE PROC [dbo].[_DemoStoredProcedure]
AS
	SELECT 1;
GO

CREATE VIEW [dbo].[_DemoTableView]
AS
	SELECT Id FROM [dbo].[_DemoTable];
GO

DROP VIEW [dbo].[_DemoTableView];
DROP PROC [dbo].[_DemoStoredProcedure];
DROP TABLE [dbo].[_DemoTable];
GO
                    ");
            }
        }

    }
}
