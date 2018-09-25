package org.remotequery.tests;

import org.junit.Test;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Utils;

import junit.framework.Assert;

public class Test_resolveValue {

	@Test
	public void test1() {
		Request request = new Request();
		request.put("firstName", "Johansson");
		request.put("zero", "");
		request.put("null", null);
		//
		Assert.assertEquals("Johansson", Utils.resolve_value(":firstName", request));
		Assert.assertEquals("Johansson", Utils.resolve_value("  :firstName  ", request));
		Assert.assertEquals("firstName", Utils.resolve_value("firstName", request));
		Assert.assertEquals(":lastName", Utils.resolve_value("':lastName'", request));
		//
		Assert.assertEquals("", Utils.resolve_value(":zero", request));
		Assert.assertEquals("", Utils.resolve_value("  :zero  ", request));
		Assert.assertEquals("zero", Utils.resolve_value("zero", request));
		//
		Assert.assertEquals("", Utils.resolve_value(":null", request));
		Assert.assertEquals("", Utils.resolve_value("  :null  ", request));
		Assert.assertEquals("null", Utils.resolve_value("null", request));
		//
		Assert.assertEquals("'", Utils.resolve_value("'", request));
		//
		Assert.assertEquals("", Utils.resolve_value(null, request));
		Assert.assertEquals("", Utils.resolve_value(":lastName", request));
		//
		Assert.assertEquals("A B", Utils.resolve_value("   'A B' ", request));
		Assert.assertEquals("A B", Utils.resolve_value("'A B'", request));
		Assert.assertEquals("A B", Utils.resolve_value("A B", request));
		Assert.assertEquals("A B", Utils.resolve_value(" A B ", request));
		Assert.assertEquals(":firstName", Utils.resolve_value("':firstName'", request));
	}
}
