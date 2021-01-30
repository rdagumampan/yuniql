--create backup
SELECT * INTO [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}_v1_0] FROM [${YUNIQL_SCHEMA_NAME}].[__yuniqldbversion]

--drop old version v1.0 tracking table
DROP TABLE [${YUNIQL_SCHEMA_NAME}].[__yuniqldbversion];

--create version v1.1 tracking table
CREATE TABLE [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] (
		[sequence_id] [SMALLINT] IDENTITY(1,1) NOT NULL,
		[version] [NVARCHAR](512) NOT NULL,
		[applied_on_utc] [DATETIME] NOT NULL,
		[applied_by_user] [NVARCHAR](32) NOT NULL,
		[applied_by_tool] [NVARCHAR](32) NOT NULL,
		[applied_by_tool_version] [NVARCHAR](16) NOT NULL,
		[status] [NVARCHAR](32) NOT NULL,
		[duration_ms] [INT] NOT NULL,
		[checksum] [NVARCHAR](64) NOT NULL,
		[failed_script_path] [NVARCHAR](4000) NULL,
		[failed_script_error] [NVARCHAR](4000) NULL,
		[additional_artifacts] [NVARCHAR](4000) NULL,
    CONSTRAINT [PK___${YUNIQL_TABLE_NAME}] PRIMARY KEY CLUSTERED ([sequence_id] ASC),
    CONSTRAINT [IX___${YUNIQL_TABLE_NAME}] UNIQUE NONCLUSTERED  ([version] ASC
));

ALTER TABLE [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] ADD  CONSTRAINT [DF_${YUNIQL_TABLE_NAME}_applied_on_utc]  DEFAULT (GETUTCDATE()) FOR [applied_on_utc];
ALTER TABLE [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] ADD  CONSTRAINT [DF_${YUNIQL_TABLE_NAME}_applied_by_user]  DEFAULT (SUSER_SNAME()) FOR [applied_by_user];

--restore old values
INSERT INTO [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}] ([version], [applied_on_utc], [applied_by_user], [applied_by_tool], [applied_by_tool_version], [status], [duration_ms], [checksum], [failed_script_path], [failed_script_error], [additional_artifacts])
SELECT [Version], [AppliedOnUtc], [AppliedByUser], [AppliedByTool], [AppliedByToolVersion], 'Successful', '0', '', NULL, NULL, NULL
FROM [${YUNIQL_SCHEMA_NAME}].[${YUNIQL_TABLE_NAME}_v1_0];
