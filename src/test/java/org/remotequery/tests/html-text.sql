--
-- HTML TEXT
--

create table JGROUND.T_HTML_TEXT (

  HTML_TEXT_ID varchar(512),
  HTML_TEXT clob,
  primary key (HTML_TEXT_ID)

);


-- MySQL version...

create table JGROUND.T_HTML_TEXT (

  HTML_TEXT_ID varchar(512),
  HTML_TEXT mediumtext,
  primary key (HTML_TEXT_ID)

);