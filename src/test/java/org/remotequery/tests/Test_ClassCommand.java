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
	public void test_uuid_with_method() throws Exception {

		String uuid = new Request().addRole("SYSTEM").run("UUID.method.createNew").getColumn("uuid").get(0);

		if (Utils.isBlank(uuid)) {
			Assert.fail("no uuid created");
		}

	}

}
