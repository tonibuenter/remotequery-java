package org.remotequery.tests;

import org.junit.Assert;
import org.remotequery.RemoteQuery.StatementNode;

public class RemoteQueryAssert {

	public static void assertStatementNodeEquals(StatementNode expected, StatementNode actual) {

		Assert.assertEquals(expected.cmd, actual.cmd);
		Assert.assertEquals(expected.children.size(), actual.children.size());
		for (int i = 0; i < expected.children.size(); i++) {
			assertStatementNodeEquals(expected.children.get(i), actual.children.get(i));
		}

	}

}
