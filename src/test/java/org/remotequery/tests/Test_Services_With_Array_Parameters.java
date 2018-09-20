package org.remotequery.tests;

import java.util.List;

import org.junit.Assert;
import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery.Request;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Test_Services_With_Array_Parameters {

	private static Logger logger = LoggerFactory.getLogger(Test_Services_With_Array_Parameters.class);

	@BeforeClass
	public static void beforeClass() throws Exception {
		TestCentral.init();
	}

	@Test
	public void test_set_array_parameters() throws Exception {

		logger.debug("start");

		Request request = new Request();

		request.setServiceId("Test.Command.arrayParameter").run();

		Assert.assertEquals("New York,Paris,London,Peking", request.get("names"));
		Assert.assertEquals("New York,Paris,London,Peking", request.get("namesCopy"));
		Assert.assertEquals("", request.get("namesCopy2"));

	}

	@Test
	public void test_array_parameters() throws Exception {

		logger.debug("start");

		Request request = new Request();

		request.setServiceId("Address.selectWithNamesArray");
		request.addRole("ADDRESS_READER");
		request.put("names", "Anna,Ralf,Sara");

		List<Address> list = request.run().asList(Address.class);

		Assert.assertEquals(3, list.size());
		Assert.assertEquals("7", list.get(0).addressId);
		Assert.assertEquals("8", list.get(1).addressId);
		Assert.assertEquals("9", list.get(2).addressId);

	}

}
