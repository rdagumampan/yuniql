CREATE TABLE regions (
    region_id INT IDENTITY(1,1) PRIMARY KEY,
    region_name VARCHAR (25) DEFAULT NULL
);
 
CREATE TABLE countries (
    country_id CHAR (2) PRIMARY KEY,
    country_name VARCHAR (40) DEFAULT NULL,
    region_id INT NOT NULL,
    FOREIGN KEY (region_id) REFERENCES regions (region_id)
);
 
CREATE TABLE locations (
    location_id INT IDENTITY(1,1) PRIMARY KEY,
    street_address VARCHAR (40) DEFAULT NULL,
    postal_code VARCHAR (12) DEFAULT NULL,
    city VARCHAR (30) NOT NULL,
    state_province VARCHAR (25) DEFAULT NULL,
    country_id CHAR (2) NOT NULL,
    FOREIGN KEY (country_id) REFERENCES countries (country_id)
);
 
CREATE TABLE jobs (
    job_id INT IDENTITY(1,1) PRIMARY KEY,
    job_title VARCHAR (35) NOT NULL,
    min_salary DECIMAL (8, 2) DEFAULT NULL,
    max_salary DECIMAL (8, 2) DEFAULT NULL
);
 
CREATE TABLE departments (
    department_id INT IDENTITY(1,1) PRIMARY KEY,
    department_name VARCHAR (30) NOT NULL,
    location_id INT DEFAULT NULL,
    FOREIGN KEY (location_id) REFERENCES locations (location_id)
);
 
CREATE TABLE employees (
    employee_id INT IDENTITY(1,1) PRIMARY KEY,
    first_name VARCHAR (20) DEFAULT NULL,
    last_name VARCHAR (25) NOT NULL,
    email VARCHAR (100) NOT NULL,
    phone_number VARCHAR (20) DEFAULT NULL,
    hire_date DATE NOT NULL,
    job_id INT NOT NULL,
    salary DECIMAL (8, 2) NOT NULL,
    manager_id INT DEFAULT NULL,
    department_id INT DEFAULT NULL,
    FOREIGN KEY (job_id) REFERENCES jobs (job_id),
    FOREIGN KEY (department_id) REFERENCES departments (department_id),
    FOREIGN KEY (manager_id) REFERENCES employees (employee_id)
);
 
CREATE TABLE dependents (
    dependent_id INT IDENTITY(1,1) PRIMARY KEY,
    first_name VARCHAR (50) NOT NULL,
    last_name VARCHAR (50) NOT NULL,
    relationship VARCHAR (25) NOT NULL,
    employee_id INT NOT NULL,
    FOREIGN KEY (employee_id) REFERENCES employees (employee_id)
);