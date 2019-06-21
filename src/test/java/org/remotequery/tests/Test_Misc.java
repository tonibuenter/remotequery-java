package org.remotequery.tests;

import java.util.Arrays;
import java.util.Comparator;
import java.util.HashMap;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.junit.Assert;
import org.junit.Test;
import org.remotequery.RemoteQueryUtils;

public class Test_Misc {

	@Test
	public void sort1() {
		Integer[] i1 = { 8, 4, 1, 3 };
		Integer[] i2 = { 1, 3, 4, 8 };
		Integer[] i3 = { 8, 4, 3, 1 };

		Arrays.sort(i1);
		Assert.assertArrayEquals(i2, i1);

		Arrays.sort(i1, new Comparator<Integer>() {
			@Override
			public int compare(Integer o1, Integer o2) {
				return -1 * o1.compareTo(o2);
			}
		});

		Assert.assertArrayEquals(i3, i1);
	}

	@Test
	public void textingTest() {

		Pattern ptn = Pattern.compile("\\:\\w+");

		Matcher matcher = ptn.matcher("Hello :names. :name_ :name- :name");
		StringBuffer stringBuffer = new StringBuffer();
		while (matcher.find()) {
			String s = matcher.group();
			s = s.substring(1);
			System.out.println(s);
			matcher.appendReplacement(stringBuffer, s);
		}
		System.out.println(stringBuffer.toString());

		Map<String, String> map = new HashMap<String, String>();

		map.put("name", "World");

		String actual = RemoteQueryUtils.texting("Hello :name", map);
		Assert.assertEquals("Hello World", actual);
		//
		actual = RemoteQueryUtils.texting("Hello :name:name", map);
		Assert.assertEquals("Hello WorldWorld", actual);
		//
		actual = RemoteQueryUtils.texting("Hello :name:bla", map);
		Assert.assertEquals("Hello World:bla", actual);
		//
		actual = RemoteQueryUtils.texting("Hello :name", map);
		Assert.assertEquals("Hello World", actual);
		//
		actual = RemoteQueryUtils.texting("Hello :names", map);
		Assert.assertEquals("Hello :names", actual);
		//

	}

}
