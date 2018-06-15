package org.remotequery.tests;

import org.junit.Assert;
import org.junit.Test;
import org.remotequery.RemoteQuery.Utils;

public class Test_Tokenizer {

	@Test
	public void commaListTest() {
		String[] list = { "a", "b", "c,,,,,,,,,,,,b", "set-0:c=b" };
		test(list);
		list = new String[] { "a" };
		test(list);
		list = new String[] {};
		test(list);
	}

	public void test(String[] list) {
		String s = Utils.joinTokens(list);
		String[] a = Utils.tokenize(s);
		Assert.assertArrayEquals(list, a);
	}

	@Test
	public void test1() {
		Assert.assertEquals(2, Utils.tokenize("set-0:a=b", ':', '\\').length);
		Assert.assertEquals(2, Utils.tokenize(Utils.tokenize("set-0:a=b", ':', '\\')[1], '=', '\\').length);

		Assert.assertEquals(3, Utils.tokenize("this \\, is, a,test").length);
		Assert.assertEquals(4, Utils.tokenize("this , is, a,test").length);
		Assert.assertEquals(" is", Utils.tokenize(" is, a,test")[0]);
		Assert.assertEquals("this,is", Utils.joinTokens(new String[] { "this", "is" }));
		Assert.assertEquals("t\\,his,is", Utils.joinTokens(new String[] { "t,his", "is" }));

	}

	@Test
	public void testNesting() {

		Assert.assertEquals("\\se:t-0", Utils.tokenize("\\se\\:t-0:a=b", ':', '\\')[0]);

		String s = Utils.tokenize("a:b=1\\,2,u=v", ':', '\\')[1];
		s = Utils.tokenize(s, ',', '\\')[0];
		s = Utils.tokenize(s, '=', '\\')[1];
		Assert.assertEquals("1,2", s);
		s = Utils.tokenize("1\\,2", ',', '\\')[0];
		Assert.assertEquals("1,2", s);
		s = Utils.tokenize("set:a=1\\,2\\,1", ':', '`')[1];
		Assert.assertEquals("a=1\\,2\\,1", s);

		s = Utils.tokenize(s, ',', '\\')[0];
		s = Utils.tokenize(s, '=', '\\')[1];
		s = Utils.tokenize(s, ',', '\\')[0];

		Assert.assertEquals("1", s);

	}
}
