CREATE TABLE ${TENANT_RESERVED_NAME}.employees (
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
    FOREIGN KEY (job_id) REFERENCES ${TENANT_RESERVED_NAME}.jobs (job_id),
    FOREIGN KEY (department_id) REFERENCES ${TENANT_RESERVED_NAME}.departments (department_id),
    FOREIGN KEY (manager_id) REFERENCES ${TENANT_RESERVED_NAME}.employees (employee_id)
);
 
CREATE TABLE ${TENANT_RESERVED_NAME}.dependents (
    dependent_id INT IDENTITY(1,1) PRIMARY KEY,
    first_name VARCHAR (50) NOT NULL,
    last_name VARCHAR (50) NOT NULL,
    relationship VARCHAR (25) NOT NULL,
    employee_id INT NOT NULL,
    FOREIGN KEY (employee_id) REFERENCES ${TENANT_RESERVED_NAME}.employees (employee_id)
);