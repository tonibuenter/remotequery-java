package remotequery.tests;

import org.junit.Assert;
import org.junit.Test;
import org.remotequery.RemoteQuery;
import org.remotequery.RemoteQuery.MainQuery;

public class ParseLevelTests {

	@Test
	public void test1() {

		Assert.assertEquals(new Integer(0), MainQuery.parseLevel("-0"));
		Assert.assertEquals(new Integer(123), MainQuery.parseLevel("-123"));
		Assert.assertEquals(new Integer(9), MainQuery.parseLevel("9"));
		Assert.assertEquals(new Integer(0), MainQuery.parseLevel("-INITIAL"));
		Assert.assertEquals(new Integer(RemoteQuery.LevelConstants.REQUEST), MainQuery.parseLevel("REQUEST"));
		Assert.assertEquals(new Integer(RemoteQuery.LevelConstants.REQUEST), MainQuery.parseLevel("-REQUEST"));
		Assert.assertEquals(new Integer(RemoteQuery.LevelConstants.HEADER), MainQuery.parseLevel("HEADER"));
		Assert.assertEquals(new Integer(RemoteQuery.LevelConstants.HEADER), MainQuery.parseLevel("header"));
		Assert.assertEquals(new Integer(RemoteQuery.LevelConstants.HEADER), MainQuery.parseLevel("-HEADER"));
		Assert.assertEquals(new Integer(RemoteQuery.LevelConstants.HEADER), MainQuery.parseLevel("-header"));
		Assert.assertEquals(new Integer(RemoteQuery.LevelConstants.APPLICATION), MainQuery.parseLevel("-application"));
		Assert.assertEquals(new Integer(RemoteQuery.LevelConstants.INTER_REQUEST), MainQuery.parseLevel("-inter_request"));
		Assert.assertEquals(new Integer(RemoteQuery.LevelConstants.INITIAL), MainQuery.parseLevel(""));

	}
}
