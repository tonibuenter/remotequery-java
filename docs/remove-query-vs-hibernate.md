# A Close Comparison between RemoteQuery and Hibernate

This is a short comparison between our RemoteQuery approach and the well know ORM tool Hiberante.

We like to achieve two things with this comparison:

* Is _RemoteQuery_ better than _Hibernate_ ?
* Understand, how RemoteQuery could improve your development tasks.

We build our example on the content from 

[Tutorialspoint.com (Hiberante)](https://www.tutorialspoint.com/hibernate/hibernate_examples.htm).


## Steps

* CRUD Service Definitions
* Database Table
* Mapping Configuration File
* Application Class
* Client Code Java
* Client Code JavaScript



## CRUD Service Definitions


### SQL Definitions

Both RemoteQuery and Hibernate based on a common SQL data definition file:


```sql
create create table EMPLOYEE (
   id INT NOT NULL auto_increment,
   first_name VARCHAR(20) default NULL,
   last_name  VARCHAR(20) default NULL,
   salary     INT  default NULL,
   PRIMARY KEY (id)
);
```


### Hibernate POJO and Service Classes

In Hibernate the following POJO is needed.


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


The following code substiture of how a service has to be build. The final code (using eg. JAX-RS) would be much larger.


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

### Hibernate ORM Mapping


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




## RemoteQuery Service Definitions

Instead of using Java POJO and a ORM Mapping RemoteQuery creates SQL queries with some additional commands. 
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


            | Java Code    | SQL Code      | XML 
----------- | ------------ | ------------- | -------------
Hibernate   | >170         | 7             | 16 
RemoteQuery | 0            | 40            | 0  




## Development Efficency


From the way the services are build the following properties can be deduced:

### Service changes



            | After a code change 
----------- | ------------ 
Hibernate   | Compile, deploy and restart
RemoteQuery | DB content change without a restart (Code is saved in DB) 


### Service roles



            | Service role based access
----------- | ------------ 
Hibernate   | Configured in the APP server
RemoteQuery | Part of service definition 


### Service Composition



            | Service Composition
----------- | ------------ 
Hibernate   | Source Code
RemoteQuery | Part of service script (call service, include service code)



### Performance Optimization

#### By the System

            | Performance Optimization By the System
----------- | ------------
Hibernate   | As good as hibernate optimization
RemoteQuery | As good as database optimization

_Note_: Database optimization is the best you can get.


#### By the Developer

            | Performance Optimization By the Developer
----------- | ------------
Hibernate   | Configure caching, writting special ORM queries (limited)
RemoteQuery | Create high performing queries using full capacities of the DB system

_Note_: RemoteQuery is completly un-biased on how data is read or written in different services.



## Conclusion

I hope I could show that RemoteQuery could be a valid option for many project facing RDB access.
RemoteQuery is used with many hundreds of services and so far showed not limits in performance or project size.
The real benefict and superiority over populare ORM tools 
like Hibernate actually starts when the back end starts to be complex and demanding.



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

With this definitions the services are directly callable by e.g. ajax calls:

var employee1, employee2, employee3;

var _continue = _.after(3, after_insert);

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

function after_insert(){
	rQ.call('listEmployees',{}, list_result);

	rQ.call('updateEmployee',{'id':employee1.id,'salary':5000}, callback);
	
	rQ.call('deleteEmployee',{'id':employee2.id}, callback);
}




Reference

https://www.tutorialspoint.com/hibernate/hibernate_examples.htm