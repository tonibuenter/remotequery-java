--
-- TEST RQ 
--



--
-- SERVICE_ID = Address.search
-- 

set-if-empty nameFilter = %
;
select * from JGROUND.T_ADDRESS where FIRST_NAME like :nameFilter or LAST_NAME like :nameFilter


--
-- SERVICE_ID = Person.save
--  new Request().setServiceId("Address.search").put("nameFilter", "Jo%").addRole("APP_USER").run();

insert into JGROUND.PERSON (FIRST_NAME, LAST_NAME) values (:firstName, :lastName)