package org.remotequery.tests;

import org.junit.Assert;
import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery2;
import org.remotequery.RemoteQuery2.CommandNode;
import org.remotequery.RemoteQuery2.Request;
import org.remotequery.RemoteQuery2.Result;
import org.remotequery.RemoteQuery2.ServiceEntry;
import org.remotequery.RemoteQuery2.ServiceRepositoryHolder;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Test_Commands {

	private static Logger logger = LoggerFactory.getLogger(Test_Commands.class);

	public static class Prop {
		public String name;
		public String value;
	}

	@BeforeClass
	public static void beforeClass() throws Exception {
		TestCentral.init();
	}

	@Test
	public void test_command_set() throws Exception {
		ServiceEntry se = ServiceRepositoryHolder.getInstance().get("Test.Command.set");
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery2.prepareCommandBlock(se);
		Assert.assertEquals("serviceRoot", cb.cmd);
		Assert.assertEquals(2, cb.children.size());
		Assert.assertEquals("set", cb.children.get(0).cmd);
		Assert.assertEquals("sql", cb.children.get(1).cmd);
	}

	@Test
	public void test_command_if() throws Exception {
		ServiceEntry se = ServiceRepositoryHolder.getInstance().get("Test.Command.if");
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery2.prepareCommandBlock(se);

		CommandNode cbExpected = new CommandNode("serviceRoot").append(new CommandNode("parameters"),
				new CommandNode("if").append(
						//
						new CommandNode("then"), new CommandNode("sql"), new CommandNode("sql"),
						new CommandNode("else"), new CommandNode("sql"), new CommandNode("fi")
				//
				));

		assertSameStruct(cbExpected, cb);
		logger.info(cb.toString());

		//
		// REQUEST RUN
		//

		Request request = new Request().setServiceId("Test.Command.if");

		Result r = request.put("name", "hello").run();
		Assert.assertTrue(r.size() > 0);
		Assert.assertEquals("true", r.asList(Prop.class).get(0).value);
		request.put("name", "blabla");
		r = request.run();
		Assert.assertTrue(r.size() > 0);
		Assert.assertEquals("false", r.asList(Prop.class).get(0).value);
	}

	@Test
	public void test_command_foreach() throws Exception {

		String serviceId = "Test.Command.foreach";

		//
		// COMMAND
		//

		ServiceEntry se = ServiceRepositoryHolder.getInstance().get(serviceId);
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery2.prepareCommandBlock(se);

		CommandNode cbExpected = new CommandNode("serviceRoot").append(
				//
				new CommandNode("parameters"),
				//
				new CommandNode("foreach").append(
						//
						new CommandNode("do"),
						//
						new CommandNode("sql"),
						//
						new CommandNode("done")),
				//
				new CommandNode("sql"),
				//
				new CommandNode("parameters"));

		assertSameStruct(cbExpected, cb);

		//
		// REQUEST RUN
		//

		Request request = new Request();

		(request.setServiceId(serviceId)).run();

		String total1 = request.get("total1");
		String total2 = request.get("total2");

		Assert.assertNotNull(total1);
		Assert.assertNotNull(total2);

	}

	@Test
	public void test_command_switch() throws Exception {

		String serviceId = "Test.Command.switch";

		//
		// Command Block
		//

		ServiceEntry se = ServiceRepositoryHolder.getInstance().get(serviceId);
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery2.prepareCommandBlock(se);

		CommandNode cbExpected = new CommandNode("serviceRoot").append(
				//
				new CommandNode("sql"),
				//
				new CommandNode("parameters"),
				//
				new CommandNode("switch").append(
						//
						new CommandNode("case"),
						//
						new CommandNode("case"),
						//
						new CommandNode("case"),
						//
						new CommandNode("sql"),
						//
						new CommandNode("break"),
						//
						new CommandNode("case"),
						//
						new CommandNode("sql"),
						//
						new CommandNode("break"),
						//
						new CommandNode("case"),
						//
						new CommandNode("sql"),
						//
						new CommandNode("break"),
						//
						new CommandNode("case"),
						//
						new CommandNode("sql"),
						//
						new CommandNode("end")),
				//
				new CommandNode("sql"),
				//
				new CommandNode("parameters"),
				//
				new CommandNode("sql"));

		assertSameStruct(cbExpected, cb);

		//
		// REQUEST RUN
		//

		Request request = new Request();

		(request.setServiceId(serviceId)).run();

		String total1 = request.get("total1");

		Assert.assertNotNull(total1);
		Assert.assertEquals("2", total1);

	}

	@Test
	public void test_command_switch_empty() throws Exception {

		String serviceId = "Test.Command.switch_empty";

		//
		// Command Block
		//

		ServiceEntry se = ServiceRepositoryHolder.getInstance().get(serviceId);
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery2.prepareCommandBlock(se);

		CommandNode cbExpected = new CommandNode("serviceRoot").append(
				//
				new CommandNode("set"),
				//
				new CommandNode("sql"),
				//
				new CommandNode("set"),
				//
				new CommandNode("set"),
				//
				new CommandNode("switch").append(
						//
						new CommandNode("case"),
						//
						new CommandNode("case"),
						//
						new CommandNode("case"),
						//
						new CommandNode("sql"),
						//
						new CommandNode("break"),
						//
						new CommandNode("case"),
						//
						new CommandNode("sql"),
						//
						new CommandNode("break"),
						//
						new CommandNode("case"),
						//
						new CommandNode("sql"),
						//
						new CommandNode("end")),
				//
				new CommandNode("sql"),
				//
				new CommandNode("parameters"),
				//
				new CommandNode("sql"));

		assertSameStruct(cbExpected, cb);

		//
		// REQUEST RUN
		//

		Request request = new Request();

		(request.setServiceId(serviceId)).run();

		String total1 = request.get("total1");

		Assert.assertNotNull(total1);
		Assert.assertEquals("2", total1);

	}

	@Test
	public void test_command_switch_default() throws Exception {

		String serviceId = "Test.Command.switch_default";

		//
		// Command Block
		//

		ServiceEntry se = ServiceRepositoryHolder.getInstance().get(serviceId);
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery2.prepareCommandBlock(se);

		CommandNode cbExpected = new CommandNode("serviceRoot").append(
				//
				new CommandNode("set"),
				//
				new CommandNode("sql"),
				//
				new CommandNode("set"),
				//
				new CommandNode("set"),
				//
				new CommandNode("switch").append(
						//
						new CommandNode("case"),
						//
						new CommandNode("case"),
						//
						new CommandNode("case"),
						//
						new CommandNode("sql"),
						//
						new CommandNode("break"),
						//
						new CommandNode("default"),
						//
						new CommandNode("sql"),
						//
						new CommandNode("break"),
						//
						new CommandNode("case"),
						//
						new CommandNode("sql"),
						//
						new CommandNode("end")),
				//
				new CommandNode("sql"),
				//
				new CommandNode("parameters"),
				//
				new CommandNode("sql"));

		assertSameStruct(cbExpected, cb);

		//
		// REQUEST RUN
		//

		Request request = new Request();

		(request.setServiceId(serviceId)).run();

		String total1 = request.get("total1");

		Assert.assertNotNull(total1);
		Assert.assertEquals("1", total1);

	}

	public static void assertSameStruct(CommandNode expected, CommandNode actual) {
		Assert.assertEquals(expected.cmd, actual.cmd);
		Assert.assertEquals(expected.children.size(), actual.children.size());
		for (int i = 0; i < expected.children.size(); i++) {
			assertSameStruct(expected.children.get(i), actual.children.get(i));
		}

	}

}
