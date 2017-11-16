# Anatonomy of a RemoteQuery Processing

## The players

![Players of the RemoteQuery](https://docs.google.com/drawings/d/e/2PACX-1vSe5Eh_cRISGGNsO2fOIHZ29ol4Pksf60_jdGR-n13sZMXS8vUKdR9QhGhMdd7aQojkt5NIcGKKV08E/pub?w=960&amp;h=720)


- Caller : A standalone or web-based Java application can create a RemoteQuery request.
- RemoteQuery Request : The request is build of service id, parameters, user id and roles. In a secured enviroment the user id and the roles are part of a authentication and authorization process.
- RemoteQuery Run : The request is processed.
- Service Entry : A service entry is detected according to the service id. The servcie execution is protected by the roles of the service entry. To be authorized to execute a service the request roles and the service entry roles have to have at least one common role. It there is no service entry role, the service is unprotected.
- The processing of a request involves database and other resources like files.
- RemoteQuery Result : After processing the request a RQ result is returned. The result mainly consists of a header, a table and a process log object.  


Code example:

```java
Request request = new Request();
request.setServiceId("Address.search");
request.put("nameFilter", "John%");
request.addRole("APP_USER");
Result result = request.run();
```

... or in one line: 

```java
Result result = new Request().setServiceId("Address.search").put("nameFilter", "John%").addRole("APP_USER").run();
```


## RemoteQuery Request

A RemoteQuery request consists of

- serviceId:  the unique name (key) of the service
- parameters: this is a key/value of Strings, in Java just a map like Map<String, String>
- userId: the user id of the caller
- roles: roles a set of roles, usually the roles of the calling user

Here an example how a request gets created:

//??? remote query request creation


When starting the processing of a request, more players enter the stage such as service entry, statements and more.

## RemoteQuery Service Entry

The service entry is the actual code that is executed. The service entry consists of

- serviceId: the unique name (key) of the service
- roles: the roles the service is protected with, also called service roles
- statements: a list of RemoteQuery statements

```
--
-- SERVICE_ID = Test.Command.example
-- ROLES      = APP_ADMIN

set:name=hello
;
select VALUE from JGROUND.T_APP_PROPERTIES where NAME = :name
```


## RemoteQuery Statements


RemoteQuery statements are a list of semi-colon delimited statements.
(A semi-colon can be escaped with a backslash '\'.)

//??? remote query statements

A statement can be one of the following type:

- **command**: the statement has a command identifier and a parameter part
- **SQL**: the statement is just SQL statement

The type are automatically recognized by RemoteQuery. Commands start with the command identifier. The following command identifier are build-in:

- *set*: setting a parameter
- *set-if-empty*: only set the parameter when empty or blank
- *parameters*: setting parameter values with a query
- *parameters-if-empty*: setting parameter values with a query when empty or blank
- *serviceId*: calling another service
- *include*: include the statements or content of another service, without role check of the included service entry statements
- *java*: referencing a Java class that implements the IQuery interface
- *if*, *then*, *else*, *end*  :  conditional execution of statements
- *switch*, *case*, *default*  : switch execution of statements
- *foreach*, *do*, *done*  :  loop over a result
- *while*, *do*, *done*  : simple conditional execution of statements
//??? more

Statement Extensibility

The programming with statements can be extended in the following way

- Using the *java* command and provide a Java class with all the possibility Java programming provides.
- Register an *additional command* identifier.


### Example of an *additional command* 





