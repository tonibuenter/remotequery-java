package org.remotequery.tests;

import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

import org.junit.Test;
import org.remotequery.RemoteQuery.Request;

import junit.framework.Assert;

public class Test_Java8 {
	@Test
	public void testOR() throws Exception {

		Request request = new Request().addRole("ADDRESS_WRITER").addRole("ADDRESS_READER");

		// 1. search for Anna
		AddressFilter addressFilter = new AddressFilter();
		addressFilter.nameFilter = "Anna";

		List<Address> addressList = request.runWith("Address.search", addressFilter).asList(Address.class);
		Map<String, Address> addressMap = addressList.stream().collect(Collectors.toMap(a -> a.addressId, a -> a));

		Assert.assertEquals(addressList.size(), addressMap.size());

		Assert.assertEquals("Anna", addressMap.get("8").getFirstName());
	}

}
