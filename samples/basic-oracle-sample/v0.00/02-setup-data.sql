/*Data for the table regions */  
INSERT ALL
 INTO regions(region_id,region_name) VALUES (1,'Europe')
 INTO regions(region_id,region_name) VALUES (2,'Americas')
 INTO regions(region_id,region_name) VALUES (3,'Asia')
 INTO regions(region_id,region_name) VALUES (4,'Middle East and Africa')
SELECT 1 FROM DUAL;

/*Data for the table countries */
INSERT ALL
 INTO countries(country_id,country_name,region_id) VALUES ('AR','Argentina',2)
 INTO countries(country_id,country_name,region_id) VALUES ('AU','Australia',3)
 INTO countries(country_id,country_name,region_id) VALUES ('BE','Belgium',1)
 INTO countries(country_id,country_name,region_id) VALUES ('BR','Brazil',2)
 INTO countries(country_id,country_name,region_id) VALUES ('CA','Canada',2)
 INTO countries(country_id,country_name,region_id) VALUES ('CH','Switzerland',1)
 INTO countries(country_id,country_name,region_id) VALUES ('CN','China',3)
 INTO countries(country_id,country_name,region_id) VALUES ('DE','Germany',1)
 INTO countries(country_id,country_name,region_id) VALUES ('DK','Denmark',1)
 INTO countries(country_id,country_name,region_id) VALUES ('EG','Egypt',4)
 INTO countries(country_id,country_name,region_id) VALUES ('FR','France',1)
 INTO countries(country_id,country_name,region_id) VALUES ('HK','HongKong',3)
 INTO countries(country_id,country_name,region_id) VALUES ('IL','Israel',4)
 INTO countries(country_id,country_name,region_id) VALUES ('IN','India',3)
 INTO countries(country_id,country_name,region_id) VALUES ('IT','Italy',1)
 INTO countries(country_id,country_name,region_id) VALUES ('JP','Japan',3)
 INTO countries(country_id,country_name,region_id) VALUES ('KW','Kuwait',4)
 INTO countries(country_id,country_name,region_id) VALUES ('MX','Mexico',2)
 INTO countries(country_id,country_name,region_id) VALUES ('NG','Nigeria',4)
 INTO countries(country_id,country_name,region_id) VALUES ('NL','Netherlands',1)
 INTO countries(country_id,country_name,region_id) VALUES ('SG','Singapore',3)
 INTO countries(country_id,country_name,region_id) VALUES ('UK','United Kingdom',1)
 INTO countries(country_id,country_name,region_id) VALUES ('US','United States of America',2)
 INTO countries(country_id,country_name,region_id) VALUES ('ZM','Zambia',4)
 INTO countries(country_id,country_name,region_id) VALUES ('ZW','Zimbabwe',4)
SELECT 1 FROM DUAL;

/*Data for the table locations */
INSERT ALL
 INTO locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (1400,'2014 Jabberwocky Rd','26192','Southlake','Texas','US')
 INTO locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (1500,'2011 Interiors Blvd','99236','South San Francisco','California','US')
 INTO locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (1700,'2004 Charade Rd','98199','Seattle','Washington','US')
 INTO locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (1800,'147 Spadina Ave','M5V 2L7','Toronto','Ontario','CA')
 INTO locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (2400,'8204 Arthur St',NULL,'London',NULL,'UK')
 INTO locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (2500,'Magdalen Centre, The Oxford Science Park','OX9 9ZB','Oxford','Oxford','UK')
 INTO locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (2700,'Schwanthalerstr. 7031','80925','Munich','Bavaria','DE')
SELECT 1 FROM DUAL;
