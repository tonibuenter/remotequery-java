--
-- SERVICE_ID = HtmlText.save
-- ROLES      = APP_USER
-- 

if htmlTextId
;
  delete from JGROUND.T_HTML_TEXT where HTML_TEXT_ID = :htmlTextId
  ;
  insert into JGROUND.T_HTML_TEXT (HTML_TEXT_ID, HTML_TEXT) values (:htmlTextId, :htmlText)
  ;
end



--
-- SERVICE_ID = HtmlText.get
-- ROLES      = APP_USER
-- 

select * from JGROUND.T_HTML_TEXT where HTML_TEXT_ID = :htmlTextId


