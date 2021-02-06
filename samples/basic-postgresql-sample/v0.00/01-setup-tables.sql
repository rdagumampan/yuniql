CREATE TABLE regions (
    region_id SERIAL PRIMARY KEY,
    region_name CHARACTER VARYING (25)
);
 
CREATE TABLE countries (
    country_id CHARACTER (2) PRIMARY KEY,
    country_name CHARACTER VARYING (40),
    region_id INTEGER NOT NULL,
    FOREIGN KEY (region_id) REFERENCES regions (region_id)
);
 
CREATE TABLE locations (
    location_id SERIAL PRIMARY KEY,
    street_address CHARACTER VARYING (40),
    postal_code CHARACTER VARYING (12),
    city CHARACTER VARYING (30) NOT NULL,
    state_province CHARACTER VARYING (25),
    country_id CHARACTER (2) NOT NULL,
    FOREIGN KEY (country_id) REFERENCES countries (country_id)
);