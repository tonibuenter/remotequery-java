package org.remotequery.tests;

import java.util.Arrays;
import java.util.Comparator;

import org.junit.Assert;
import org.junit.Test;

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

}
