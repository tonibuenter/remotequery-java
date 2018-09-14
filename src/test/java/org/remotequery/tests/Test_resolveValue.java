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
		Assert.assertEquals("Johansson", Utils.resolveValue(":firstName", request));
		Assert.assertEquals("Johansson", Utils.resolveValue("  :firstName  ", request));
		Assert.assertEquals("firstName", Utils.resolveValue("firstName", request));
		Assert.assertEquals(":lastName", Utils.resolveValue("':lastName'", request));
		//
		Assert.assertEquals("", Utils.resolveValue(":zero", request));
		Assert.assertEquals("", Utils.resolveValue("  :zero  ", request));
		Assert.assertEquals("zero", Utils.resolveValue("zero", request));
		//
		Assert.assertEquals("", Utils.resolveValue(":null", request));
		Assert.assertEquals("", Utils.resolveValue("  :null  ", request));
		Assert.assertEquals("null", Utils.resolveValue("null", request));
		//
		Assert.assertEquals("'", Utils.resolveValue("'", request));
		//
		Assert.assertEquals("", Utils.resolveValue(null, request));
		Assert.assertEquals("", Utils.resolveValue(":lastName", request));
		//
		Assert.assertEquals("A B", Utils.resolveValue("   'A B' ", request));
		Assert.assertEquals("A B", Utils.resolveValue("'A B'", request));
		Assert.assertEquals("A B", Utils.resolveValue("A B", request));
		Assert.assertEquals("A B", Utils.resolveValue(" A B ", request));
		Assert.assertEquals(":firstName", Utils.resolveValue("':firstName'", request));
	}
}
