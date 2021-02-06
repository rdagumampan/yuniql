CREATE TABLE departments (
    department_id SERIAL PRIMARY KEY,
    department_name CHARACTER VARYING (30) NOT NULL,
    location_id INTEGER,
    FOREIGN KEY (location_id) REFERENCES locations (location_id)
);
 
CREATE TABLE jobs (
    job_id SERIAL PRIMARY KEY,
    job_title CHARACTER VARYING (35) NOT NULL,
    min_salary NUMERIC (8, 2),
    max_salary NUMERIC (8, 2)
);
