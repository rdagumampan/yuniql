CREATE TABLE ${YUNIQL_DB_NAME}.${YUNIQL_TABLE_NAME}_v1_0 AS SELECT * FROM ${YUNIQL_DB_NAME}.__yuniqldbversion;

DROP TABLE IF EXISTS ${YUNIQL_DB_NAME}.__yuniqldbversion;

CREATE TABLE ${YUNIQL_DB_NAME}.${YUNIQL_TABLE_NAME} (
	sequence_id INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
	version VARCHAR(190) NOT NULL,
	applied_on_utc TIMESTAMP NOT NULL,
	applied_by_user VARCHAR(32) NOT NULL,
	applied_by_tool VARCHAR(32) NOT NULL,
	applied_by_tool_version VARCHAR(16) NOT NULL,
    status VARCHAR(32) NOT NULL,
    duration_ms INT NOT NULL,
    checksum VARCHAR(64) NOT NULL,
    failed_script_path VARCHAR(4000) NULL,
    failed_script_error VARCHAR(4000) NULL,
    additional_artifacts VARCHAR(4000) NULL,
	CONSTRAINT ix_${YUNIQL_TABLE_NAME} UNIQUE (version)
) ENGINE=InnoDB;

INSERT INTO ${YUNIQL_DB_NAME}.${YUNIQL_TABLE_NAME} (version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, status, duration_ms, checksum, failed_script_path, failed_script_error, additional_artifacts)
SELECT  version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version, 'Successful', '0', '', NULL, NULL, NULL 
FROM ${YUNIQL_DB_NAME}.${YUNIQL_TABLE_NAME}_v1_0 
ORDER BY version ASC;