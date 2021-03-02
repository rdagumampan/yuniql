CREATE TABLE departments (
    department_id INT PRIMARY KEY,
    department_name VARCHAR (30) NOT NULL,
    location_id INT,
    FOREIGN KEY (location_id) REFERENCES locations (location_id)
);

CREATE TABLE jobs (
    job_id INT PRIMARY KEY,
    job_title VARCHAR (35) NOT NULL,
    min_salary DECIMAL (8, 2),
    max_salary DECIMAL (8, 2)
);