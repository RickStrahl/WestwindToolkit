#West Wind Data
###Light Weight Business Object Library for Entity Framework Code First###

This library is a light weight business object framework around Entity Framework Code First
that provides a simple way to encapsulate your business logic in one place. 

* Wraps and manages DbContext instance
* Wrapped CRUD operations including Context management
* Integrated model and code based validatation support
* Error and data conflict management and reporting
* Consistent error model and trapping
* Most CRUD operations include pre and post processing hooks
* Optional internal Entity member simplifies single model operations
* Simplifies data access - especially CRUD - to single line operations
* Optional custom DbContext with low level Data Access Layer
	* Easy Stored Procedure Calls
	* String based SQL to read-only Entity mapping
    * Full range of DAL operations
    * Handle edge cases and LINQ nightmare code more easily    	

##Installation##



##Getting Started##
The first step in using this library is to 




##How it works##
Using this library you implement business objects that are associated
with an Entity Framework Code First Model and a default EF entity object.
You can inherit from EFCodeFirstBusinessBase<TEntity,TContext> and the
resulting class then has top level methods for most common CRUD operations
that operate either against an internal .Entity member (optional) or can receive
entity instances as parameters.

These CRUD methods are thin wrappers around the EF functionality but provide many 
convenience overloads and handle managing attaching to the DbContext automatically.

The external interface of the business object provides core CRUD operations that 
automatically common EFCF operations. Lookups by keys, New Entity creation with automatic
hookup to the context, delete operations by key and entities, and of course simple updates.
Most of these are thin wrappers around standard EF behavior, but reduce code significantly.
The business object also provides consolidated validation for EF CodeFirst Model validation 
as well as coded validation rules during save operations and consistent and consistent error 
checking and reporting for data operations and saves. 

The internal interface provides many before/after hooks for data operations and internal
overrides for validation logic that make it very easy to create consistent business logic quickly.

Optionally you can also use a custom EfCodeFirstContext overload of the DbContext object, which
provides access to a low level Data Access Layer to stored procedure access or data operations
that are too complex to manage through LINQ operations - it's an easy to use escape route to
fall back to low level SQL when LINQ is too obtuse or for data operations (like stored proc calls)
that are not easily handled through LINQ or Entity Framework natively.

