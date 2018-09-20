# RemoteQuery Reference Manual

## Usage Model

The usage model of a RQ service can be summarized as follows:


* Create a request object with _a service id_, a list of _access roles_, a _parameter map_
* Call the _run method_
* Process the _result object_

## Run Method Execution

The Remote Query engine does the following with in the _run method_:

* Check the provided _access roles_ to the _service roles_. If _service roles_ are not empty and the intersection between the two role list is not empty continue, else abort.
* Process SQL as Commands with the provided _parameter map_
* Return the last query result as a _result object_





## Commands Statement Syntax

The command statement syntax is:

```
*command* white-space *parameter-part*

```

Multiple command statements are separated with a semicolon: `;`


## Parameters

Parameters are references by a colon and its name.
In the following example the value of *id* is inserted into the query:


```
select * from T_ADDRESS where ADDRESS_TID = :id

```
In the following example the value of *id* is assigned to the parameters *addressId*.

```
set addressId = :id

```

## Array Parameters

A parameter such as :

```
names = 'Anna,Ralf,Sara'
``` 
can be used for a query with a in-clause such as:

```
select * from JGROUND.T_ADDRESS where FIRST_NAME in (:names[]) order by ADDRESS_ID
```
The parameter *:names[]* will automatically be filled with the three comma separated names.


## Commands (build-in)

Command | Usage| Description
--- | --- | ---
*set*  | set hello = 'world' | This sets right hand value (here: world) to the request parameter (here: 'hello').
*set-if-empty* | set-if-empty name = 'value' | Same as *set* but only if the current value of name is empty.
*parameters*  | parameters select * from T_ADDRESS ... | The parameter part is processed as statements and the result - actually the first row if available - is applied to the parameters.
*parameters-if-empty*  |   | The same as with *parameters* but only for parameters that are empty.

### Command *serviceId* 

The *serviceId* expects another serviceId. The service behind it is called and the result returned. All service roles are applied.

Example:

```
serviceId Test.Command.example

```


### Command *include* 

The *include* command is actually rather a macro the includes the statements of another service. This actually implies that roles of the
inluded service are not applied.


Example:

```
include Test.Command.example

```


### Command *java* or *class* 

The parameter part of a *java* or *class* command 
is expected to be Java class that implements the `RemoteQuery.IQuery` interface.



Example:

```
java org.remotequery.tests.UuidQuery
```

*Remark:*

If the Java class implements the `RemoteQuery.ICommand` interface as well, the `RemoteQuery.ICommand.run` method is called instead of the `RemoteQuery.IQuery.run` method.

#### Command *java* or *class*  with method name

For the *java* or *class* command we can define a method in the Java 8 style instead of a Java class:

```
java org.remotequery.tests.UuidQuery::createNew
```

The method defined expects a request object as input and a result object as output as the `RemoteQuery.IQuery.run` method.





### Command *if*, *else*, *end* 

The *if* command expects a parameter and if the parameter has a value the command below are executed. If not the commands below the *else* are executed. The *end*  signifies the end of the *if*.

Example:

```
if :addressId;
    serviceId Address.update;
  else;
    serviceId Address.insert;
end
```

### Command *switch*, *case*, *default*, *break*, *end* 

The *switch* command expects a parameter name. If the value of the parameter is the same as the parameter part of a *case* statement, this *case* statement matches. The statements after a matching *case* are processed to the end of the *switch* child statements or to the break statements. The statements after the *default* are processed if there was not matching case statements before. The *end* statement signifies the end of the *switch*.

Example:

```
switch :ticketType;
  case 'comment';    
    serviceId TicketComment.save;
    break;
  case 'incident';    
    serviceId TicketIncident.save;
    break;
  default;    
    serviceId Ticket.save;
end  
```


### Command *foreach*, *end* 

The *foreach* command expects as an argument a statement. The result of this statement is used for iteration over the rows. The rows are taken for paramter values with the column names (in camel case) as parameter names. The *end* statement signifies the end of the *foreach*.

Example:

```
foreach 
  select NAME, VALUE from JGROUND.T_APP_PROPERTIES 
  ;
  insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE)  values (:name || '-2', :value)
  ;
end
```




### Command *while*, *end* 

The *while* command is by the structure very similar to the *if* command. 
The child statements of the while are repeatedly executed till the parameter 
value of the *while* is empty. The *end* statement signifies the end of the *while* command:

```
while parameter_is_not_empty;
  statement_1;
  statement_2;
  .. more statements;
end;
```

Example:

```
insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE) values ('while-1', '1');
insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE) values ('while-2', '2');
insert into JGROUND.T_APP_PROPERTIES (NAME, VALUE) values ('while-3', '3');
set whileName = start;
while whileName;
  delete from JGROUND.T_APP_PROPERTIES where NAME = :whileName;
  parameters select NAME as "WHILE_NAME" from JGROUND.T_APP_PROPERTIES where NAME like 'while%';
end;
```

