

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

**Citation**:


> When we work with an object-oriented system, there is a mismatch between the object model 
> and the relational database. RDBMSs represent 
> data in a tabular format whereas object-oriented languages, 
> such as Java or C# represent it as an interconnected graph of objects.



**Comment**:

The gab between relational and object models is highly overstated. 
As soon as you are using technical IDs for single table entries, the design
_is_ object oriented. In many cases this is a good design even without ORM applied.

On the other side, if the database is _legacy_ and 'deep' relational designed, ORM tools do not provide elegant mappings often resulting in a lot of hand written glue code and 
configuration. 



### Granularity

**Citation**:

> Sometimes you will have an object model, which has more 
> classes than the number of corresponding tables in the database.

**Comment**:

In real time this situation is very rare. As the database structure has a longer life span than the classes it is reasonable to
design the classes according to the tables. Doing otherwise is calling for unforced difficulties.
With _RemoteQuery_ we showed that there are no ORM data classes needed at all.


### Inheritance

**Citation**:

> RDBMSs do not define anything similar to Inheritance, which is a natural 
> paradigm in object-oriented programming languages.

**Comment**:

Even being enthusiastic about object-oriented concepts, I learned that implementing data access classes  with inheritence result in a painful experience.
painful. Avoiding inheritance on implementation in OO and RDB models is by far the best solution.


### Identity

**Citation**:

> An RDBMS defines exactly one notion of 'sameness': the primary key. Java, however, 
> defines both object identity (a==b) 
> and object equality (a.equals(b)).

**Comment**:

This is true with an academic view not really a problem. If needed, you always can overwrite the equals method.

### Associations

**Citation**:

> Object-oriented languages represent associations using object references whereas an RDBMS 
> represents an association as 
> a foreign key column.

**Comment**:

Most business cases can be implemented with flat tables like objects and lists of objects. 
Objects with relations can easily be
connected by using hash tables over their keys.
With _RemoteQuery_ we usually solved this by connecting objects on the client side.

### Navigation

**Citation**:

> The ways you access objects in Java and in RDBMS are fundamentally different.

**Comment**:

It is true to a certain extent. If you take the SQL join and order by statements into consideration close similarities can be seen between object-oriented navigation and relational joining of tables. 
With _RemoteQuery_ we enjoy the complementary character of RDM and object-oriented programming.


## Propagated Advantages

Having seen now that the problems and gaps between 
OO and RDB models are not that grave in the real world,
I like to comment on the claimed advantages.

**Citation**:

> Let’s business code access objects rather than DB tables.

**Comment**:

Actually there is no natural law for that. Second, DB tables, views and even stored procedures do
live often significantly longer than application code. I would propagate: keep business logic in
Java code to a absolute minimum. For reasons of maintainability and performance rather use 
queries and views for
business logic.
With RemoteQuery we are able to put more than 95% of the business logic into the RemoteQuery scripts.

**Citation**:

> Hides details of SQL queries from OO logic.

**Comment**:

One of ORMs central achievment is  reading and writing parent child releations. As it turns out in reality, is that 
in many cases the queries for read and write are slowing down when number of records rise. The ORM tools solution ends up in
writing specific queries which optimize the process. So, it ends up where it could have began.

**Citation**:

> Entities based on business concepts rather than database structure.

**Comment**:

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

