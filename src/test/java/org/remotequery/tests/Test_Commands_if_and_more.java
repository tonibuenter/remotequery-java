package org.remotequery.tests;

import org.junit.Assert;
import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery;
import org.remotequery.TestCentral;
import org.remotequery.RemoteQuery.CommandNode;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.remotequery.RemoteQuery.ServiceEntry;
import org.remotequery.RemoteQuery.ServiceRepositoryHolder;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Test_Commands_if_and_more {

	private static Logger logger = LoggerFactory.getLogger(Test_Commands_if_and_more.class);

	public static class Prop {
		public String name;
		public String value;
	}

	@BeforeClass
	public static void beforeClass() throws Exception {
		TestCentral.init();
	}

	@Test
	public void test_command_if() throws Exception {

		//
		// COMMAND NODE
		//

		ServiceEntry se = ServiceRepositoryHolder.get().get("Test.Command.if");
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery.prepareCommandBlock(se);

		CommandNode cbExpected = new CommandNode("serviceRoot").append(new CommandNode("parameters"),
				new CommandNode("if").append(
						//
						new CommandNode("sql"), new CommandNode("sql"), new CommandNode("else"), new CommandNode("sql"),
						new CommandNode("end")
				//
				));

		RemoteQueryAssert.assertCommandNodeEquals(cbExpected, cb);
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
	public void test_command_if_elseOnly() throws Exception {

		String serviceId = "Test.Command.if_elseOnly";

		//
		// COMMAND NODE
		//

		ServiceEntry se = ServiceRepositoryHolder.get().get(serviceId);
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery.prepareCommandBlock(se);

		CommandNode cbExpected = new CommandNode("serviceRoot").append(
				//
				new CommandNode("put"),
				new CommandNode("if").append(
						//
						new CommandNode("else"),
						//
						new CommandNode("put"),
						//
						new CommandNode("end")
				//
				));

		RemoteQueryAssert.assertCommandNodeEquals(cbExpected, cb);
		logger.info(cb.toString());

		//
		// REQUEST RUN
		//

		Request request = new Request().setServiceId(serviceId);

		request.run();
		Assert.assertEquals("true", request.get("elseValue"));

		request.put("condition1", "hello");

		request.run();

		Assert.assertEquals("not reached else", request.get("elseValue"));
	}

	@Test
	public void test_command_switch() throws Exception {

		String serviceId = "Test.Command.switch";

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

		RemoteQueryAssert.assertCommandNodeEquals(cbExpected, cb);

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
		// COMMAND NODE
		//

		ServiceEntry se = ServiceRepositoryHolder.get().get(serviceId);
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery.prepareCommandBlock(se);

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

		RemoteQueryAssert.assertCommandNodeEquals(cbExpected, cb);

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
		// COMMAND NODE
		//

		ServiceEntry se = ServiceRepositoryHolder.get().get(serviceId);
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery.prepareCommandBlock(se);

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

		RemoteQueryAssert.assertCommandNodeEquals(cbExpected, cb);

		//
		// REQUEST RUN
		//

		Request request = new Request();

		(request.setServiceId(serviceId)).run();

		String total1 = request.get("total1");

		Assert.assertNotNull(total1);
		Assert.assertEquals("1", total1);

	}

	@Test
	public void test_command_foreach() throws Exception {

		String serviceId = "Test.Command.foreach";

		//
		// COMMAND NODE
		//

		ServiceEntry se = ServiceRepositoryHolder.get().get(serviceId);
		Assert.assertNotNull(se);
		CommandNode cb = RemoteQuery.prepareCommandBlock(se);

		CommandNode cbExpected = new CommandNode("serviceRoot").append(
				//
				new CommandNode("parameters"),
				//
				new CommandNode("foreach").append(
						//
						new CommandNode("sql"),
						//
						new CommandNode("end")),
				//
				new CommandNode("sql"),
				//
				new CommandNode("parameters"));

		RemoteQueryAssert.assertCommandNodeEquals(cbExpected, cb);

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
	public void test_command_while() throws Exception {

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
