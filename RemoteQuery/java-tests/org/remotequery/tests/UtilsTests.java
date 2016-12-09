package org.remotequery.tests;

import org.junit.Assert;
import org.junit.Test;
import org.remotequery.RemoteQuery.Utils;
import org.remotequery.RemoteQuery.MLTokenizer.Command;

public class UtilsTests {

	@Test
	public void test1() {

		
		String statement = "set:a=b//:sdlkfjdslf";
		Command p = Utils.parseCommandValue(statement);
		
		Assert.assertEquals(p.tokens.get(0), "set");
		Assert.assertEquals(p.statement, "a=b//:sdlkfjdslf");
		
		//
		//
		//
		
		statement = "setsakf asöasf ls";
		p = Utils.parseCommandValue(statement);
		Assert.assertEquals(p.statement, statement);
		
		//
		//
		//
		
		statement = "set-if-empty:a=asdfs fds";
		p = Utils.parseCommandValue(statement);
		Assert.assertEquals(p.tokens.get(0), "set-if-empty");
		Assert.assertEquals(p.statement, "a=asdfs fds");
	}
}
