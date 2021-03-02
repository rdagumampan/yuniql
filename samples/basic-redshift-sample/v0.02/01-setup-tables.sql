CREATE TABLE employees (
    employee_id INT PRIMARY KEY,
    first_name VARCHAR (20),
    last_name VARCHAR (25) NOT NULL,
    email VARCHAR (100) NOT NULL,
    phone_number VARCHAR (20),
    hire_date DATE NOT NULL,
    job_id INT NOT NULL,
    salary DECIMAL (8, 2) NOT NULL,
    manager_id INT,
    department_id INT,
    FOREIGN KEY (job_id) REFERENCES jobs (job_id),
    FOREIGN KEY (department_id) REFERENCES departments (department_id),
    FOREIGN KEY (manager_id) REFERENCES employees (employee_id)
);

CREATE TABLE dependents (
    dependent_id INT PRIMARY KEY,
    first_name VARCHAR (50) NOT NULL,
    last_name VARCHAR (50) NOT NULL,
    relationship VARCHAR (25) NOT NULL,
    employee_id INT NOT NULL,
    FOREIGN KEY (employee_id) REFERENCES employees (employee_id)
);