package org.remotequery.tests;

import org.junit.Assert;
import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;
import org.remotequery.RemoteQuery.ServiceEntry;
import org.remotequery.RemoteQuery.ServiceRepositoryHolder;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * 
 * @author tonibuenter
 * 
 */
public class Test_Bootstrap {

	Logger logger = LoggerFactory.getLogger(Test_Bootstrap.class);

	@BeforeClass
	public static void beforeClass() throws Exception {
		TestCentral.init();
	}

	@Test
	public void logger_stuff() throws Exception {

		Logger logger = LoggerFactory.getLogger("org.remotequery.RemoteQuery2.sql");
		logger.error(logger.getName());
		Assert.assertEquals(true, logger.isDebugEnabled());
	}

	@Test
	public void serviceRepository_is_ok() throws Exception {
		ServiceEntry se = ServiceRepositoryHolder.get().get("RQService.select");
		Assert.assertNotNull(se);
		Assert.assertNotNull(se.serviceId);
		Assert.assertEquals("RQService.select", se.serviceId);
	}

	@Test
	public void sqlCommand_is_ok() throws Exception {
		new Request().setServiceId("RQService.select");
		ServiceEntry se = ServiceRepositoryHolder.get().get("RQService.select");
		Assert.assertNotNull(se);
		Assert.assertNotNull(se.serviceId);
		Assert.assertEquals("RQService.select", se.serviceId);

		//
		se = ServiceRepositoryHolder.get().get("RQService.delete");
		Assert.assertNotNull(se);
		Assert.assertNotNull(se.serviceId);
		Assert.assertEquals("RQService.delete", se.serviceId);
		Assert.assertTrue(se.getRoles().contains("SYSTEM"));
		Assert.assertTrue(!se.getRoles().contains("APP_USER"));
	}

	@Test
	public void setCommand_is_ok() throws Exception {

		ServiceEntry se = ServiceRepositoryHolder.get().get("RQService.select");
		Assert.assertNotNull(se);
		Assert.assertNotNull(se.serviceId);
		Assert.assertEquals("RQService.select", se.serviceId);

		//
		se = ServiceRepositoryHolder.get().get("RQService.delete");
		Assert.assertNotNull(se);
		Assert.assertNotNull(se.serviceId);
		Assert.assertEquals("RQService.delete", se.serviceId);
		Assert.assertTrue(se.getRoles().contains("SYSTEM"));
		Assert.assertTrue(!se.getRoles().contains("APP_USER"));
	}

	@Test
	public void AppProperties_check() throws Exception {

		Result result = new Request().setServiceId("AppProperties.get").put("name", "hello").run();
		Assert.assertNotNull(result.processLog);
		Assert.assertEquals("403", result.processLog.statusCode);

	}

}
