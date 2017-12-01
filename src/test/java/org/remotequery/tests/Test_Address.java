package org.remotequery.tests;

import java.util.HashSet;
import java.util.List;
import java.util.Set;

import org.junit.BeforeClass;
import org.junit.Test;
import org.remotequery.RemoteQuery.ObjectStore;
import org.remotequery.RemoteQuery.Request;
import org.remotequery.RemoteQuery.Result;

import junit.framework.Assert;

public class Test_Address {


	
	
	

	@Test
	public void testAddress_filter() {
		Result result = new Request().setServiceId("Address.search").put("nameFilter", "Jo%").addRole("ADDRESS_READER")
				.run();
		// convert to a POJO
		List<Address> list = result.asList(Address.class);
		Assert.assertEquals(2, list.size());
	}

	@Test
	public void testORMapping_new_address() throws Exception {

		Address address = new Address();
		address.firstName = "Monika";
		address.lastName = "Gilic";
		address.street = "Blumenweg 73";
		address.street = "8676";
		address.street = "Underwiler";

		Set<String> roles = new HashSet<String>();
		roles.add("ADDRESS_WRITER");

		ObjectStore<Address> objectStore = new ObjectStore<Address>(Address.class, roles);

		address = objectStore.update("Address.save", address).asObject(Address.class);
		Assert.assertNotNull(address);
		Assert.assertNotNull(address.addressId);
		Assert.assertEquals("Monika", address.firstName);

	}

	@Test
	public void testOR() throws Exception {

		Request request = new Request().addRole("ADDRESS_WRITER").addRole("ADDRESS_READER");

		// 1. search for Anna
		AddressFilter addressFilter = new AddressFilter();
		addressFilter.nameFilter = "Anna";

		List<Address> addressList = request.runWith("Address.search", addressFilter).asList(Address.class);
		Address address = addressList.get(0);
		Assert.assertEquals("8", address.addressId);

		// 2. save change
		address.lastName = "Braader Mayer";

		address = request.runWith("Address.save", address).as(Address.class);

		Assert.assertNotNull(address);
		Assert.assertNotNull(address.addressId);
		Assert.assertEquals("Anna", address.firstName);
		Assert.assertEquals("Anna", address.firstName);
		Assert.assertEquals("Braader Mayer", address.lastName);
		Assert.assertEquals("8", address.addressId);

	}

	@BeforeClass
	public static void beforeClass() throws Exception {
		TestCentral.init();
	}

}
