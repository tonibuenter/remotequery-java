--
--
-- INIT 01 BOOTSTRAP
--
-- Warning: SQL commands have to end with ';EOL' !
--

--
-- T_RQ_SERVICE
--

create schema JGROUND
;



create SEQUENCE JGROUND.GLOBAL_ID START with 1000
;



create table JGROUND.T_RQ_SERVICE (
   SERVICE_ID varchar(256),
   STATEMENTS varchar(4000),
   ROLES varchar(4000),
   DATASOURCE varchar(512),
   primary key (SERVICE_ID)
)
;


create table JGROUND.T_DUAL (
  TID bigint, 
  primary key(TID)
)
;

delete from JGROUND.T_DUAL;

insert into JGROUND.T_DUAL (TID) values (0);



create table JGROUND.T_APP_PROPERTIES (
   NAME varchar(512),
   VALUE varchar(4000),
   primary key (NAME)
);


--
-- T_PERSON
--

create table JGROUND.T_PERSON(FIRST_NAME varchar(1024), LAST_NAME varchar(1024));

--
-- RQService.save
--

DELETE FROM JGROUND.T_RQ_SERVICE where SERVICE_ID = 'RQService.save';

INSERT INTO JGROUND.T_RQ_SERVICE 
(
  SERVICE_ID, 
  STATEMENTS, 
  ROLES
)
values 
(
	'RQService.save', 
	'
	delete from JGROUND.T_RQ_SERVICE where SERVICE_ID = :SERVICE_ID; insert into JGROUND.T_RQ_SERVICE 
	(SERVICE_ID, STATEMENTS, ROLES)  
	values 
	(:SERVICE_ID, :statements, :ROLES)
	',
	'SYSTEM,APP_ADMIN'
)
;



insert into JGROUND.T_APP_PROPERTIES
(NAME, VALUE)
values
('hello', 'world')
;
