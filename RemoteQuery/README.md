RemoteQuery
===========

RemoteQuery is a very simple but powerful tool for service creation and combination. It focuses on SQL and Java based implementation. The following point show the main parts:

+ A simple SQL select is already a RQ service
+ Any RQ service can be protected with a list of access roles
+ A program implementing the RQ interface is already an RQ service
+ The RQ Servlet enables SQ services as REST services
+ Any SQL statement with named parameter is SQ service
+ The RQ Servlet directly maps HTTP parameters to SQ named parameters

Example
-------

Let us assume we have a RQ entry like "addressSearch select * from T_ADDRESS where city like :searchString".
The URL http://hostname/remoteQuery/SearchAddress?searchString=Zuer% will get a list of addresses a RQ result format

{
  header : ['firstName', 'lastName', .... , 'city']
  table : [['Hans', ' Maier', ... , 'Zuerich'],[...]] 
}



Classes and Libs
----------------
This folder hosts the essential RQ program components:
- RemoteQuery
- RemoteQueryServlet (for a web application)
- Google GSON library

