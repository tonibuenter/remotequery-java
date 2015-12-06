
Welome to RemoteQuery!
======================


In one sentence: RemoteQuery is a tiny utility which enables easily create SQL statements 
as JSON rest-like services. Next to SQL plain Java, Groovy can be used. 

A service is as easy defined as choosing a service id, a list of roles that can access the service and an SQL statement:
```
SERVICE_ID = Sales_Overview
ROLES      = SALE,ADMIN

select * from T_SALES
```
To make thing useful with large project service statement composition, parameter control and much more is supported.

+ [RemoteQuery (Intro Slides)] (RemoteQuery.pdf?raw=true)
+ [RemoteQuery Tech Stack Promotion] (RemoteQuery%20Tech%20Stack%20Promotion.pdf?raw=true)


About RemoteQuery Repository
----------------------------

The RemoteQuery repository contains the projects *RemoteQuery* and *RemoteQueryWeb*. 
RemoteQuery is often abbreviated as 'RQ' or 'rQ'.

RemoteQuery (RQ)
----------------

The RemoteQuery (RQ) project is the main project. It includes the RemoteQuery class and the RemoteQueryServlet (Java web component).
Currently the Java implementation is ready for use.

It is planned to have RemoteQuery for ASP.NET and PHP ready end of this year.

[See RemoteQuery/README.rd](https://github.com/tonibuenter/RemoteQuery/blob/master/RemoteQuery/README.md)


RemoteQueryWeb
--------------

RemoteQueryWeb is a sample Java web project for using RQ in a Java Servlet-base web application.


Essential RQ components for Java
--------------------------------

+ [RemoteQuery.java] (https://github.com/tonibuenter/RemoteQuery/blob/master/RemoteQuery/java/org/remotequery/RemoteQuery.java)
+ [RemoteQueryServlet.java](https://github.com/tonibuenter/RemoteQuery/blob/master/RemoteQuery/java/org/remotequery/RemoteQueryServlet.java)
+ *gson-2.2.4.jar* [See RemoteQuery/java_libs] (https://github.com/tonibuenter/RemoteQuery/tree/master/RemoteQuery/java-libs) or on Google Code
+ *slf4j-api-1.7.9.jar* [See RemoteQuery/java_libs] (https://github.com/tonibuenter/RemoteQuery/tree/master/RemoteQuery/java-libs)



Quick Start for the RemoteQueryWeb application with Java/Eclipse
----------------------------------------------------------------

+ Download the **RemoteQuery** (Download ZIP button)
+ Import the sub folder **RemoteQueryWeb** with Eclipse as existing projects
+ You may possibly set the JDK in the project
+ Deploy the RemoteQueryWeb to Apache Tomcat or JEE server
+ ... all done ...


