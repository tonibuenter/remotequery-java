--
-- INIT 01 TESTS
--


--
-- SERVICE_ID = Test.Command.set
--

set:name=hello
;
select VALUE from JGROUND.T_APP_PROPERTIES where NAME = :name



--
-- SERVICE_ID = Test.Command.if
--

parameters:select VALUE as "DOES_EXIST" from JGROUND.T_APP_PROPERTIES where NAME = :name
;
if:doesExist
;
then
;
select 'true' as "VALUE" from JGROUND.T_APP_PROPERTIES
;
select 'true' as "VALUE" from JGROUND.T_APP_PROPERTIES
;
else
;
select 'false' as "VALUE" from JGROUND.T_APP_PROPERTIES
;
fi



--
-- SERVICE_ID = Test.Command.foreach
--

parameters:select count(*) as "TOTAL1" from JGROUND.T_APP_PROPERTIES 
;
foreach:select NAME, VALUE from JGROUND.T_APP_PROPERTIES 
;
do
;
insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (:name || '-2', :value)
;
done
;
select * from JGROUND.T_APP_PROPERTIES 
;
parameters:select count(*) as "TOTAL2" from JGROUND.T_APP_PROPERTIES 



--
-- SERVICE_ID = Test.Command.switch
--

delete from JGROUND.T_APP_PROPERTIES where NAME like 'Test.Command.switch%'
;
parameters:select 'A12' as "SWITCH_VALUE" from JGROUND.T_DUAL
;
switch:switchValue;
  case:A12;
  case:A13;
  case:A14;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values ('Test.Command.switch-1', 'ok');
  break;
  
  case:A13;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values ('Test.Command.switch-2', 'ok');
  break;
  
  case:A14;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values ('Test.Command.switch-3', 'ok');
  break;
  
  case:A12;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values ('Test.Command.switch-4', 'ok');
end;
;
select * from JGROUND.T_APP_PROPERTIES where NAME like 'Test.Command.switch%'
;
parameters:select count(*) as "TOTAL1" from JGROUND.T_APP_PROPERTIES where NAME like 'Test.Command.switch%'
;;
delete from JGROUND.T_APP_PROPERTIES where NAME like 'Test.Command.switch%'
;



--
-- SERVICE_ID = Test.Command.switch_empty
--

set:prefix=Test.Command.switch_empty%
;
delete from JGROUND.T_APP_PROPERTIES where NAME like :prefix
;
set:switchValue=ABC
;
set:switchValue=
;
switch:switchValue;
  case:A12;
  case:;
  case:A14;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (:prefix || '1', 'ok');
  break;
  
  case:;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (:prefix || '2', 'ok');
  break;
  
  case:A14;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (:prefix || '3', 'ok');
end;
;
select * from JGROUND.T_APP_PROPERTIES where NAME like :prefix
;
parameters:select count(*) as "TOTAL1" from JGROUND.T_APP_PROPERTIES where NAME like 'Test.Command.switch%'
;
delete from JGROUND.T_APP_PROPERTIES where NAME like :prefix
;



--
-- SERVICE_ID = Test.Command.switch_default
--

set:prefix=Test.Command.switch_default%
;
delete from JGROUND.T_APP_PROPERTIES where NAME like :prefix
;
set:switchValue=ABC
;
set:switchValue=NOMATCH
;
switch:switchValue;
  case:A12;
  case:;
  case:A14;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (:prefix || '1', 'ok');
  break;
  
  default:;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (:prefix || '2', 'ok');
  break;
  
  case:A14;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (:prefix || '3', 'ok');
end;
;
select * from JGROUND.T_APP_PROPERTIES where NAME like :prefix
;
parameters:select count(*) as "TOTAL1" from JGROUND.T_APP_PROPERTIES where NAME like 'Test.Command.switch%'
;
delete from JGROUND.T_APP_PROPERTIES where NAME like :prefix
;

