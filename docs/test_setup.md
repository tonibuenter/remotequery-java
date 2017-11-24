# Remote Query Quickstart

## Eclipse project

Download or clone this repository. The repository is a regular `Eclipse Java Project`.

Run JUnit in `src/test/java`. The code in `TestCentral.java` shows how RemoteQuery is initialized:


```java
 
    //
    // 1. Database : Create a temporary directory for a Apache Derby DB with with embedded driver
    //

    Path tmpDbPath = Files.createTempDirectory("remoteQueryTestDb");
    logger.info("Will try to temporary db in folder: " + tmpDbPath.toAbsolutePath());

    String dbdriver = "org.apache.derby.jdbc.EmbeddedDriver";
    String dburl = "jdbc:derby:" + tmpDbPath.toAbsolutePath() + "/test;create=true";
    String dbuserid = "derby";
    String dbpasswd = "derby";

    //
    // 2. DataSource : Create a data source object with Apache BasicDataSource
    //

    BasicDataSource basicDataSource = new BasicDataSource();
    basicDataSource.setDriverClassName(dbdriver);
    basicDataSource.setUrl(dburl);
    basicDataSource.setUsername(dbuserid);
    basicDataSource.setPassword(dbpasswd);
    Connection connection = basicDataSource.getConnection();
    logger.info("Connection from Apache BasicDataSource: " + connection);

    //
    // 3. DB Objects : Create schema and tables, insert bootstrap service entry
    //

    for (String sqlfileName : sqlfileNames) {
      Reader input = new InputStreamReader(
          TestCentral.class.getResourceAsStream("/org/remotequery/tests/" + sqlfileName), "UTF-8");
      String sqlText = IOUtils.toString(input);
      input.close();
      RemoteQueryUtils.processSqlText(connection, sqlText, sqlfileName);
    }

    //
    // 4. Initialize RemoteQuery : Register data source, create and register an sql service
    // repository with the service table JGROUND.T_RQ_SERVICE
    //

    logger.info("Register default data source...");
    DataSources.register(basicDataSource);

    logger.info("Register default ...");
    ServiceRepositorySql serviceRepository = new ServiceRepositorySql(basicDataSource, "JGROUND.T_RQ_SERVICE");
    ServiceRepositoryHolder.set(serviceRepository);

    //
    // 5. Load RQ Services : Read application's service definitions from rq.sql files
    //

    for (String fileName : rqSqlfileNames) {
      Reader input = new InputStreamReader(
          TestCentral.class.getResourceAsStream("/org/remotequery/tests/" + fileName), "UTF-8");
      String rqSqlText = IOUtils.toString(input);
      input.close();
      RemoteQueryUtils.processRqSqlText(connection, rqSqlText, "RQService.save", fileName);
    }

 
```


#### 1. Database 

The database used here is an Apache Derby DB that uses a temporary directory and applies the embedded driver. In a productive environment the database is provided via a DataSource. So this step might not be needed.

#### 2. DataSource

As RemoteQuery works in a multi-threaded environment is requires a Java DataSource object. Just for easy testing an Apache 
BasicDataSource is used. In a productive environment the DataSource object might be provided over `JNDI` or injected via CDI.

#### 3. DB Objects

Just for testing, all DB objects such as schema and tables are created. In a real-world setting this would all be done only once. A  first bootstrap service `RQService.save` is inserted for using the convenient loading of RQ services (see step 5)  into the RQ service table. In our test setup we use the table `JGROUND.T_RQ_SERVICE` as service table. The DDL is like:

```
create table JGROUND.T_RQ_SERVICE (
   SERVICE_ID varchar(256),
   STATEMENTS varchar(4000),
   ROLES varchar(4000),
   DATASOURCE varchar(512),
   primary key (SERVICE_ID)
);
```

#### 4. Initialize RemoteQuery

After the preparing steps 1 to 3, the RemoteQuery system will now be initialized with:
* a default datasource and
* an RQ service repository

In this test setup, an instance of the SQL based service repository `ServiceRepositorySql` is used. The class `ServiceRepositorySql` is provided by RQ. The instantiation requires a datasource and a DB object such as a table or a view. The following columns have to be selectable for the creation of a service entry:

* _SERVICE\_ID_ : The service id
* _STATEMENTS_ : RQ statements
* _ROLES_ : Comma separated list of role names, could be empty
* _DATASOURCE_ : Name of the datasource, could be empty for default datasource

 The `ServiceRepositorySql` will call an sql statement like `select * from service_table where SERVICE_ID = :serviceId` for the creation of a service entry.



 ####  5. Load RQ Services
 
 The last section in this test setup is reading in all services from the rq.sql files.
 Further details on [RQ SQL Files](rq_sql_files.md)


