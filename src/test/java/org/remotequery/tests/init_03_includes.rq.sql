--
-- INIT 03 INCLUDES
--


--
-- SERVICE_ID = Test.Include.includee
-- ROLES      = SYSTEM
--

set:includee=hello;



--
-- SERVICE_ID = Test.Include.includer
--

if isblank
;
set:bla=123
;
include:Test.Include.includee
;
fi;




--
-- SERVICE_ID = Test.Include.result
--

if isblank
;
set:bla=123
;
set:includee=hello;
;
fi;


