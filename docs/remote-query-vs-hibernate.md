
![RemoteQuery vs Hibernate](g4317.png)


# A Comparison between _RemoteQuery_ and _Hibernate_

This is a quick comparison between our _RemoteQuery_ approach and the well know ORM tool _Hiberante_.

We like to achieve **two things** with this comparison:

* Is _RemoteQuery_ better than _Hibernate_ ?
* Understand, how _RemoteQuery_ could improve your development tasks.

The example shown here is base on the _Hibernate_ example on [Tutorialspoint.com (Hiberante) [1]](https://www.tutorialspoint.com/hibernate/hibernate_examples.htm).


## Steps

* CRUD Service Definitions (SQL and Java Code)
* Mapping Configuration File
* Application Class
* Client Code Java
* Client Code JavaScript



## CRUD Service Definitions


### SQL Definitions

Both _RemoteQuery_ and _Hibernate_ based on a common SQL data definition file:


```sql

create create table EMPLOYEE (
   id INT NOT NULL auto_increment,
   first_name VARCHAR(20) default NULL,
   last_name  VARCHAR(20) default NULL,
   salary     INT  default NULL,
   PRIMARY KEY (id)
);

```


### _Hibernate_ POJO

In _Hibernate_ the following POJO is needed.


```java

public class Employee {
   private int id;
   private String firstName; 
   private String lastName;   
   private int salary;  

   public Employee() {}
   public Employee(String fname, String lname, int salary) {
      this.firstName = fname;
      this.lastName = lname;
      this.salary = salary;
   }
   
   public int getId() {
      return id;
   }
   
   public void setId( int id ) {
      this.id = id;
   }
   
   public String getFirstName() {
      return firstName;
   }
   
   public void setFirstName( String first_name ) {
      this.firstName = first_name;
   }
   
   public String getLastName() {
      return lastName;
   }
   
   public void setLastName( String last_name ) {
      this.lastName = last_name;
   }
   
   public int getSalary() {
      return salary;
   }
   
   public void setSalary( int salary ) {
      this.salary = salary;
   }
}

```

### _Hibernate_ Service Classes


The following code reflects well how a _Hibernate_ based service 
has to be build. The final code (using eg. JAX-RS) would be rather larger.


```java

import java.util.List; 
import java.util.Date;
import java.util.Iterator; 
 
import org.hibernate.HibernateException; 
import org.hibernate.Session; 
import org.hibernate.Transaction;
import org.hibernate.SessionFactory;
import org.hibernate.cfg.Configuration;

public class ManageEmployee {
   private static SessionFactory factory; 
   public static void main(String[] args) {
      
      try {
         factory = new Configuration().configure().buildSessionFactory();
      } catch (Throwable ex) { 
         System.err.println("Failed to create sessionFactory object." + ex);
         throw new ExceptionInInitializerError(ex); 
      }
      
      ManageEmployee ME = new ManageEmployee();

      /* Add few employee records in database */
      Integer empID1 = ME.addEmployee("Zara", "Ali", 1000);
      Integer empID2 = ME.addEmployee("Daisy", "Das", 5000);
      Integer empID3 = ME.addEmployee("John", "Paul", 10000);

      /* List down all the employees */
      ME.listEmployees();

      /* Update employee's records */
      ME.updateEmployee(empID1, 5000);

      /* Delete an employee from the database */
      ME.deleteEmployee(empID2);

      /* List down new list of the employees */
      ME.listEmployees();
   }
   
   /* Method to CREATE an employee in the database */
   public Integer addEmployee(String fname, String lname, int salary){
      Session session = factory.openSession();
      Transaction tx = null;
      Integer employeeID = null;
      
      try {
         tx = session.beginTransaction();
         Employee employee = new Employee(fname, lname, salary);
         employeeID = (Integer) session.save(employee); 
         tx.commit();
      } catch (HibernateException e) {
         if (tx!=null) tx.rollback();
         e.printStackTrace(); 
      } finally {
         session.close(); 
      }
      return employeeID;
   }
   
   /* Method to  READ all the employees */
   public void listEmployees( ){
      Session session = factory.openSession();
      Transaction tx = null;
      
      try {
         tx = session.beginTransaction();
         List employees = session.createQuery("FROM Employee").list(); 
         for (Iterator iterator = employees.iterator(); iterator.hasNext();){
            Employee employee = (Employee) iterator.next(); 
            System.out.print("First Name: " + employee.getFirstName()); 
            System.out.print("  Last Name: " + employee.getLastName()); 
            System.out.println("  Salary: " + employee.getSalary()); 
         }
         tx.commit();
      } catch (HibernateException e) {
         if (tx!=null) tx.rollback();
         e.printStackTrace(); 
      } finally {
         session.close(); 
      }
   }
   
   /* Method to UPDATE salary for an employee */
   public void updateEmployee(Integer EmployeeID, int salary ){
      Session session = factory.openSession();
      Transaction tx = null;
      
      try {
         tx = session.beginTransaction();
         Employee employee = (Employee)session.get(Employee.class, EmployeeID); 
         employee.setSalary( salary );
		 session.update(employee); 
         tx.commit();
      } catch (HibernateException e) {
         if (tx!=null) tx.rollback();
         e.printStackTrace(); 
      } finally {
         session.close(); 
      }
   }
   
   /* Method to DELETE an employee from the records */
   public void deleteEmployee(Integer EmployeeID){
      Session session = factory.openSession();
      Transaction tx = null;
      
      try {
         tx = session.beginTransaction();
         Employee employee = (Employee)session.get(Employee.class, EmployeeID); 
         session.delete(employee); 
         tx.commit();
      } catch (HibernateException e) {
         if (tx!=null) tx.rollback();
         e.printStackTrace(); 
      } finally {
         session.close(); 
      }
   }
}
```

### _Hibernate_ ORM Mapping


The mapping is done with a XML configuration or annotations. Here we show the XML configuration.

```xml
<?xml version = "1.0" encoding = "utf-8"?>
<!DOCTYPE hibernate-mapping PUBLIC 
"-//Hibernate/Hibernate Mapping DTD//EN"
"http://www.hibernate.org/dtd/hibernate-mapping-3.0.dtd"> 

<hibernate-mapping>
   <class name = "Employee" table = "EMPLOYEE">
      
      <meta attribute = "class-description">
         This class contains the employee detail. 
      </meta>
      
      <id name = "id" type = "int" column = "id">
         <generator class="native"/>
      </id>
      
      <property name = "firstName" column = "first_name" type = "string"/>
      <property name = "lastName" column = "last_name" type = "string"/>
      <property name = "salary" column = "salary" type = "int"/>
      
   </class>
</hibernate-mapping>
```




## _RemoteQuery_ Service Definitions

Instead of using Java POJO and a ORM Mapping _RemoteQuery_ creates SQL queries with some additional commands. 
The queries are then available as services and can be used directly by URLs or by client code in Java.


```sql

-- SERVICE_ID = addEmployee
-- ROLES      = HR_ADMIN

create-tid id
;
insert into EMPLOYEE
(id, first_name, last_name, salary)
values
(:id, :firstName, :lastName, :salary) 
;
select * from EMPLOYEE where id = :id



-- SERVICE_ID = listEmployees
-- ROLES      = HR_ADMIN

select * from EMPLOYEE



-- SERVICE_ID = updateEmployee
-- ROLES      = HR_ADMIN

update EMPLOYEE
set salary = :salary
where
id = :id



-- SERVICE_ID = deleteEmployee
-- ROLES      = HR_ADMIN

delete from EMPLOYEE where id = :id

```



## Code Line Count


|            | Java Code    | SQL Code      | XML |
| ----------- | ------------ | ------------- | -------------|
| Hibernate   | >170         | 7             | 16 |
| RemoteQuery | 0            | 40            | 0  |




## Development Efficency


From the way the services are build, the following properties can be deduced:

### Service changes



|             | New Service or change service | 
| ----------- | ------------ | 
| Hibernate   | Compile, deploy and restart| 
| RemoteQuery | DB content change without a restart (Code is saved in DB) | 

A big plus for _RemoteQuery_. No need of compile and re-deploy in case of new services or SQL and _RemoteQuery_ commands changes. 

### Service roles



|             | Service role based access| 
| ----------- | ------------ | 
| Hibernate   | Configured in the APP server| 
| RemoteQuery | Part of service definition | 

As role based access is used and understood very well _RemoteQuery_ has it build in the definition.



### Service Composition



|             | Service Composition| 
| ----------- | ------------ | 
| Hibernate   | Source Code (?!)| 
| RemoteQuery | Part of service script (call service, include service code)| 

_RemoteQuery_ commands like **include** and **serviceId** are ready to combine services on the server side. 
In addition there is a possibity to combine service calls at client side (e.g. JavaScript/Ajax).


### Performance Optimization

#### By the System

|             | Performance Optimization By the System| 
| ----------- | ------------| 
| Hibernate   | As good as hibernate optimization| 
| RemoteQuery | As good as database optimization| 

_Hibernate_ offers many possibility to optimize database access by minimizing the number of calls to the database. 
But more effective database optimizations are done on the level of database and query design. Designing a database for using _Hibernate_ often comes with the price that reporting is sub-optimal. Or on the on the other hand, re-design for optimization is implies enormous maintainance work
on _Hibernate_ configuration and code.


_Note_: Database optimization is the best you can get.


#### By the Developer

|             | Performance Optimization By the Developer| 
| ----------- | ------------| 
| Hibernate   | Configure caching, writting special ORM queries (limited)| 
| RemoteQuery | Create high performing table design and queries using full capacities of the DB system| 

A good read for good and strategic RDM design is the book _The Art Of SQL_ [1]. It points out that the RDB design is still central to 
the success of a project relying on relational data.


## _RemoteQuery_ Extensibility

_RemoteQuery_ offers two simple ways to extend the _RemoteQuery_ scripts:

* Register a class as a service by  **java com.myproject.MyService**
* Register programmatically an additional command. E.g.: **create-tid id**

## Conclusion

I hope I could show that _RemoteQuery_ could be a valid option for many project facing RDB access. 

_RemoteQuery_ puts RDB in the center of the
scene which in fact is still the case in many projects. 

_RemoteQuery_ is used with many hundreds of services and so far showed not limits in performance or project size.

_RemoteQuery_ scales very well with small and big projects. With project size _RemoteQuery_ written source code grows much less than typical ORM tools like _Hibernate_.





### JavaScript Example Code

```javascript

rQ.call('addEmployee',{'firstName':'Zara','lastName':'Ali','salary':1000}, function(data){
	employee1 = rQ.toList(data)[0];
});

```



### Java Example Code

```java

new Request().setServiceId("addEmployee")
	.addRole("HR_ADMIN")
	.put("firstName", "Zara")
	.put("lastName", "Ali")
	.put("salary", "1000")
	.run();

```



There is no Java code server needed. In the simple case no POJOs are needed.

Never the less if there is a need for Java specific handling inside of CRUD or other services support is provided (Employee Java Service).

## Client Code JavaScript

With this definitions the services are directly callable. E.g. by Ajax calls:

```JavaScript

var employee1, employee2, employee3;
after_insert);

rQ.call('addEmployee',{'firstName':'Zara','lastName':'Ali','salary':1000}, function(data){
	employee1 = rQ.toList(data)[0];
});
rQ.call('addEmployee',{'firstName':'Daisy','lastName':'Das','salary':5000}, function(data){
	employee2 = rQ.toList(data)[0];
	_continue();
});
rQ.call('addEmployee',{'firstName':'John','lastName':'Paul','salary':10000}, function(data){
	employee3 = rQ.toList(data)[0];
	_continue();
});

function after_insert() {

	rQ.call('listEmployees',{}, list_result);

	rQ.call('updateEmployee', {'id':employee1.id,'salary':5000}, callback);
	
	rQ.call('deleteEmployee', {'id':employee2.id}, callback);
}

```




## Reference

| Nr | Reference Detail |
| ---- | ----|
| 1 | https://www.tutorialspoint.com/hibernate/hibernate_examples.htm |
| 2 | **The Art of SQL** by Stéphane Faroult with Peter Robson; 2006 O’Reilly Media, Inc |


