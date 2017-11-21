package org.remotequery.tests;

import java.util.List;

import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;

import junit.framework.Assert;

public class Test_Address {

	public static class Address {
		public String firstName;
		public String lastName;
		public String city;
	}

	@Test
	public void testCreateNewUser() {
		Result result = new Request().setServiceId("Address.search").put("nameFilter", "Jo%").addRole("APP_USER").run();
		// convert to a POJO
		List<Address> list = result.asList(Address.class);
		Assert.assertEquals(2, list.size());
	}

	@BeforeClass
	public static void beforeClass() throws Exception {
		TestCentral.init();
	}

}
