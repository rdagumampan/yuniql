CREATE TABLE employees (
    employee_id INT (11) AUTO_INCREMENT PRIMARY KEY,
    first_name VARCHAR (20) DEFAULT NULL,
    last_name VARCHAR (25) NOT NULL,
    email VARCHAR (100) NOT NULL,
    phone_number VARCHAR (20) DEFAULT NULL,
    hire_date DATE NOT NULL,
    job_id INT (11) NOT NULL,
    salary DECIMAL (8, 2) NOT NULL,
    manager_id INT (11) DEFAULT NULL,
    department_id INT (11) DEFAULT NULL,
    FOREIGN KEY (job_id) REFERENCES jobs (job_id),
    FOREIGN KEY (department_id) REFERENCES departments (department_id),
    FOREIGN KEY (manager_id) REFERENCES employees (employee_id)
);
 
CREATE TABLE dependents (
    dependent_id INT (11) AUTO_INCREMENT PRIMARY KEY,
    first_name VARCHAR (50) NOT NULL,
    last_name VARCHAR (50) NOT NULL,
    relationship VARCHAR (25) NOT NULL,
    employee_id INT (11) NOT NULL,
    FOREIGN KEY (employee_id) REFERENCES employees (employee_id)
);