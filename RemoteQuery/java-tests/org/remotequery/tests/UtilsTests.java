package org.remotequery.tests;

import org.junit.Assert;
import org.junit.Test;
import org.remotequery.RemoteQuery.Utils;

public class UtilsTests {

	@Test
	public void test1() {

		String p[];
		String statement = "set:a=b//:sdlkfjdslf";
		p = Utils.parseCommandValue(statement);
		Assert.assertEquals(p[0], "set");
		Assert.assertEquals(p[1], "a=b//:sdlkfjdslf");
		
		//
		//
		//
		
		statement = "setsakf asöasf ls";
		p = Utils.parseCommandValue(statement);
		Assert.assertEquals(p[0], statement);
		Assert.assertEquals(p[1], "");
		
		//
		//
		//
		
		statement = "set-if-empty:a=asdfs fds";
		p = Utils.parseCommandValue(statement);
		Assert.assertEquals(p[0], "set-if-empty");
		Assert.assertEquals(p[1], "a=asdfs fds");
	}
}
