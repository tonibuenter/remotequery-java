package remotequery.tests;

import org.junit.Assert;
import org.junit.Test;
import org.remotequery.RemoteQuery.Utils;

public class TokenizerTests {

	@Test
	public void test1() {

		Assert.assertEquals(3, Utils.tokenize("this \\, is, a,test").length);
		Assert.assertEquals(4, Utils.tokenize("this , is, a,test").length);
		Assert.assertEquals(" is", Utils.tokenize(" is, a,test")[0]);
		Assert.assertEquals("this,is",
		    Utils.joinTokens(new String[] { "this", "is" }));
		Assert.assertEquals("t\\,his,is",
		    Utils.joinTokens(new String[] { "t,his", "is" }));

	}

	@Test
	public void test2() {

		Assert.assertEquals(2, Utils.tokenize("set-0:a=b", ':', '\\').length);
		Assert
		    .assertEquals(2, Utils.tokenize(
		        Utils.tokenize("set-0:a=b", ':', '\\')[1], '=', '\\').length);

	}
}
