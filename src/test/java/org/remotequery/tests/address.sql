create table JGROUND.T_ADDRESS (
   ADDRESS_ID bigint,
   FIRST_NAME varchar(256),
   LAST_NAME varchar(256),
   STREET varchar(256),
   ZIP varchar(256),
   CITY varchar(256),
   primary key (ADDRESS_ID)
);


create table JGROUND.T_ROLE (
   USER_ID varchar(256),
   ROLE varchar(256),
   primary key (USER_ID, ROLE)
);