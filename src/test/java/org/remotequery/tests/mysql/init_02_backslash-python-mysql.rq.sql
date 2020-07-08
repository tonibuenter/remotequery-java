--
-- SERVICE_ID = Test.Command.backslash
-- INFO       = Overwrite for mysql an python / does not work with nodejs!
-- 

set semicolon = '\\;'
;
insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE) values ('semicolon', :semicolon)
;
parameters
  select VALUE as "IS_TRUE" from JGROUND.T_APP_PROPERTIES where VALUE = '\\;' and NAME = 'semicolon'
;
if :isTrue
;
  set semicolon = 'ok'
;
end