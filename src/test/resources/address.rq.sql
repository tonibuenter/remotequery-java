--
-- SERVICE_ID = Address.search
-- ROLES      = ADDRESS_READER
-- 

set-if-empty nameFilter = '%'
;
select * from JGROUND.T_ADDRESS where FIRST_NAME like :nameFilter or LAST_NAME like :nameFilter



--
-- SERVICE_ID = Address.save
-- ROLES      = ADDRESS_WRITER
--

if :addressId
  ;
else
  ;
  parameters select NEXT VALUE for JGROUND.global_id as ADDRESS_ID from JGROUND.T_DUAL;
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



--
-- SERVICE_ID = Address.selectWithNamesArray
-- ROLES      = ADDRESS_READER
--

select * from JGROUND.T_ADDRESS where FIRST_NAME in (:names[]) order by ADDRESS_ID


