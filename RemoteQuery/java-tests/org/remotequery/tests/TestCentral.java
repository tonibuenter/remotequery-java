package org.remotequery.tests;

import java.io.InputStreamReader;
import java.io.Reader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.sql.Connection;
import java.sql.SQLException;

import javax.sql.DataSource;

import org.apache.commons.dbcp.BasicDataSource;
import org.apache.commons.io.IOUtils;
import org.remotequery.RemoteQuery2;
import org.remotequery.RemoteQuery2.ServiceRepositoryHolder;
import org.remotequery.RemoteQuery2.ServiceRepositorySql;
import org.remotequery.RemoteQueryUtils2;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class TestCentral {

	private static Logger logger = LoggerFactory.getLogger(TestCentral.class);
	private static DataSource dataSource;

	public static void init() throws Exception {

		if (TestCentral.dataSource != null) {
			logger.info("TestCentral already initialized");
			return;
		}

		//
		// EMBEDDED DERBY PARAMETERS
		//

		Path tmpDbPath = Files.createTempDirectory("remoteQueryTestDb");
		logger.info("Will try to temporary db in folder: " + tmpDbPath.toAbsolutePath());

		String dbdriver = "org.apache.derby.jdbc.EmbeddedDriver";
		String dburl = "jdbc:derby:" + tmpDbPath.toAbsolutePath() + "/test;create=true";
		String dbuserid = "derby";
		String dbpasswd = "derby";

		//
		// APACHE DATASOURCE
		//

		BasicDataSource basicDataSource = new BasicDataSource();
		basicDataSource.setDriverClassName(dbdriver);
		basicDataSource.setUrl(dburl);
		basicDataSource.setUsername(dbuserid);
		basicDataSource.setPassword(dbpasswd);
		Connection connection = basicDataSource.getConnection();
		logger.info("Got connection: " + connection);

		//
		// INIT DATABASE (create db objects, load bootstap service entry)
		//

		String sqlfileNames[] = { "init_01_bootstrap.sql" };
		for (String sqlfileName : sqlfileNames) {
			Reader input = new InputStreamReader(
					TestCentral.class.getResourceAsStream("/org/remotequery/tests/" + sqlfileName), "UTF-8");
			String sqlText = IOUtils.toString(input);
			input.close();
			RemoteQueryUtils2.processSqlText(connection, sqlText, sqlfileName);
		}

		//
		// INIT REMOTE QUERY (DataSourceEntry, ServiceRepository, ...)
		//

		logger.info("Try to init RemoteQuery.DataSourceEntry...");
		new RemoteQuery2.DataSourceEntry(basicDataSource);

		logger.info("Try to init RemoteQuery.ServiceRepository...");
		ServiceRepositoryHolder.setInstance(new ServiceRepositorySql(basicDataSource, "JGROUND.T_RQ_SERVICE"));

		//
		// LOAD RQ Services from rq.sql files
		//

		String rqSqlfileNames[] = { "init_02_commands.rq.sql", "init_10_system_services.rq.sql",
				"init_20_address_services.rq.sql" };
		for (String fileName : rqSqlfileNames) {
			Reader input = new InputStreamReader(
					TestCentral.class.getResourceAsStream("/org/remotequery/tests/" + fileName), "UTF-8");
			String rqSqlText = IOUtils.toString(input);
			input.close();
			RemoteQueryUtils2.processRqSqlText(connection, rqSqlText, "RQService.save", fileName);
		}

		connection.close();
		dataSource = basicDataSource;

	}

	public static void main(String[] args) throws Exception {
		init();
	}

	public static Connection getConnection() {
		try {
			if (dataSource == null) {
				init();
			}
			return dataSource.getConnection();
		} catch (Exception e) {
			logger.error(e.getMessage(), e);
		}
		return null;
	}

	public static void returnConnection(Connection connection) {
		try {
			connection.close();
		} catch (SQLException e) {
			logger.error(e.getMessage(), e);
		}
	}

}
