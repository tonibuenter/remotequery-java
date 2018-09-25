

# A Critical View on ORM Tools

Assume the situation of junior developer locking for a way to handle relational databases (RDB) with Java. Looking at common articles and blogs will harden the impression that object relational (ORM) mapping tools are the kings way to pursue. He or she may finally choose a tool such as _Hibernate_, _EJB_, _TopLink_ or others the like.

In this short article I like to point out some thought and experiences about the back-side of the ORM approach that many tools are following.

Topics like:

* maintainabilty
* implication/obstruction on design
* priorities
* effectiveness

Further I will point out our approach called _RemoteQuery_ that might be a valid alternative for many project situations.


For this article I refer to a for - my purpuse interchangable - article from [Tutorialpoints.com](https://www.tutorialspoint.com/hibernate/orm_overview.htm) that points out the positive qualities of ORM tools [1].


I will critically comment point by point and conclude with a final statement.

### Why Object Relational Mapping (ORM)?

<cite>
When we work with an object-oriented system, there is a mismatch between the object model 
and the relational database. RDBMSs represent 
data in a tabular format whereas object-oriented languages, 
such as Java or C# represent it as an interconnected graph of objects.
</cite>


_Comment_
The gab between relational and object models is highly overstated. 
As soon as you are using technical IDs for single table entries, the design
_is_ object oriented. This is a good way to design and many ORM actually often enforce it.

On the other side, if the database is really relational designed, ORM tools need a lot of hand 
written helper code and 
configuration. ORM tools do not help much.



### Granularity

<cite>
Sometimes you will have an object model, which has more 
classes than the number of corresponding tables in the database.
</cite>

_Comment_
This situation is best solved with redesign the relational or object model.
With RemoteQuery / SQL you easily can define adapter queries for the unsolvable cases.
Even if ORM could solve this structural gab performace and maintenance are hurt.


### Inheritance

<cite>
RDBMSs do not define anything similar to Inheritance, which is a natural 
paradigm in object-oriented programming languages.
</cite>


_Comment_
Even being a fan of OO concepts, I learned that implementing inheritance on data access classes is
painful. Avoiding inheritance implementation between OO and RDB models is by far the best solution.



### Identity
<cite>
An RDBMS defines exactly one notion of 'sameness': the primary key. Java, however, 
defines both object identity (a==b) 
and object equality (a.equals(b)).
</cite>


COMMENT
This is not really a problem. You alway can overwrite the equals method.


CITE

Associations
<cite>
Object-oriented languages represent associations using object references whereas an RDBMS 
represents an association as 
a foreign key column.
</cite>


COMMENT

Most business cases can be implemented with flat table like objects and list of objects. 
Connected objects can very easily
connected with using hash tables over their keys.
With RemoteQuery we just solved this by connecting objects on the client side.

CITE

Navigation

<cite>
The ways you access objects in Java and in RDBMS are fundamentally different.
</cite>


COMMENT

In this absolute definition this is not true. Think about SQL join statements. 
They do often a quite reasonable job in navigate down 
hierarchies.


## Propagated Advantages

Having seen now that actually the problems between 
OO and RDB models are not that grave in real world. 
I like to comment on the wide spread advantages that are propagated.


<cite>
Let’s business code access objects rather than DB tables.
</cite>


COMMENT
Actually there is no natural law for that. Second, DB tables, views and even stored procedures do
live often significantly longer than application code. I would propagate: keep business logic in
Java code to a absolute minimum. For reasons of maintainability and performance rather use 
queries and views for
business logic.
With RemoteQuery we are able to put more than 95% of the business logic into the RemoteQuery scripts.


<cite>
Hides details of SQL queries from OO logic.
</cite>


COMMENT

This is wishful but unrealistic.

CITE

<cite>
Entities based on business concepts rather than database structure.
</cite>


COMMENT

Make your live easier and base database structure on business concept.

CITE

<cite>
Transaction management and automatic key generation.
</cite>


COMMENT

This is not a specieal ORM topic. Actually, rely on the ORM transaction management is 
complex and often rather a source of failures than an elegant solution.

CITE

<cite>
Fast development of application.
</cite>


COMMENT

This is a promise I haven't seen.


## Concepts




## Reference

| Nr | Reference Detail |
| ---- | ----|
| 1 | [ORM Overview Tutorialpoints.com](https://www.tutorialspoint.com/hibernate/orm_overview.htm) |
| 2 | **The Art of SQL** by Stéphane Faroult with Peter Robson; 2006 O’Reilly Media, Inc |

