package org.remotequery.tests;

import org.junit.Assert;
import org.junit.Test;
import org.remotequery.z_RemoteQuery.Utils;
import org.remotequery.z_RemoteQuery.MLTokenizer.Command;

public class Test_Utils {

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
		Assert.assertEquals(p.statement, "asöasf ls");
		
		//
		//
		//
		
		statement = "set-if-empty:a=asdfs fds";
		p = Utils.parseCommandValue(statement);
		Assert.assertEquals(p.tokens.get(0), "set-if-empty");
		Assert.assertEquals(p.statement, "a=asdfs fds");
	}
}
