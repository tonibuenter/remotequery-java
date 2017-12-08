--
-- SERVICE_ID = RQService.search
-- ROLES      = APP_USER
-- 

set-if-empty nameFilter = %
;
select SERVICE_ID, STATEMENTS, ROLES 
from
  JGROUND.T_RQ_SERVICE
where
  SERVICE_ID like :nameFilter

