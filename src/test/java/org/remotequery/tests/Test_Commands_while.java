package org.remotequery.tests;

import org.junit.Assert;
import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery;
import org.remotequery.RemoteQuery.StatementNode;
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
		StatementNode cb = RemoteQuery.prepareCommandBlock(se);

		StatementNode cbExpected = new StatementNode("serviceRoot").append(
				//
				new StatementNode("sql"),
				//
				new StatementNode("sql"),
				//
				new StatementNode("sql"),
				//
				new StatementNode("set"),
				//
				new StatementNode("while").append(
						//
						new StatementNode("sql"),
						//
						new StatementNode("parameters"),
						//
						new StatementNode("end")),
				//
				new StatementNode("sql"));

		RemoteQueryAssert.assertStatementNodeEquals(cbExpected, cb);

		//
		// REQUEST RUN
		//

		Result result = new Request().setServiceId(serviceId).run();
		Assert.assertNotNull(result);
		Assert.assertEquals(0, result.size());

	}

}
