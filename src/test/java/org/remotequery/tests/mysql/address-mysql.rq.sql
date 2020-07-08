--
-- SERVICE_ID = Address.save
-- ROLES      = ADDRESS_WRITER
-- INFO       = This is an overwrite for Mysql
--

if :addressId
  ;
else
  ;
  parameters select JGROUND.NEXTVAL() as ADDRESS_ID from JGROUND.T_DUAL;
  insert into JGROUND.T_ADDRESS (ADDRESS_ID) values (:addressId);
end
;
update JGROUND.T_ADDRESS set
  FIRST_NAME = :firstName,
  LAST_NAME = :lastName,
  STREET = :street,
  ZIP = :zip,
  CITY = :city
where
  ADDRESS_ID = :addressId
;
select * from JGROUND.T_ADDRESS where ADDRESS_ID = :addressId

