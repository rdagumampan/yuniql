CREATE TABLE regions (
    region_id INT PRIMARY KEY,
    region_name CHAR VARYING (25)
);

CREATE TABLE countries (
    country_id CHAR (2) PRIMARY KEY,
    country_name VARCHAR (40),
    region_id INT NOT NULL,
    FOREIGN KEY (region_id) REFERENCES regions (region_id)
);

CREATE TABLE locations (
    location_id INT PRIMARY KEY,
    street_address VARCHAR (40),
    postal_code VARCHAR (12),
    city VARCHAR (30) NOT NULL,
    state_province VARCHAR (25),
    country_id CHAR (2) NOT NULL,
    FOREIGN KEY (country_id) REFERENCES countries (country_id)
);