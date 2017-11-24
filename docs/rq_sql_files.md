## RQ SQL Files

RQ SQL files are very close to regular sql files. The main difference is the header section before the service statements. 

Look at this example:

```
--
-- SERVICE_ID = Address.save
-- ROLES      = ADDRESS_WRITER
--

if addressId
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
```

The header section actually in a sql comment (with `--`) that has two service entry parameters, namely: `SERVICE_ID` and `ROLES`. The RQ statements are placed below the header section. The statements section that will end at the next sql comment (with `--`). 

The service entry parameters are filled into the RQ load service given with the `serviceId` parameter in the `RemoteQueryUtils.processRqSqlText(connection, rqSqlText, serviceId, fileName);` call.

So, the RQ load service might look like:

```
insert into JGROUND.T_RQ_SERVICE 
  (SERVICE_ID, STATEMENTS, ROLES)  
  values 
  (:SERVICE_ID, :statements, :ROLES)
```
  
  
