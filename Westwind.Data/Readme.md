#West Wind Data
###Light Weight Business Object Library for Entity Framework Code First###

This library is a light weight business object framework around Entity Framework Code First.
It provides a simple way to encapsulate your business logic in one place, with the following
features:

* Wraps and manages DbContext instance
* Wrapped CRUD operations with DbContext management (and more)
* Model and code based validatation support
* Error and data conflict management and reporting
* Consistent error model and trapping
* Most CRUD operations include pre and post processing hooks
* Optional internal Entity member simplifies single model operations
* Simplifies data access - especially CRUD - to single line operations
* Optional custom DbContext with low level Data Access Layer
	 * Handle edge cases and LINQ nightmare code more easily    	
     * Easy Stored Procedure Calls
	 * String based SQL to read-only Entity mapping
     * Full range of DAL operations
     
##Installation##
In the current beta status of the library you have to install the library
from the binaries. There's no NuGet package just yet. The latest binaries
can be found in the /libs folder.

Requirements:
* .NET 4.5 Runtime
* EntityFramework 5.x (from NuGet)

To run the Westwind.Data framework you will need the following assemblies:

* Westwind.Data.dll
* Westwind.Utilities.dll
* EntityFramework 5.0 (from NuGet)

##Getting Started##
*under construction*

###Create your EF CodeFirst Model and Database###
The EF Code First business object library works off an existing Entity Framework Code First
Model and Database, so before you create a business object you'll need to create the 
EF model and context. 

Create a connection string entry in your .config file, ideally with the same name as the 
DbContext, so no parameters are required for the Context to find the connection.

###Create your Business Object###
Create an instance of the business object and inherit it from EfCodeFirstBusinessBase:

```C#
public class busCustomer : EfCodeFirstBusinessBase<Customer,OrdersContext>
{ }
```    

You specify a main entity type (Customer in this case) and the DbContext type (OrdersContext). 
You now have a functioning business object for Customers.

Note that you create many business objects for each **logical** business context
or operation which wouldn't necessarily match each entity in the data model. For example,
you would have an busOrder business object, but likely not a LineItem business object since
lineitems are logically associated with the Order and managed through an Order business object.

###Using the Business Object###
Without adding any functionality the business object is now functional and can run basic
CRUD operations:

```C#
var customerBus = new busCustomer();
    
// Add a new customer
var customer = customerBus.NewEntity();
customer.LastName = "Strahl";
customer.FirstName = "Rick";
customer.Entered = DateTime.UtcNow;
    
// Save all data since last Save() operation
Assert.IsTrue(customerBus.Save(),customerBus.ErrorMessage)
    
// new PK gets auto-updated after save
int id = customer.Id;
    
// load a new customer instance by Pk and make a change
var customer2 = customerBus.Load(id);
customer2.Updated = DateTime.Now;

// Also use alternate way to add another customer
var customer3 = new Customer() {
        LastName = "Egger",
        FirstName = "Markus",
        Entered = DateTime.Now
}
customerBus.NewEntity(customer3);  // attach customer as new

// both the updated and the new customer entities are saved
Assert.IsTrue(customerBus.Save(),customerBus.ErrorMessage)
        
// delete the first customer by pk
Assert.IsTrue(customerBus.Delete(id));
```

Although a business object by default maps to an entity type, the business object
is not bound to the entity. Internally the business object can manipulate the 
entire model accessible via the Entity instance.

###Connection Strings###
By default a business object - like a DbContext object - is instantiated with a 
default constructor which looks for a connection string entry in the .config file
with the same name as the DbContext instance. This is the recommended way to set
up the business object since it's easy, yet also configurable via the connection
string entry.

If you require custom connection strings you'll need to create custom constructors
that point back at the business object base constructor and allow for custom connection
strings:

```C#
public class busCustomer : EfCodeFirstBusinessBase<Customer,OrdersContext>
{ 
     // uses default connection string (dbContext name)
     public busCustomer() 
     { }

     public busCustomer(string connString) : base(connString)
     { }
}
``` 

###Adding to the Business Object###
The previous operations are not that different from plain EF CodeFirst operations, except
for some simplified CRUD operations based on IDs and auto-attachment. The real value
of a business object comes from encapsulation of business operations in methods of the
business object. Internally the business object can use those same CRUD operations,
and also override a host of provide hook methods for common tasks.

Here are some common hook methods to override:

```c#
public class busCustomer : EfCodeFirstBusinessBase<Customer,OrdersContext>
{ 
	public override void OnNewEntity(Customer entity)
	{
		entity.Entered = DateTime.UtcNow;
	}
	public override bool OnBeforeSave(Customer entity)
	{
		entity.Updated = DateTime.UtcNow;
	}
	public override void OnValidate(Customer entity)
	{
		if (string.Empty(entity.LastName + entity.FirstName + entity.Company)
			this.ValidationErrors.Add("Please provide at least one name");
	}
}    
```
*under construction - to be continued*

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
are similar to common EFCF operations but provider easier operation, like automatic
context attachment, ID based lookups and deletes and many other small conveniences. 
Most of these are thin wrappers around standard EF behavior, but they reduce 
code significantly.

The business object also provides consolidated validation for EF CodeFirst Model validation 
as well as code based validation rules via implementation of an OnValidate() method in
the business object. The validation routines include a ValidationErrors collection that
provides both EF model based errors as well as errors added via code. This allows maximum
flexibility when creating dealing with validation logic that requires complex operations
or data lookups.

during save operations and consistent and consistent error 
checking and reporting for data operations and saves. 

The internal interface provides many before/after hooks for data operations and internal
overrides for validation logic that make it very easy to create consistent business logic quickly.

Optionally you can also use a custom EfCodeFirstContext overload of the DbContext object, which
provides access to a low level Data Access Layer to stored procedure access or data operations
that are too complex to manage through LINQ operations - it's an easy to use escape route to
fall back to low level SQL when LINQ is too obtuse or for data operations (like stored proc calls)
that are not easily handled through LINQ or Entity Framework natively.

