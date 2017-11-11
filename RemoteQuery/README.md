
![Players of the RemoteQuery](https://docs.google.com/drawings/d/e/2PACX-1vQsPlanMiS2yX50Qxo3qR4Eb2di8tXoW3387qDHBcaJtvpu18WlyTY-k9Gfcvk8bCVCEhC9akweRta2/pub?w=378&amp;h=94)



# RemoteQuery (RQ)


RemoteQuery (RQ) is a very simple but powerful tool for secure service creation, combination and publication. 

It focuses on SQL and Java based implementation. The highlights:

+ An SQL statement with a name is a RQ service
+ Any RQ service can be protected with a list of roles
+ A Java class implementing the RQ IQuery interfase is a RQ service
+ The RQ Servlet directly maps HTTP parameters to SQ named parameters

## Example RemoteQuery Web

Let us assume we have server side the following RQ service entry : 

```
serviceId   : Address.search
statements  : select FIRST_NAME, LAST_NAME, CITY from T_ADDRESS where FIRST_NAME like :nameFilter or LAST_NAME like :nameFilter
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

## Example RemoteQuery Standalone Java


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

See [QuickStart](docs/quickstart.md)

## RemoteQuery on the Web

RemoteQuery is a standalone component that can be used as a web backend service delivery. Part of the distribution a RemoteQuery servlet is provided with a default implementation.

See (docs/remotequery_web.md)
