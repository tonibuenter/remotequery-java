package org.remotequery.tests;

import java.util.HashMap;
import java.util.Map;

import org.junit.Assert;
import org.junit.Test;
import org.remotequery.RemoteQuery;

public class Test_ConvertQuery {

	@Test
	public void t1() {
		String query;
		String questionMarkQuery;
		String parameters;

		query = "select :Abc from Table x where x = :w";
		questionMarkQuery = "select ? from Table x where x = ?";
		parameters = "Abc,w";

		assertIt(query, questionMarkQuery, parameters);

		query = "select :u_fA :sadf from Table x where x = :w os :x";
		questionMarkQuery = "select ? ? from Table x where x = ? os ?";
		parameters = "u_fA,sadf,w,x";

		assertIt(query, questionMarkQuery, parameters);

		query = ":a:x";
		questionMarkQuery = "??";
		parameters = "a,x";
		assertIt(query, questionMarkQuery, parameters);

		query = ":a:x':sdf sdf'";
		questionMarkQuery = "??':sdf sdf'";
		parameters = "a,x";
		assertIt(query, questionMarkQuery, parameters);

		Map<String, String> map = new HashMap<>();
		map.put("fileTids", "1,2,3");
		query = ":a,:x':sdf sdf' where TID in (:fileTids[])";
		questionMarkQuery = "?,?':sdf sdf' where TID in (?,?,?)";
		parameters = "a,x,fileTids[0],fileTids[1],fileTids[2]";
		assertIt(query, questionMarkQuery, parameters, map);

		map = new HashMap<>();
		map.put("fileTids", "a,c,c,d,dd,a");
		map.put("personId", "hans,toni,albert");
		query = "select FIRST_NAME from JGROUND.T_PERSON where id in (:personId[])";
		questionMarkQuery = "select FIRST_NAME from JGROUND.T_PERSON where id in (?,?,?)";
		parameters = "personId[0],personId[1],personId[2]";
		assertIt(query, questionMarkQuery, parameters, map);

		map = new HashMap<>();
		map.put("fileTids", "a,c,c,d,dd,a");
		map.put("personId", "hans,toni,albert");
		query = "select FIRST_NAME from JGROUND.T_PERSON where id in (:personId[])  :fileTids[]";
		questionMarkQuery = "select FIRST_NAME from JGROUND.T_PERSON where id in (?,?,?)  ?,?,?,?,?,?";
		parameters = "personId[0],personId[1],personId[2],fileTids[0],fileTids[1],fileTids[2],fileTids[3],fileTids[4],fileTids[5]";
		assertIt(query, questionMarkQuery, parameters, map);

		map = new HashMap<>();
		map.put("aa", "12.23,765.2,1E13");
		query = "update JGROUND.T_TIME_SERIE values (:aa[])";
		questionMarkQuery = "update JGROUND.T_TIME_SERIE values (?,?,?)";
		parameters = "aa[0],aa[1],aa[2]";
		assertIt(query, questionMarkQuery, parameters, map);

		map = new HashMap<>();
		map.put("aa", "12.23,765.2,1E13");
		map.put("b", "12.23,765.2,1E13");
		query = ":aa[] update JGROUND.T_TIME_SERIE values (:b)";
		questionMarkQuery = "?,?,? update JGROUND.T_TIME_SERIE values (?)";
		parameters = "aa[0],aa[1],aa[2],b";
		assertIt(query, questionMarkQuery, parameters, map);

	}

	public void assertIt(String query, String questionMarkQuery, String parameters) {
		assertIt(query, questionMarkQuery, parameters, null);
	}

	public void assertIt(String query, String questionMarkQuery, String parameters, Map<String, String> map) {
		map = map == null ? new HashMap<String, String>() : map;
		RemoteQuery.QueryAndParams qap = new RemoteQuery.QueryAndParams(query, map);
		qap.convertQuery();
		Assert.assertEquals(query, qap.named_query);
		Assert.assertEquals(questionMarkQuery, qap.qm_query);
		Assert.assertArrayEquals(parameters.split(","), qap.param_list.toArray());
	}

}
