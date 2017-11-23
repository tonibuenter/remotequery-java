# Object-relational Mapping with RemoteQuery

## Support for POJO

Adding object-relational support for RQ is a actually a small step. What is needed is mapping of POJOs to parameters in the request and a mapping of the result to a POJO or a list of POJOs.

Here an example which shows everysthing (see also: `Test_Address.testOR`):

#### POJO's AddressFilter and Address

RemoteQuery directly supports working with POJO that have standard set and get methods of public fields:

```java

  public static class Address {
    public String addressId;
    public String firstName;
    public String lastName;
    public String street;
    public String zip;
    public String city;
  }

  public static class AddressFilter {
    public String nameFilter;
  }

```

#### Search with AddressFilter

The request method `runWith` supports now a POJO the will be injected as a set of parameters. The result can be mapped to a POJO again.

```java
    // ...
    AddressFilter addressFilter = new AddressFilter();
    addressFilter.nameFilter = "Anna";

    List<Address> addressList = request.runWith("Address.search", addressFilter).asList(Address.class);
    Address address = addressList.get(0);

```



#### Save Address

In this second example the address object is changed and saved again:

```java
    // ...
    address.lastName = "Braader Mayer";
    address = request.runWith("Address.save", address).as(Address.class);
```

#### The Address Services

The services used for the above examples look like:

```
--
-- SERVICE_ID = Address.search
-- ROLES      = ADDRESS_READER
-- 

set-if-empty nameFilter = %
;
select * from JGROUND.T_ADDRESS where FIRST_NAME like :nameFilter or LAST_NAME like :nameFilter



--
-- SERVICE_ID = Address.save
-- ROLES      = ADDRESS_WRITER
--

if addressId
  ;
else
  ;
  parameters select NEXT VALUE for JGROUND.global_id as ADDRESS_ID from JGROUND.T_DUAL;
  insert into JGROUND.T_ADDRESS (ADDRESS_ID) values (:addressId);
end
;
update JGROUND.T_ADDRESS set
  FIRST_NAME = :firstName,
  LAST_NAME = :lastName,
  STREET = :street,
  ZIP = :zip,
  CITY = :city
where
  ADDRESS_ID = :addressId
;
select * from JGROUND.T_ADDRESS where ADDRESS_ID = :addressId


```


