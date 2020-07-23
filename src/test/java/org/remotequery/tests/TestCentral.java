package org.remotequery.tests;

import org.apache.commons.dbcp.BasicDataSource;
import org.apache.commons.io.IOUtils;
import org.remotequery.RemoteQuery.DataSources;
import org.remotequery.RemoteQuery.ServiceRepositoryHolder;
import org.remotequery.RemoteQuery.ServiceRepositorySql;
import org.remotequery.RemoteQueryUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import javax.sql.DataSource;
import java.io.InputStreamReader;
import java.io.Reader;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Path;
import java.sql.Connection;

public class TestCentral {

  private static Logger logger = LoggerFactory.getLogger(TestCentral.class);

  static {
    System.out.println("CLASSPATH " + System.getProperty("java.class.path"));
  }

  private static DataSource dataSource;

  private static String sqlfileNames[] = {"init_01_bootstrap.sql", "address.sql", "address_testdata.sql",
          "html-text.sql"};

  private static String rqSqlfileNames[] = {"init_02_commands.rq.sql", "init_03_includes.rq.sql",
          "init_10_system_services.rq.sql", "address.rq.sql", "html-text.rq.sql"};

  public static void init() throws Exception {

    if (TestCentral.dataSource != null) {
      logger.info("TestCentral already initialized, will do nothing...");
      return;
    }

    //
    // 1. Database : Create a temporary directory for a Appache Derby DB
    // with with embedded driver
    //

    Path tmpDbPath = Files.createTempDirectory("remoteQueryTestDb");
    logger.info("Will try to temporary db in folder: " + tmpDbPath.toAbsolutePath());

    String dbdriver = "org.apache.derby.jdbc.EmbeddedDriver";
    String dburl = "jdbc:derby:" + tmpDbPath.toAbsolutePath() + "/test;create=true";
    String dbuserid = "derby";
    String dbpasswd = "derby";

    //
    // 2. DataSource : Create a data source object with Apache
    // BasicDataSource
    //

    BasicDataSource basicDataSource = new BasicDataSource();
    basicDataSource.setDriverClassName(dbdriver);
    basicDataSource.setUrl(dburl);
    basicDataSource.setUsername(dbuserid);
    basicDataSource.setPassword(dbpasswd);
    Connection connection = basicDataSource.getConnection();
    logger.info("Connection from Apache BasicDataSource: " + connection);

    //
    // 3. DB Objects : Create schema and tables, insert bootstrap service
    // entry
    //

    for (String sqlfileName : sqlfileNames) {
      URL url = TestCentral.class.getClassLoader().getResource( sqlfileName);
      if (url == null) {
        logger.error("Did not find: " + sqlfileName);
        continue;
      }
      Reader input = new InputStreamReader(
              TestCentral.class.getClassLoader().getResourceAsStream(sqlfileName), "UTF-8");
      String sqlText = IOUtils.toString(input);
      input.close();
      RemoteQueryUtils.processSqlText(connection, sqlText, sqlfileName);
    }
    connection.close();

    //
    // 4. Initialize RemoteQuery : Register data source, create and register
    // an sql service
    // repository with the service table JGROUND.T_RQ_SERVICE
    //

    logger.info("Register default data source...");
    DataSources.register(basicDataSource);

    logger.info("Register default ...");
    ServiceRepositorySql serviceRepository = new ServiceRepositorySql(basicDataSource, "JGROUND.T_RQ_SERVICE");
    ServiceRepositoryHolder.set(serviceRepository);

    //
    // 5. Load RQ Services : Read application's service definitions from
    // rq.sql files
    //

    for (String fileName : rqSqlfileNames) {
       URL url = TestCentral.class.getClassLoader().getResource( fileName);
      if (url == null) {
        logger.error("Did not find: " + fileName);
        continue;
      }
      try (Reader input = new InputStreamReader(
              TestCentral.class.getClassLoader().getResourceAsStream( fileName), "UTF-8");) {
        String rqSqlText = IOUtils.toString(input);
        RemoteQueryUtils.processRqSqlText(rqSqlText, "RQService.save", fileName);
      } catch (Exception e) {
        logger.error(e.getMessage(), e);
      }

    }

    dataSource = basicDataSource;

  }

}
