package org.remotequery.tests;

import java.sql.Connection;
import java.sql.SQLException;

public class TestCentral {

	public static Connection createTestConnection() throws SQLException {

		Connection connection = java.sql.DriverManager
		    .getConnection(
		        "jdbc:derby://localhost:1528//Users/tonibuenter/tmp/TESTDB;create=true",
		        "derby", "derby");
		return connection;

	}

}
