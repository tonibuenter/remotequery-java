
![Players of the RemoteQuery](https://docs.google.com/drawings/d/e/2PACX-1vQsPlanMiS2yX50Qxo3qR4Eb2di8tXoW3387qDHBcaJtvpu18WlyTY-k9Gfcvk8bCVCEhC9akweRta2/pub?w=378&amp;h=94)



# RemoteQuery (RQ)

## An efficient service middleware for SQL and Java


RemoteQuery (RQ) is a simple but powerful tool for secure service creation with focus on SQL queries and Java. 

The highlights:

+ The most simple RQ service: an SQL statement
+ A Java class implementing the RQ IQuery interfase is a RQ service
+ Any RQ service can be protected with a list of roles
+ The RQ Servlet directly maps HTTP parameters to SQ named parameters
+ Simple but powerfull OR (object-relational) support

## Example 1: RemoteQuery Web

Let us assume we have the following RQ service entry: 

```
SERVICE_ID   : Address.search
ROLES        : APP_USER

select FIRST_NAME, LAST_NAME, CITY from T_ADDRESS where FIRST_NAME like :nameFilter or LAST_NAME like :nameFilter
```

The the following URL:

```
http://hostname/remoteQuery/Address.search?nameFilter=Jo%
```
 
returns JSON:

```json
{
  "header" : ["firstName", "lastName", "city"],
  "table" : [
        ["John", "Maier", "Zuerich"],
        ["Mary", "Johnes", "Zuerich"]
  ]
}
```
(If the user has the role APP_USER.)

## Example 2: RemoteQuery as standalone Java


```java

public static class Address {
  public String firstName;
  public String lastName;
  public String city;
}

Result result = new Request().setServiceId("Address.search").put("nameFilter", "Jo%").addRole("APP_USER").run();

// convert to a POJO
List<Address> list = result.asList(Address.class)

```


## Quick Start

Download or clone this repository and run the JUnit classes.

More on : [QuickStart](docs/quickstart.md)

## RemoteQuery on the Web

RemoteQuery is a standalone component that can be used as a web backend service delivery. Part of the distribution a RemoteQuery servlet is provided with a default implementation.

See (docs/remotequery_web.md)


## Service Query and Manual

[Service Query and Manual](docs/anatonomy.md)

