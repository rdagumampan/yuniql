/*Data for the table jobs */
INSERT INTO jobs(job_id,job_title,min_salary,max_salary) 
VALUES 
    (1,'Public Accountant',4200.00,9000.00),
    (2,'Accounting Manager',8200.00,16000.00),
    (3,'Administration Assistant',3000.00,6000.00),
    (4,'President',20000.00,40000.00),
    (5,'Administration Vice President',15000.00,30000.00),
    (6,'Accountant',4200.00,9000.00),
    (7,'Finance Manager',8200.00,16000.00),
    (8,'Human Resources Representative',4000.00,9000.00),
    (9,'Programmer',4000.00,10000.00),
    (10,'Marketing Manager',9000.00,15000.00),
    (11,'Marketing Representative',4000.00,9000.00),
    (12,'Public Relations Representative',4500.00,10500.00),
    (13,'Purchasing Clerk',2500.00,5500.00),
    (14,'Purchasing Manager',8000.00,15000.00),
    (15,'Sales Manager',10000.00,20000.00),
    (16,'Sales Representative',6000.00,12000.00),
    (17,'Shipping Clerk',2500.00,5500.00),
    (18,'Stock Clerk',2000.00,5000.00),
    (19,'Stock Manager',5500.00,8500.00)
 GO
 
/*Data for the table departments */
INSERT INTO departments(department_id,department_name,location_id) 
VALUES 
    (1,'Administration',1700),
    (2,'Marketing',1800),
    (3,'Purchasing',1700),
    (4,'Human Resources',2400),
    (5,'Shipping',1500),
    (6,'IT',1400),
    (7,'Public Relations',2700),
    (8,'Sales',2500),
    (9,'Executive',1700),
    (10,'Finance',1700),
    (11,'Accounting',1700)
GO