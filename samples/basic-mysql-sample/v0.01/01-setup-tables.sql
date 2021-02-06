CREATE TABLE jobs (
    job_id INT (11) AUTO_INCREMENT PRIMARY KEY,
    job_title VARCHAR (35) NOT NULL,
    min_salary DECIMAL (8, 2) DEFAULT NULL,
    max_salary DECIMAL (8, 2) DEFAULT NULL
);
 
CREATE TABLE departments (
    department_id INT (11) AUTO_INCREMENT PRIMARY KEY,
    department_name VARCHAR (30) NOT NULL,
    location_id INT (11) DEFAULT NULL,
    FOREIGN KEY (location_id) REFERENCES locations (location_id)
);
