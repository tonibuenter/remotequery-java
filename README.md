
![Under Construction](https://docs.google.com/drawings/d/e/2PACX-1vQsPlanMiS2yX50Qxo3qR4Eb2di8tXoW3387qDHBcaJtvpu18WlyTY-k9Gfcvk8bCVCEhC9akweRta2/pub?w=378&amp;h=94)



# RemoteQuery (RQ)

## An efficient service middleware for friends of SQL, Java and more ...


RemoteQuery (RQ) is a simple but powerful tool for secure service creation using SQL queries and Java as first class citizen. 

The highlights:

+ The most simple RQ service: an SQL statement
+ A Java class implementing the RQ IQuery.process method is a RQ service
+ Any RQ service can be protected with a list of roles
+ The RQ Servlet directly maps HTTP parameters to SQ named parameters
+ The most simple yet powerfull object-relational support

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
 
will return for users with the APP_USER role the following JSON:

```json
{
  "header" : ["firstName", "lastName", "city"],
  "table" : [
        ["John", "Maier",  "Zuerich"],
        ["Mary", "Johannes", "Zuerich"]
  ]
}
```


## Example 2: Standalone RemoteQuery


```java
public static class Address {
  public String firstName;
  public String lastName;
  public String city;
}

Result result = new Request().setServiceId("Address.search").put("nameFilter", "Jo%").addRole("APP_USER").run();

// convert to a POJO
List<Address> list = result.asList(Address.class);
```


## Quick Start

Download or clone this repository. The repository is a Eclipse project (`Java Project`). 
It expects Java 8, but RemoteQuery runs with Java 7 as well.

Here some structural hints:

* RQ main : `src/main/java` with `java-libs` is needed for RQ 
* JUnit tests : `src/test/java` with `java-test-libs` together with RQ main
* Web : `src/web/java`, `src/web/webapp` with `java-web-libs` together with Unit tests

### Standalone

Run the JUnit classes in `src/test/java` to see how the RQ runs standalone.
`TestCentral.java` creates an apache-derby database in the temp directory (see: `Files.createTempDirectory`) with test tables and services. DB object and services are defined in the corresponding `sql` and `rq.sql` files.

### Java Web Container

For running RQ as part of a Java web container just start the `remote-query-start-web` launch configuration (main class : `StartJetty.java`). Open the browser on http://localhost:8080. An embedded Jetty server is started. The integration of RQ is done by the `RemoteQueryWeb.java` servlet. The servlet rather simple. Please apply security such as user authentication and authorization.


## Remote Query Documentation

[RemoteQuery User Manual](docs/user_manual.md)
[RemoteQuery Reference Manual](docs/reference_manual.md)

