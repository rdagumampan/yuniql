--create backup
SELECT * INTO ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME}_v1_0 FROM ${YUNIQL_SCHEMA_NAME}.__yuniqldbversion;

-- --drop old version v1.0 tracking table
DROP TABLE IF EXISTS ${YUNIQL_SCHEMA_NAME}.__yuniqldbversion;

--create version v1.1 tracking table
CREATE TABLE ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME}(
    sequence_id  SMALLSERIAL PRIMARY KEY NOT NULL,
    version VARCHAR(512) NOT NULL,
    applied_on_utc TIMESTAMP NOT NULL DEFAULT(current_timestamp AT TIME ZONE 'UTC'),
    applied_by_user VARCHAR(32) NOT NULL DEFAULT(user),
    applied_by_tool VARCHAR(32) NOT NULL,
    applied_by_tool_version VARCHAR(16) NOT NULL,
    status VARCHAR(32) NOT NULL,
    duration_ms INTEGER NOT NULL,
    checksum VARCHAR(64) NOT NULL,
    failed_script_path VARCHAR(4000) NULL,
    failed_script_error VARCHAR(4000) NULL,
    additional_artifacts VARCHAR(4000) NULL,
    CONSTRAINT ix_${YUNIQL_TABLE_NAME} UNIQUE(version)
);

--restore old values
INSERT INTO ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} (version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, status, duration_ms, checksum, failed_script_path, failed_script_error, additional_artifacts)
SELECT  version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, 'Successful', '0', '', NULL, NULL, NULL 
FROM ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME}_v1_0 
ORDER BY version ASC;