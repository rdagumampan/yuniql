/*Data for the table regions */
 
INSERT INTO ${TENANT_RESERVED_NAME}.regions(region_id,region_name) VALUES (1,'Europe');
INSERT INTO ${TENANT_RESERVED_NAME}.regions(region_id,region_name) VALUES (2,'Americas');
INSERT INTO ${TENANT_RESERVED_NAME}.regions(region_id,region_name) VALUES (3,'Asia');
INSERT INTO ${TENANT_RESERVED_NAME}.regions(region_id,region_name) VALUES (4,'Middle East and Africa');
 
 
/*Data for the table countries */
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('AR','Argentina',2);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('AU','Australia',3);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('BE','Belgium',1);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('BR','Brazil',2);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('CA','Canada',2);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('CH','Switzerland',1);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('CN','China',3);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('DE','Germany',1);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('DK','Denmark',1);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('EG','Egypt',4);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('FR','France',1);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('HK','HongKong',3);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('IL','Israel',4);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('IN','India',3);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('IT','Italy',1);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('JP','Japan',3);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('KW','Kuwait',4);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('MX','Mexico',2);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('NG','Nigeria',4);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('NL','Netherlands',1);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('SG','Singapore',3);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('UK','United Kingdom',1);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('US','United States of America',2);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('ZM','Zambia',4);
INSERT INTO ${TENANT_RESERVED_NAME}.countries(country_id,country_name,region_id) VALUES ('ZW','Zimbabwe',4);
 
/*Data for the table locations */
INSERT INTO ${TENANT_RESERVED_NAME}.locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (1400,'2014 Jabberwocky Rd','26192','Southlake','Texas','US');
INSERT INTO ${TENANT_RESERVED_NAME}.locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (1500,'2011 Interiors Blvd','99236','South San Francisco','California','US');
INSERT INTO ${TENANT_RESERVED_NAME}.locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (1700,'2004 Charade Rd','98199','Seattle','Washington','US');
INSERT INTO ${TENANT_RESERVED_NAME}.locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (1800,'147 Spadina Ave','M5V 2L7','Toronto','Ontario','CA');
INSERT INTO ${TENANT_RESERVED_NAME}.locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (2400,'8204 Arthur St',NULL,'London',NULL,'UK');
INSERT INTO ${TENANT_RESERVED_NAME}.locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (2500,'Magdalen Centre, The Oxford Science Park','OX9 9ZB','Oxford','Oxford','UK');
INSERT INTO ${TENANT_RESERVED_NAME}.locations(location_id,street_address,postal_code,city,state_province,country_id) VALUES (2700,'Schwanthalerstr. 7031','80925','Munich','Bavaria','DE');