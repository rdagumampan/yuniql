/*Data for the table regions */
INSERT INTO regions(region_id,region_name) 
VALUES 
  (1,'Europe'),
  (2,'Americas'),
  (3,'Asia'),
  (4,'Middle East and Africa')
GO

/*Data for the table countries */
INSERT INTO countries(country_id,country_name,region_id) 
VALUES 
    ('AR','Argentina',2),
    ('AU','Australia',3),
    ('BE','Belgium',1),
    ('BR','Brazil',2),
    ('CA','Canada',2),
    ('CH','Switzerland',1),
    ('CN','China',3),
    ('DE','Germany',1),
    ('DK','Denmark',1),
    ('EG','Egypt',4),
    ('FR','France',1),
    ('HK','HongKong',3),
    ('IL','Israel',4),
    ('IN','India',3),
    ('IT','Italy',1),
    ('JP','Japan',3),
    ('KW','Kuwait',4),
    ('MX','Mexico',2),
    ('NG','Nigeria',4),
    ('NL','Netherlands',1),
    ('SG','Singapore',3),
    ('UK','United Kingdom',1),
    ('US','United States of America',2),
    ('ZM','Zambia',4),
    ('ZW','Zimbabwe',4)
GO

/*Data for the table locations */
INSERT INTO locations(location_id,street_address,postal_code,city,state_province,country_id) 
VALUES 
    (1400,'2014 Jabberwocky Rd','26192','Southlake','Texas','US'),
    (1500,'2011 Interiors Blvd','99236','South San Francisco','California','US'),
    (1700,'2004 Charade Rd','98199','Seattle','Washington','US'),
    (1800,'147 Spadina Ave','M5V 2L7','Toronto','Ontario','CA'),
    (2400,'8204 Arthur St',NULL,'London',NULL,'UK'),
    (2500,'Magdalen Centre, The Oxford Science Park','OX9 9ZB','Oxford','Oxford','UK'),
    (2700,'Schwanthalerstr. 7031','80925','Munich','Bavaria','DE')
GO