Example: Ticketing Services
==================================

This examples shows a list of services for a tiny ticketing system.

Here the use cases which have to be implemented:

- *user creates new ticket*
- *supporter reads ticket and writes a response*
- *user reads response and writes comments*
- *supporter closes, postpones or delegates ticket*
- *supporter searches tickets*
- *user searches tickets*

We start bottom-up building the database







From the caller or user side, the first player in RemoteQuery processing is the request.

RemoteQuery Request
-----

A RemoteQuery request consists of

- serviceId:  the unique name (key) of the service
- parameters: this is a key/value of Strings, in Java just a map like Map<String, String>
- userId: the user id of the calling user
- roles: roles a set of roles, usually the roles of the calling user

Here an example how a request gets created:

//??? remote query request creation


When starting the processing of a request, more players enter the stage such as service entry, statements and more.

RemoteQuery Service Entry
------------------------- 

The service entry is the actual code that is executed. The service entry consists of

- serviceId: the unique name (key) of the service
- roles: the roles the service is protected with, also called service roles
- statements: a list of RemoteQuery statements

//??? remote service entry



RemoteQuery Statements
------------------------- 

RemoteQuery statements are a list of semi-colon delimited statements.

//??? remote query statements

A statement can be one of the following:

- command: the statement has a command part and a parameter part
- SQL: the statement is just SQL statement


Built-in commands:

- *set*: setting a parameter
- *set-if-empty*: only set the parameter when empty or blank
- *serviceId*: calling another service
- *include*: include the statements or content of another service
- *java*: referencing a Java class that implements the IQuery interface
//??? more



