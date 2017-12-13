package org.remotequery.tests;

import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;

import junit.framework.Assert;

public class Test_HtmlText {

	@Test
	public void insertHtmlText() {
		String texttext = "<html>hallo</html>";
		Result result = new Request().setServiceId("HtmlText.save").put("htmlTextId", "test_01")
				.put("htmlText", texttext).addRole("APP_USER").run();
		Assert.assertEquals(1, result.rowsAffected);
		//
		result = new Request().setServiceId("HtmlText.get").put("htmlTextId", "test_01").addRole("APP_USER").run();
		Assert.assertEquals(texttext, result.getFirstRowAsMap().get("htmlText"));
	}

	@BeforeClass
	public static void beforeClass() throws Exception {
		TestCentral.init();
	}

}
