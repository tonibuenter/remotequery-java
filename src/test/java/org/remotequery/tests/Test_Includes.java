package org.remotequery.tests;

import java.util.HashMap;
import java.util.List;

import org.apache.commons.lang3.StringUtils;
import org.junit.Assert;
import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery;
import org.remotequery.TestCentral;
import org.remotequery.RemoteQuery.ServiceEntry;
import org.remotequery.RemoteQuery.ServiceRepositoryHolder;
import org.remotequery.RemoteQuery.Utils;

public class Test_Includes {


	@BeforeClass
	public static void beforeClass() throws Exception {
		TestCentral.init();
	}

	@Test
	public void test_include1() throws Exception {
		ServiceEntry qe1 = ServiceRepositoryHolder.get().get("Test.Include.includer");
		ServiceEntry qe2 = ServiceRepositoryHolder.get().get("Test.Include.result");

		List<String> resStat = RemoteQuery.resolveIncludes(qe1.statements, new HashMap<String, Integer>());
		String statements = StringUtils.join(resStat.toArray(new String[] {}),";");
		Assert.assertEquals(Utils.removeWhitespace(qe2.statements), Utils.removeWhitespace(statements));

	}

}
