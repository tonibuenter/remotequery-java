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
		StatementNode cb = RemoteQuery.prepareCommandBlock(se);

		StatementNode cbExpected = new StatementNode("serviceRoot").append(new StatementNode("parameters"),
				new StatementNode("if").append(
						//
						new StatementNode("sql"), new StatementNode("sql"), new StatementNode("else"), new StatementNode("sql"),
						new StatementNode("end")
				//
				));

		RemoteQueryAssert.assertStatementNodeEquals(cbExpected, cb);
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
		StatementNode cb = RemoteQuery.prepareCommandBlock(se);

		StatementNode cbExpected = new StatementNode("serviceRoot").append(
				//
				new StatementNode("put"),
				new StatementNode("if").append(
						//
						new StatementNode("else"),
						//
						new StatementNode("put"),
						//
						new StatementNode("end")
				//
				));

		RemoteQueryAssert.assertStatementNodeEquals(cbExpected, cb);
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
		StatementNode cb = RemoteQuery.prepareCommandBlock(se);

		StatementNode cbExpected = new StatementNode("serviceRoot").append(
				//
				new StatementNode("sql"),
				//
				new StatementNode("parameters"),
				//
				new StatementNode("switch").append(
						//
						new StatementNode("case"),
						//
						new StatementNode("case"),
						//
						new StatementNode("case"),
						//
						new StatementNode("sql"),
						//
						new StatementNode("break"),
						//
						new StatementNode("case"),
						//
						new StatementNode("sql"),
						//
						new StatementNode("break"),
						//
						new StatementNode("case"),
						//
						new StatementNode("sql"),
						//
						new StatementNode("break"),
						//
						new StatementNode("case"),
						//
						new StatementNode("sql"),
						//
						new StatementNode("end")),
				//
				new StatementNode("sql"),
				//
				new StatementNode("parameters"),
				//
				new StatementNode("sql"));

		RemoteQueryAssert.assertStatementNodeEquals(cbExpected, cb);

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
		StatementNode cb = RemoteQuery.prepareCommandBlock(se);

		StatementNode cbExpected = new StatementNode("serviceRoot").append(
				//
				new StatementNode("set"),
				//
				new StatementNode("sql"),
				//
				new StatementNode("set"),
				//
				new StatementNode("set"),
				//
				new StatementNode("switch").append(
						//
						new StatementNode("case"),
						//
						new StatementNode("case"),
						//
						new StatementNode("case"),
						//
						new StatementNode("sql"),
						//
						new StatementNode("break"),
						//
						new StatementNode("case"),
						//
						new StatementNode("sql"),
						//
						new StatementNode("break"),
						//
						new StatementNode("case"),
						//
						new StatementNode("sql"),
						//
						new StatementNode("end")),
				//
				new StatementNode("sql"),
				//
				new StatementNode("parameters"),
				//
				new StatementNode("sql"));

		RemoteQueryAssert.assertStatementNodeEquals(cbExpected, cb);

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
		StatementNode cb = RemoteQuery.prepareCommandBlock(se);

		StatementNode cbExpected = new StatementNode("serviceRoot").append(
				//
				new StatementNode("set"),
				//
				new StatementNode("sql"),
				//
				new StatementNode("set"),
				//
				new StatementNode("set"),
				//
				new StatementNode("switch").append(
						//
						new StatementNode("case"),
						//
						new StatementNode("case"),
						//
						new StatementNode("case"),
						//
						new StatementNode("sql"),
						//
						new StatementNode("break"),
						//
						new StatementNode("default"),
						//
						new StatementNode("sql"),
						//
						new StatementNode("break"),
						//
						new StatementNode("case"),
						//
						new StatementNode("sql"),
						//
						new StatementNode("end")),
				//
				new StatementNode("sql"),
				//
				new StatementNode("parameters"),
				//
				new StatementNode("sql"));

		RemoteQueryAssert.assertStatementNodeEquals(cbExpected, cb);

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
		StatementNode cb = RemoteQuery.prepareCommandBlock(se);

		StatementNode cbExpected = new StatementNode("serviceRoot").append(
				//
				new StatementNode("parameters"),
				//
				new StatementNode("foreach").append(
						//
						new StatementNode("sql"),
						//
						new StatementNode("end")),
				//
				new StatementNode("sql"),
				//
				new StatementNode("parameters"));

		RemoteQueryAssert.assertStatementNodeEquals(cbExpected, cb);

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


}
