package org.remotequery.tests;

import org.apache.commons.lang3.tuple.Triple;
import org.junit.Assert;
import org.junit.Test;
import org.remotequery.RemoteQuery2.Utils;

public class Test_ParseCommands {
	@Test
	public void test1() {
		_parse_("  ", null, null, null);
		_parse_(" set:a = b ", "set", "a = b", "set:a = b");
		_parse_("set:a = b", "set", "a = b", "set:a = b");
		_parse_("set : a = b", "set", "a = b", "set : a = b");
		_parse_(" set-if-empty:a=b ", "set-if-empty", "a=b", "set-if-empty:a=b");
		_parse_("serviceId:This_and that", "serviceId", "This_and that", "serviceId:This_and that");
		_parse_("Select * from ldsfkds", "sql", "Select * from ldsfkds", "Select * from ldsfkds");
		_parse_("settti: * from ldsfkds", "sql", "settti: * from ldsfkds", "settti: * from ldsfkds");
		_parse_("if : select * from ldsfkds", "if", "select * from ldsfkds", "if : select * from ldsfkds");
		_parse_("then", "then", "", "then");
		//
		String t = "set : a b'\"";
		_parse_(t, "set", "a b'\"", t.trim());
	}

	public void _parse_(String statementRaw, String cmd, String parameter, String statement) {
		Triple<String, String, String> t = Utils.parseStatement(statementRaw);
		if (t == null) {
			Assert.assertEquals(t, statement);
			return;
		}
		Assert.assertEquals(cmd, t.getLeft());
		Assert.assertEquals(parameter, t.getMiddle());
		Assert.assertEquals(statement, t.getRight());
	}

}
