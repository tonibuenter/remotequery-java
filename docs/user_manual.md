# RemoteQuery User Manual

## RemoteQuery Components

The following diagram shows the main component and players in a RemoteQuery request.

![Players of the RemoteQuery](https://docs.google.com/drawings/d/e/2PACX-1vSe5Eh_cRISGGNsO2fOIHZ29ol4Pksf60_jdGR-n13sZMXS8vUKdR9QhGhMdd7aQojkt5NIcGKKV08E/pub?w=960&amp;h=720)


- *Caller* : A standalone or web-based Java application can create a RemoteQuery request.
- *RemoteQuery Request* : The request is build of service id, parameters, user id and roles. In a secured enviroment the user id and the roles are part of a authentication and authorization process.
- *RemoteQuery Run* : The request is processed.
- *Service Entry* : A service entry is detected according to the service id. The servcie execution is protected by the roles of the service entry. To be authorized to execute a service the request roles and the service entry roles have to have at least one common role. It there is no service entry role, the service is unprotected.
- *Resources* : The processing of a request involves database and other resources like files.
- *RemoteQuery Result* : After processing the request a RQ result is returned. The result mainly consists of a header, a table and a process log object.  


Code example:

```java
Request request = new Request();
request.setServiceId("Address.search");
request.put("nameFilter", "John%");
request.addRole("ADDRESS_READER");
Result result = request.run();
```

... or in one line: 

```java
Result result = new Request().put("nameFilter", "John%").addRole("ADDRESS_READER").run("Address.search");
```


## RemoteQuery Request

A RemoteQuery request consists of

- serviceId:  the unique name (key) of the service
- parameters: this is a key/value of Strings, in Java just a map like Map<String, String>
- userId: the user id of the caller
- roles: roles a set of roles, usually the roles of the calling user



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

set name = hello
;
select VALUE from JGROUND.T_APP_PROPERTIES where NAME = :name
```


## RemoteQuery Statements


RemoteQuery statements are a list of semi-colon separated statements.
(A semi-colon can be escaped with a backslash '\'.)


A statement can be one of the following type:

- **SQL**: the statement is just SQL statement
- **Command**: the statement has a command identifier and a parameter part

The type are automatically recognized by RemoteQuery. Commands start with the command identifier. The following command identifier are build-in:

- *set* (synonym *put*): setting a parameter with a value
- *set-if-empty* (synonym *put-if-empty*): only set the parameter when empty
- *copy* : copy value from parameter to parameter
- *copy-if-empty* :  copy value from parameter to parameter only when empty
- *parameters*: setting parameter values with a query
- *parameters-if-empty*: setting parameter values with a query when empty
- *serviceId*: calling another service
- *include*: include the statements or content of another service, without role check of the included service entry statements
- *java*: referencing a Java class that implements the IQuery interface
- *if*, *else*, *end*  :  conditional execution of statements
- *switch*, *case*, *default*, *break*, *end*  : switch execution of statements
- *foreach*, *end*  :  loop over a result
- *while*, *end*  : simple conditional execution of statements


## Results

If a RQ statement produces a RQ result it is given as 'current result' to the next statement. Finally it is returned back to the request (returned by the run method of the request).

### SNAIL_CASE to camelCase

An unique design feature of RemoteQuery is the convertion of snail case to camel case.
As SQL names most often apply snail case due to case insensitivity (e.g.: USER\_ID, MANAGEMENT\_OFFICE, DAY\_OF\_WEEK, calender_Week), these names are mapped to the RQ result header by applying a camel case converstion:

SNAIL_CASE | camelCase|
--- | ---
USER\_ID|userId
MANAGEMENT\_OFFICE | managementOffice
DAY\_OF\_WEEK | dayOfWeek
calender\_Week |calenderWeek

So a query like:

```
SELECT USER\_ID, calendar\_week from ...
```

would results in a result JSON like:

```
{
  "header" : ["userId", "calendarWeek"],
  "table" : [
        ...
  ]
}
```









## SQL Statements

Writing SQL statements is done pretty straight forward. The following rules apply:

- Use *:parameter* for an SQL parameter from the request. 
- All paramters are Strings. Apply conversions when necessary in the SQL query (e.g. to_timestamp). Many database systems actually do the conversion automatically.
- You can use everything the corresponding JDBC driver understands (e.g. calling stored procecures).
- Make sure to escape semicolon inside the query  by **\;**


### Command Statements

Command statements have been introduced to augment the SQL processing in a simple and light way. The command structure is like

- **command** white_space\* parameter\_part

Example:

```
set days = 25
```


Basically, for each command there is a command class that runs the command. 
The example above uses the SetCommand class which is doing a put ('days', '13') to the current request object. 





## Statement Extensibility

The programming with statements can be extended in the following way

- Using the *java* command and provide a Java class with all the possibility Java programming provides.
- Register an **additional command** identifier like create-new-user:
```
    RemoteQuery.Commands.Registry.put("create-new-user", new CreateNewUser());

```
The CreateNewUser has to implement the ICommand:

```java
  public interface ICommand {

    public Result run(Request request, Result currentResult, CommandNode commandNode, ServiceEntry serviceEntry);

  }

```

So, a service entry could use the new command like:

```
--
-- SERVICE_ID = Test.Command.extension_newuser
-- 

create-new-user John Smith

```

For further details on the command see:

[RemoteQuery Reference Manual](reference_manual.md)








