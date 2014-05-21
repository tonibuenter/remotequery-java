package remotequery.tests;

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
		
		Assert.assertEquals("123", r.getValue("a"));
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
}
