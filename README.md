
![Remote Query](docs/remote-query.png)


# RemoteQuery (RQ)

## An efficient middleware for friends of SQL, Java and more ...


RemoteQuery (RQ) is a simple but powerful tool for secure service creation using SQL 
queries and Java as first class citizen. 

## Just another 'Hibernate', 'JPA', ... ?

Yes indeed, but a much more efficient one. RQ does not introduce another level of complexity
and uncertainty. RQ does not provide a sophisticated and magic mapping and optimization for DB access. 

We strongly believe that the database and the SQL query developer can build the best queries. 

Further, after years of the object to relational mapping, it seems obvious that a good relational database design is very important and should not be restrained by an additional layer such as Hibernate and JPAs.

After starting with RQ the Java Code was reduced even using JPA and Hibernate by 80 per cent. On the other side, the RQ code of which 90 per cent are just SQL statements, has been proven to be easily maintainable and testable.

## The highlights:

+ The most simple RQ service is an SQL statement
+ Any RQ service can be protected with a list of roles
+ SQL parameters are directly mapped into the query (still preventing SQL injection)
+ A Java class (or Python function) can be used for a RQ service
+ A simple yet powerfull object-relational support without obfuscation is provided for the Java and Python coding [OR Support](docs/object_relational_support.md)


## Example RemoteQuery Web

Let us assume we have the following RQ service entry: 

```
SERVICE_ID   : Address.search
ROLES        : APP_USER

select 
  FIRST_NAME, LAST_NAME, CITY 
from T_ADDRESS 
where 
  FIRST_NAME like :nameFilter 
or 
  LAST_NAME like :nameFilter
```

The the following URL:

```
http://hostname/remoteQuery/Address.search?nameFilter=Jo%
```
 
will return - for users with the APP_USER role - the following JSON:

```
{
  "header" : ["firstName", "lastName", "city"],
  "table" : [
        ["John", "Maier",  "Zuerich"],
        ["Mary", "Johannes", "Zuerich"]
  ]
}
```

## Example Standalone RemoteQuery

```java
public static class Address {
  public String firstName;
  public String lastName;
  public String city;
}

Result result = 
    new Request()
       .setServiceId("Address.search").put("nameFilter", "Jo%")
       .addRole("APP_USER")
       .run();

// convert to a POJO
List<Address> list = result.asList(Address.class);
```


## Quick Start

Download or clone this repository. The repository is a Eclipse project (`Java Project`). 
It expects Java 8, but RemoteQuery runs with Java 7 as well.

Here some hints for the directory layout of this repository:

* RQ main : `src/main/java/` with `java-libs/` is needed for RQ 
* JUnit tests : `src/test/java/` with `java-test-libs/` together with RQ main
* Web : `src/web/java/`, `src/web/webapp/` with `java-web-libs/` together with Unit tests
* Documentation : `docs/`

### Standalone

Run the JUnit classes in `src/test/java` to see how the RQ runs standalone.
`TestCentral.java` creates an apache-derby database in the temp directory (see: `Files.createTempDirectory`) with test tables and services. DB object and services are defined in the corresponding `*.sql` and `*.rq.sql` files.

### Java Web Container

For running RQ as part of a Java web container just start the `remote-query-start-web` launch configuration (main class : `StartJetty.java`). Open the browser on http://localhost:8080. 

*Explanation*: An embedded Jetty server is started. The web integration of RQ is done by the `RemoteQueryWeb.java` servlet. The servlet is rather simple. It works fine as a starting point for a web project that applies is own user authentication and authorization.

More details:
[RemoteQuery Test Setup](docs/test_setup.md)

## Remote Query Documentation

* [RemoteQuery User Manual](docs/user_manual.md)
* [RemoteQuery Reference Manual](docs/reference_manual.md)

