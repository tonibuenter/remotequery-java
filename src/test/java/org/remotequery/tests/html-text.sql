--
-- HTML TEXT
--

-- drop table JGROUND.T_HTML_TEXT
create table JGROUND.T_HTML_TEXT (
  HTML_TEXT_ID varchar(512),
  HTML_TEXT CLOB,
  primary key (HTML_TEXT_ID)
);


create table JGROUND.T_HTML_TEXT (
  HTML_TEXT_ID varchar(512),
  HTML_TEXT mediumtext,
  primary key (HTML_TEXT_ID)
);

-- MySQL version...

create table JGROUND.T_HTML_TEXT (
  HTML_TEXT_ID varchar(512),
  HTML_TEXT mediumtext,
  primary key (HTML_TEXT_ID)
);