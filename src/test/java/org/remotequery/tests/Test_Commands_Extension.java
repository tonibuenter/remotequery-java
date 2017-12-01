package org.remotequery.tests;

import org.junit.Assert;
import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery;
import org.remotequery.RemoteQuery.StatementNode;
import org.remotequery.RemoteQuery.ICommand;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.remotequery.RemoteQuery.ServiceEntry;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Test_Commands_Extension {

	public static class CreateNewUser implements ICommand {
		public Result run(Request request, Result currentResult, StatementNode statementNode, ServiceEntry serviceEntry) {
			Logger logger = LoggerFactory.getLogger(Test_Bootstrap.class);
			request.put("newUser", statementNode.parameter);
			logger.warn("Reached Command Extension " + CreateNewUser.class.getName());
			return currentResult;
		}
	}

	@Test
	public void testCreateNewUser() {
		RemoteQuery.Commands.Registry.put("create-new-user", new CreateNewUser());

		Request request = new Request().setServiceId("Test.Command.extension_CreateNewUser");

		request.run();

		String user = request.get("newUser");

		Assert.assertEquals("John Smith", user);

	}
	
	
	@BeforeClass
	public static void beforeClass() throws Exception {
		TestCentral.init();
	}


}
