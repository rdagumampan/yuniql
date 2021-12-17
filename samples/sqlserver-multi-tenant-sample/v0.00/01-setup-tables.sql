CREATE TABLE ${TENANT_RESERVED_NAME}.regions (
    region_id INT IDENTITY(1,1) PRIMARY KEY,
    region_name VARCHAR (25) DEFAULT NULL
);
 
CREATE TABLE ${TENANT_RESERVED_NAME}.countries (
    country_id CHAR (2) PRIMARY KEY,
    country_name VARCHAR (40) DEFAULT NULL,
    region_id INT NOT NULL,
    FOREIGN KEY (region_id) REFERENCES ${TENANT_RESERVED_NAME}.regions (region_id)
);
 
CREATE TABLE ${TENANT_RESERVED_NAME}.locations (
    location_id INT IDENTITY(1,1) PRIMARY KEY,
    street_address VARCHAR (40) DEFAULT NULL,
    postal_code VARCHAR (12) DEFAULT NULL,
    city VARCHAR (30) NOT NULL,
    state_province VARCHAR (25) DEFAULT NULL,
    country_id CHAR (2) NOT NULL,
    FOREIGN KEY (country_id) REFERENCES ${TENANT_RESERVED_NAME}.countries (country_id)
);
