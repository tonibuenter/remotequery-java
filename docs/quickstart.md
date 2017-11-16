# Remote Query Quickstart

## Eclipse Project

Download or clone this repository and you get Eclipse project (Dynamic Web).

Run JUnit in `src/test/java`. The code in `TestCentral.java` shows how RemoteQuery is initialized:

1. **DataBase** : Derby with embedded driver
2. **DataSource** : Apache BasicDataSource
3. **DB Objects** : Create tables, insert bootstrap service entry
4. **Initialize RemoteQuery** : Register data source and service repository
5. **Load RQ Services** : Read service definition from rq.sql files


```java
 
      //
      // 1. DataBase : Derby with embedded driver
      //

      Path tmpDbPath = Files.createTempDirectory("remoteQueryTestDb");
      logger.info("Will try to temporary db in folder: " + tmpDbPath.toAbsolutePath());

      String dbdriver = "org.apache.derby.jdbc.EmbeddedDriver";
      String dburl = "jdbc:derby:" + tmpDbPath.toAbsolutePath() + "/test;create=true";
      String dbuserid = "derby";
      String dbpasswd = "derby";

      //
      // 2. DataSource : Apache BasicDataSource
      //

      BasicDataSource basicDataSource = new BasicDataSource();
      basicDataSource.setDriverClassName(dbdriver);
      basicDataSource.setUrl(dburl);
      basicDataSource.setUsername(dbuserid);
      basicDataSource.setPassword(dbpasswd);
      Connection connection = basicDataSource.getConnection();
      logger.info("Got connection: " + connection);

      //
      // 3. DB Objects : Create tables, insert bootstrap service entry
      //

      for (String sqlfileName : sqlfileNames) {
        Reader input = new InputStreamReader(
            TestCentral.class.getResourceAsStream("/org/remotequery/tests/" + sqlfileName), "UTF-8");
        String sqlText = IOUtils.toString(input);
        input.close();
        RemoteQueryUtils.processSqlText(connection, sqlText, sqlfileName);
      }

      //
      // 4. Initialize RemoteQuery : Register data source and service repository
      //

      logger.info("Register default data source...");
      DataSources.register(basicDataSource);

      logger.info("Register default ...");
      ServiceRepositorySql serviceRepository = new ServiceRepositorySql(basicDataSource, "JGROUND.T_RQ_SERVICE");
      ServiceRepositoryHolder.set(serviceRepository);

      //
      // 5. Load RQ Services : Read service definition from rq.sql files
      //

      for (String fileName : rqSqlfileNames) {
        Reader input = new InputStreamReader(
            TestCentral.class.getResourceAsStream("/org/remotequery/tests/" + fileName), "UTF-8");
        String rqSqlText = IOUtils.toString(input);
        input.close();
        RemoteQueryUtils.processRqSqlText(connection, rqSqlText, "RQService.save", fileName);
      }


 
```


## Remarks to 4. Initialize RemoteQuery

In case of multiple data sources they can be registered by a name. The given name for registration corresponds with the service entry data source attribute. In case of a registration without a name or empty name or **RemoteQuery.DEFAULT\_DATASOURCE** as name, the data source is used as default data source for service entries also with empty dataSource attribute or with **RemoteQuery.DEFAULT\_DATASOURCE**.

### Class ServiceRepositorySql

The class ServiceRepositorySql is a convenience class for reading  service entries from a table or view. 

It requires a data source and a table or view name:
```java
new ServiceRepositorySql(basicDataSource, "JGROUND.T_RQ_SERVICE")
```

In the above example the table JGROUND.T\_RQ\_SERVICE is provided.

This table or view is expected to return the following columns with the query:

```sql
select * from JGROUND.T_RQ_SERVICE where SERVICE_ID = ?
```

The columns expected are

* SERVICE_ID : Service id
* STATEMENTS : RQ statements
* ROLES : Comma separated list of role names, could be empty
* DATASOURCE : Name of the datasource, could be empty


The table used in the example above is:

```sql
create table JGROUND.T_RQ_SERVICE (
   SERVICE_ID varchar(256),
   STATEMENTS varchar(4000),
   ROLES varchar(4000),
   DATASOURCE varchar(512),
   primary key (SERVICE_ID)
);

```





