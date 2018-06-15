--
-- SERVICE_ID = Test.Command.switch_empty
-- INFO       = Overwrite for mysql
--

set prefix=Test.Command.switch_empty%
;
delete from JGROUND.T_APP_PROPERTIES where NAME like :prefix
;
set switchValue=ABC
;
set switchValue=
;
switch switchValue;
  case A12;
  case ;
  case A14;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (concat(:prefix, '1'), 'ok');
  break;
  
  case ;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (concat(:prefix, '2'), 'ok');
  break;
  
  case A14;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (concat(:prefix, '3'), 'ok');
end;
;
select * from JGROUND.T_APP_PROPERTIES where NAME like :prefix
;
parameters select count(*) as "TOTAL1" from JGROUND.T_APP_PROPERTIES where NAME like 'Test.Command.switch%'
;
delete from JGROUND.T_APP_PROPERTIES where NAME like :prefix



--
-- SERVICE_ID = Test.Command.switch_default
-- INFO       = Overwrite for mysql
--

set prefix=Test.Command.switch_default%
;
delete from JGROUND.T_APP_PROPERTIES where NAME like :prefix
;
set switchValue=ABC
;
set switchValue=NOMATCH
;
switch switchValue;
  case A12;
  case ;
  case A14;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (concat(:prefix, '1'), 'ok');
  break;
  
  default ;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (concat(:prefix, '2'), 'ok');
  break;
  
  case A14;
    insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (concat(:prefix, '3'), 'ok');
end;
;
select * from JGROUND.T_APP_PROPERTIES where NAME like :prefix
;
parameters select count(*) as "TOTAL1" from JGROUND.T_APP_PROPERTIES where NAME like 'Test.Command.switch%'
;
delete from JGROUND.T_APP_PROPERTIES where NAME like :prefix
;



--
-- SERVICE_ID = Test.Command.foreach
-- INFO       = Overwrite for mysql
--

parameters 
  select count(*) as "TOTAL1" from JGROUND.T_APP_PROPERTIES 
;
foreach 
  select NAME, VALUE from JGROUND.T_APP_PROPERTIES 
  ;
  insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (concat(:name, '-2'), :value)
  ;
end
;
select * from JGROUND.T_APP_PROPERTIES 
;
parameters
  select count(*) as "TOTAL2" from JGROUND.T_APP_PROPERTIES 

  
  
--
-- SERVICE_ID = Test.Command.backslash
-- INFO       = Overwrite for mysql
-- 

set semicolon = \\;
;
insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE) values ('semicolon', :semicolon)
;
parameters
  select VALUE as "IS_TRUE" from JGROUND.T_APP_PROPERTIES where VALUE = '\\;' and NAME = 'semicolon'
;
if isTrue
;
  set semicolon = ok
;
end



