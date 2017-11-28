package org.remotequery.tests;

import org.junit.Assert;
import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery;
import org.remotequery.RemoteQuery.CommandNode;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.remotequery.RemoteQuery.ServiceEntry;
import org.remotequery.RemoteQuery.ServiceRepositoryHolder;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Test_Commands_while {

	private static Logger logger = LoggerFactory.getLogger(Test_Commands_while.class);

	public static class Prop {
		public String name;
		public String value;
	}

	@BeforeClass
	public static void beforeClass() throws Exception {
		TestCentral.init();
	}

	@Test
	public void test_command_while() throws Exception {
		
		logger.debug("start");

		String serviceId = "Test.Command.while";

		//
		// COMMAND NODE
		//

		ServiceEntry se = ServiceRepositoryHolder.get().get(serviceId);
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery.prepareCommandBlock(se);

		CommandNode cbExpected = new CommandNode("serviceRoot").append(
				//
				new CommandNode("sql"),
				//
				new CommandNode("sql"),
				//
				new CommandNode("sql"),
				//
				new CommandNode("set"),
				//
				new CommandNode("while").append(
						//
						new CommandNode("sql"),
						//
						new CommandNode("parameters"),
						//
						new CommandNode("end")),
				//
				new CommandNode("sql"));

		RemoteQueryAssert.assertCommandNodeEquals(cbExpected, cb);

		//
		// REQUEST RUN
		//

		Result result = new Request().setServiceId(serviceId).run();
		Assert.assertNotNull(result);
		Assert.assertEquals(0, result.size());

	}

}
