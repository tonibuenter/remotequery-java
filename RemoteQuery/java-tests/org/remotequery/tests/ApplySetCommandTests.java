package org.remotequery.tests;

import org.junit.Assert;
import org.junit.Test;
import org.remotequery.RemoteQuery;
import org.remotequery.RemoteQuery.MainQuery;
import org.remotequery.RemoteQuery.Request;

public class ApplySetCommandTests {

	@Test
	public void test1() {
		Request r = new Request();
		r.put("a", "123");
		String cmd = RemoteQuery.MLT.set + "-0";
		String stmt = "a=124";
		MainQuery.applySetCommand(r, cmd, stmt);
		Assert.assertEquals("124", r.getValue("a"));
	}
	@Test
	public void test2() {
		Request r = new Request();
		r.put("a", "123");
		String cmd = RemoteQuery.MLT.set + "0";
		String stmt = "a=124";
		MainQuery.applySetCommand(r, cmd, stmt);
		Assert.assertEquals("124", r.getValue("a"));
	}
	@Test
	public void test3() {
		Request r = new Request();
		r.put("a", "123");
		String cmd = RemoteQuery.MLT.set + "100";
		String stmt = "a=124";
		MainQuery.applySetCommand(r, cmd, stmt);
		
		Assert.assertEquals("124", r.getValue("a"));
	}
	@Test
	public void test4() {
		Request r = new Request();
		r.put("b", "123");
		String cmd = RemoteQuery.MLT.set + "100";
		String stmt = "a=124";
		MainQuery.applySetCommand(r, cmd, stmt);
		
		Assert.assertEquals("123", r.getValue("b"));
	}
	@Test
	public void test5() {
		Request r = new Request();
		r.put("b", "123");
		r.put(100, "b", "33");
		r.put(1, "b", "33");
		r.put(10000, "b", "33");
		r.put(1123412, "b", "33");
		r.put(11, "b", "33");
		String cmd = RemoteQuery.MLT.set_null ;
		String stmt = "b";
		MainQuery.applySetCommand(r, cmd, stmt);
		
		Assert.assertEquals(null, r.getValue("b"));
	}
}
