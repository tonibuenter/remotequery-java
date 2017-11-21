package org.remotequery.tests;

import org.junit.Assert;
import org.remotequery.RemoteQuery.CommandNode;

public class RemoteQueryAssert {
	public static void assertCommandNodeEquals(CommandNode expected, CommandNode actual) {
		Assert.assertEquals(expected.cmd, actual.cmd);
		Assert.assertEquals(expected.children.size(), actual.children.size());
		for (int i = 0; i < expected.children.size(); i++) {
			assertCommandNodeEquals(expected.children.get(i), actual.children.get(i));
		}
	}
}
