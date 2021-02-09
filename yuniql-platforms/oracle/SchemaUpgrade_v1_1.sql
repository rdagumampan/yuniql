CREATE TABLE ${YUNIQL_DB_NAME}.${YUNIQL_TABLE_NAME}_v1_0 AS SELECT * FROM ${YUNIQL_DB_NAME}.__yuniqldbversion;

DROP TABLE ${YUNIQL_DB_NAME}.__yuniqldbversion;

CREATE TABLE ${YUNIQL_DB_NAME}.${YUNIQL_TABLE_NAME} (
	sequence_id NUMBER NOT NULL,
	version VARCHAR2(190) NOT NULL,
	applied_on_utc TIMESTAMP NOT NULL,
	applied_by_user VARCHAR2(32) NOT NULL,
	applied_by_tool VARCHAR2(32) NOT NULL,
	applied_by_tool_version VARCHAR2(16) NOT NULL,
    status VARCHAR2(32) NOT NULL,
    duration_ms NUMBER NOT NULL,
    checksum VARCHAR2(64) NOT NULL,
    failed_script_path VARCHAR2(4000) NULL,
    failed_script_error VARCHAR2(4000) NULL,
    additional_artifacts VARCHAR2(4000) NULL,
	CONSTRAINT pk_${YUNIQL_TABLE_NAME} PRIMARY KEY (sequence_id),
	CONSTRAINT ix_${YUNIQL_TABLE_NAME} UNIQUE (version)
);

INSERT INTO ${YUNIQL_DB_NAME}.${YUNIQL_TABLE_NAME} (version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, status, duration_ms, checksum, failed_script_path, failed_script_error, additional_artifacts)
SELECT  version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, 'Successful', '0', '', NULL, NULL, NULL 
FROM ${YUNIQL_DB_NAME}.${YUNIQL_TABLE_NAME}_v1_0 
ORDER BY version ASC;