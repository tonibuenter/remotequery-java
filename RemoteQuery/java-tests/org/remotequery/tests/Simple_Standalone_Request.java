package org.remotequery.tests;

import java.sql.Connection;

import junit.framework.Assert;

import org.junit.AfterClass;
import org.junit.BeforeClass;
import org.junit.Test;

import static org.remotequery.RemoteQuery.*;

import org.remotequery.RemoteQuery.DataSourceEntry;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.remotequery.RemoteQuery.ServiceEntry;
import org.remotequery.RemoteQuery.ServiceRepository;
import org.remotequery.RemoteQuery.Utils;

/**
 * 
 * @author tonibuenter
 * 
 */
public class Simple_Standalone_Request {

	private static Connection connection;

	@BeforeClass
	public static void initAll() throws Exception {
		connection = TestCentral.createTestConnection();
		Utils.runQuery(connection, "create schema TEST_1");
		Utils
		    .runQuery(
		        connection,
		        "create table TEST_1.T_PERSON(FIRST_NAME varchar(1024), LAST_NAME varchar(1024))");
	}

	@AfterClass
	public static void closeAll() throws Exception {
		Utils.runQuery(connection, "drop table TEST_1.T_PERSON");
		Utils.runQuery(connection, "drop schema TEST_1 restrict");
		Utils.closeQuietly(connection);
	}

	@Test
	public void testConnection() throws Exception {

		//
		// 1. DataSource creation
		//

		// 1.1 DataSource
		TestDataSource ds = new TestDataSource(connection);
		Assert.assertNotNull(ds);

		//
		// 2 DataSources registration
		//
		new DataSourceEntry(ds);

		//
		// 3 ServiceRepositoryJson setup
		//
		ServiceRepository sr = new ServiceRepository(
		    "["
		        + "{'serviceId':'insertAddress','statements':'insert into TEST_1.T_PERSON(FIRST_NAME,LAST_NAME) values (:firstName,:lastName)'}"
		        + ","
		        + "{'serviceId':'selectAddresses','statements':'select * from TEST_1.T_PERSON'}"
		        + "]");
		ServiceEntry se = sr.get("insertAddress");
		Assert.assertEquals("insertAddress", se.getServiceId());

		//
		// 4 Create And Run Process
		//

		// 4.1 Insert
		Request request = new Request();
		request.setServiceId("insertAddress");
		request.setUserId("testuser");
		request.put("firstName", "Sophie");
		request.put("lastName", "McGraham");
		MainQuery task = new MainQuery();
		task.run(request);

		// 4.1 Select
		request = new Request();
		request.setServiceId("selectAddresses");
		Result result = new MainQuery().run(request);
		Assert.assertEquals(1, result.getTotalCount());

	}
}
