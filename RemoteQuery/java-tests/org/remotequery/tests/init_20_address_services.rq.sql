--
-- TEST RQ 
--


--
-- SERVICE_ID = Person.save
--

insert into JGROUND.PERSON (FIRST_NAME, LAST_NAME) values (:firstName, :lastName)