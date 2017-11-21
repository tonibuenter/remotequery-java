package org.remotequery.tests;

import java.io.InputStreamReader;
import java.io.Reader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.sql.Connection;

import javax.sql.DataSource;

import org.apache.commons.dbcp.BasicDataSource;
import org.apache.commons.io.IOUtils;
import org.remotequery.RemoteQuery.DataSources;
import org.remotequery.RemoteQuery.ServiceRepositoryHolder;
import org.remotequery.RemoteQuery.ServiceRepositorySql;
import org.remotequery.RemoteQueryUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class TestCentral {

	private static Logger logger = LoggerFactory.getLogger(TestCentral.class);

	private static DataSource dataSource;

	private static String sqlfileNames[] = { "init_01_bootstrap.sql", "init_20_address_db.sql",
			"init_21_address_testdata.sql" };

	private static String rqSqlfileNames[] = { "init_02_commands.rq.sql", "init_03_includes.rq.sql",
			"init_10_system_services.rq.sql", "init_22_address_services.rq.sql" };

	public static void init() throws Exception {

		if (TestCentral.dataSource != null) {
			logger.info("TestCentral already initialized");
			return;
		}

		//
		// 1. Database : Derby with embedded driver
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
		// 4. Initialize RemoteQuery : Register data source and service
		// repository
		//

		logger.info("Register default data source...");
		DataSources.register(basicDataSource);

		logger.info("Register default ...");
		ServiceRepositorySql serviceRepository = new ServiceRepositorySql(basicDataSource, "JGROUND.T_RQ_SERVICE");
		ServiceRepositoryHolder.set(serviceRepository);

		//
		// 5. Load RQ Services : Read service definitions from rq.sql files
		//

		for (String fileName : rqSqlfileNames) {
			Reader input = new InputStreamReader(
					TestCentral.class.getResourceAsStream("/org/remotequery/tests/" + fileName), "UTF-8");
			String rqSqlText = IOUtils.toString(input);
			input.close();
			RemoteQueryUtils.processRqSqlText(connection, rqSqlText, "RQService.save", fileName);
		}

		connection.close();
		dataSource = basicDataSource;

	}

}
