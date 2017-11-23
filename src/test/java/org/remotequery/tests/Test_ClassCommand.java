package org.remotequery.tests;

import org.junit.Assert;
import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery.ProcessLog;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.remotequery.RemoteQuery.ServiceEntry;
import org.remotequery.RemoteQuery.ServiceRepositoryHolder;
import org.remotequery.RemoteQuery.Utils;

public class Test_ClassCommand {

	@BeforeClass
	public static void beforeClass() throws Exception {
		TestCentral.init();
	}

	@Test
	public void test_uuid() throws Exception {

		String uuid = new Request().addRole("SYSTEM").run("UUID.create").getColumn("uuid").get(0);

		if (Utils.isBlank(uuid)) {
			Assert.fail("no uuid created");
		}

	}

	@Test
	public void test_command_copy() throws Exception {
		Request request = new Request().setServiceId("Test.Command.copy");
		request.run();

		Assert.assertEquals("hello", request.get("name"));
		Assert.assertEquals("hello", request.get("name1"));
		Assert.assertEquals("hello2", request.get("name2"));
	}

	@Test
	public void test_command_backslash() throws Exception {
		ServiceEntry se = ServiceRepositoryHolder.get().get("Test.Command.backslash");
		Assert.assertNotNull(se);
		ProcessLog.Current(new ProcessLog());
		Request request = new Request().setServiceId("Test.Command.backslash");
		request.run();
		Assert.assertEquals("ok", request.get("semicolon"));
	}

	@Test
	public void test_command_example() throws Exception {
		ServiceEntry se = ServiceRepositoryHolder.get().get("Test.Command.example");
		Assert.assertNotNull(se);
		ProcessLog.Current(new ProcessLog());
		Request request = new Request().setServiceId("Test.Command.example");
		Result result = request.run();
		Assert.assertEquals("403", result.processLog.statusCode);
		request.addRole("APP_ADMIN");
		result = request.run();
		Assert.assertEquals("world", result.table.get(0).get(0));
	}

	// Test.Command.backslash

	@Test
	public void test_command_serviceid() throws Exception {
		ServiceEntry se = ServiceRepositoryHolder.get().get("Test.Command.serviceid");
		Assert.assertNotNull(se);
		ProcessLog.Current(new ProcessLog());
		Request request = new Request().setServiceId("Test.Command.serviceid");
		Result result = request.run();
		Assert.assertEquals("403", result.processLog.statusCode);
		request.addRole("APP_ADMIN");
		result = request.run();
		Assert.assertEquals("world", result.table.get(0).get(0));
	}

}
