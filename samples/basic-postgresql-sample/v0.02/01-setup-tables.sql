CREATE TABLE employees (
    employee_id SERIAL PRIMARY KEY,
    first_name CHARACTER VARYING (20),
    last_name CHARACTER VARYING (25) NOT NULL,
    email CHARACTER VARYING (100) NOT NULL,
    phone_number CHARACTER VARYING (20),
    hire_date DATE NOT NULL,
    job_id INTEGER NOT NULL,
    salary NUMERIC (8, 2) NOT NULL,
    manager_id INTEGER,
    department_id INTEGER,
    FOREIGN KEY (job_id) REFERENCES jobs (job_id),
    FOREIGN KEY (department_id) REFERENCES departments (department_id),
    FOREIGN KEY (manager_id) REFERENCES employees (employee_id)
);
 
CREATE TABLE dependents (
    dependent_id SERIAL PRIMARY KEY,
    first_name CHARACTER VARYING (50) NOT NULL,
    last_name CHARACTER VARYING (50) NOT NULL,
    relationship CHARACTER VARYING (25) NOT NULL,
    employee_id INTEGER NOT NULL,
    FOREIGN KEY (employee_id) REFERENCES employees (employee_id)
);