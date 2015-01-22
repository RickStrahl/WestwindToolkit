# West Wind Data
### Light Weight Business Object Library for Entity Framework Code First###

This library is a light weight business object framework around Entity Framework Code First.
It provides a simple way to encapsulate your business logic in one place, with the following
features:

* Wraps and manages DbContext instance
* Wrapped CRUD operations with DbContext lifetime management</br>
  (Load,Save,Delete,NewEntity,Validate operations)  
* Simplifies saving and updating of data consistently
* Model and code based validatation support
* Pre and Post hooks for CRUD operations
* Error and data conflict management and reporting
* Consistent error model and trapping
* Optional internal Entity member simplifies single model operations
* Simplifies data access - especially CRUD - to single line operations
* **Full access** to EF CodeFirst functionality
* Optional custom DbContext with low level Data Access Layer
     * Full featured ADO.NET Data Access Layer
     * Single line DAL methods
     * Easy Stored Procedure Calls
	 * SQL String queries to read-only Entity mapping
     * Full range of DAL operations
     
More Info:
* [Part of the West Wind Toolkit](http://west-wind.com/westwindtoolkit/)
* [Get it on NuGet](http://nuget.org/packages/Westwind.Data/)
* [Class Reference](http://west-wind.com/westwindtoolkit/docs/?topic=Class%20EfCodeFirstBusinessBase%3CTEntity,TContext%3E)

### How it Works
Using this library you implement business objects that are associated with an Entity Framework Code First Model and a top level EF entity object. You can inherit from EFCodeFirstBusinessBase<TEntity,TContext> and the resulting class then acts as a business object that can provide a logical container for your business logic code. 

Although the business object associates with a top level entity, you typically build business objects that can encompass operations against multiple tables of the database. For example an Order business object typically encapsulates the order header and line items as well as some customer and shipping record operations which would all live in a an Order business object. The entity association is merely to simplify default operations so no explicit entity references need to be passed.

The business object allows you to encapsulate your business logic and data access code against EntityFramework into the business layer without bleeding EF functionality into the application layer. This makes for easier testing and allows for easier isolation of data related code, as well as migration to other data platforms or changed behavior in the future.

### Benefits

##### Simplified CRUD Operations
The external interface of the business object provides core CRUD operations that 
are similar to common EF operations but provide easier operation, like automatic
context attachment, ID based lookups and deletes and many other small conveniences. Use single line `Load()`, `Save()`, `NewEntity()`, `Delete()`, `Validate()` operations to simplify CRUD operations. Most of these are thin wrappers around standard EF behavior, but they can reduce code significantly and keep Entity Framework semantics out of application code. You still retain full access to the underlying DbContext, but you should 

##### Validation
The business object also provides consolidated validation for EF CodeFirst Model validation 
as well as code based validation rules via implementation of an `OnValidate()` method in
the business object. The validation routines include a `ValidationErrors` collection that
provides both EF model based errors as well as errors added via code. When the `Save()` method is called both the EF model validation (ie. Model Attributes and Model Validators) as well as code based validation that allows you to validate across the entire model and not just a single entity.   This allows maximum flexibility when creating dealing with validation logic that requires complex operations or data lookups.

##### Interception Hooks
The internal interface provides many before/after hooks for data operations and internal
overrides for validation logic that make it very easy to create consistent business logic. You can inject check and update code when loading and saving data.

##### Optional extended DbContext Object
Optionally you can also use a custom <code>EfCodeFirstContext</code> subclass of the DbContext object, which provides access to a low level Data Access Layer to stored procedure access or data operations that are too complex to manage through LINQ operations - it's an easy to use escape route to fall back to low level SQL when LINQ is too obtuse or for data operations (like stored proc calls) that are not easily handled through LINQ or Entity Framework natively.

##### DbContext Management
Each business object instance gets its own DbContext instance that is internally managed and maintained. All operations in the same business object thus share the same DbContext. A context can be optionally assigned to another context by using custom constructors, but in general each business object has its own context to avoid potential cross talk and entity cache bloat.

The associated DbContext is created when the business object is created and automatically released when the business object is released. The business object implements IDisposable() so the standard pattern of using `using()` statements applies.

## Using Westwind.Data Business Objects

### Installation
The easiest way to install the library is via NuGet:

    PM > Install-Package Westwind.Data

Alternately you can also download and install the required assemblies from the GIT repository from the libs folder. To use this library you need:

* Westwind.Data.dll
* Westwind.Utilities.dll
* EntityFramework 6 (from NuGet)

Requirements:
* .NET 4.5 Runtime
* [EntityFramework 6 (from NuGet)](https://www.nuget.org/packages/EntityFramework)


### Getting Started
The West Wind Data library works by providing a base business object class that you inherit from. The base class wraps the DbContext and a base entity type, but the base entity type is just a convenience for top level entity operations. From within the business object you have full access to the entire DbContext model. 

The important point is that most applications have many business objects, but they don't necessarily map one to one with the database tables of the application.

### Create your EF CodeFirst Model and Database ###
The business object components works off an existing Entity Framework Code First
Model and Database, so before you create a business object you'll need to create the 
EF model and context.

Create a connection string entry in your .config file, ideally with the same name as the 
DbContext, so no parameters are required for the Context to find the connection using the default constructor.

```xml
<add name="WebStoreContext" 
     connectionString="server=.;database=OrdersSample;integrated security=true;MultipleActiveResultSets=true;" 
     providerName="System.Data.SqlClient" />
```

You can explicitly specify a connection string using custom constructors, or by changing the DbContext initialization code. More on that later.

### Create your Business Object ###
Create an instance of the business object and inherit it from EfCodeFirstBusinessBase:

```C#
public class busCustomer : EfCodeFirstBusinessBase<Customer,WebStoreContext>
{ }
```    

You specify a main entity type (Customer in this case) and the DbContext type (WebStoreContext).  You now have a functioning business object for Customers.

Note that you create many business objects for each **logical** business context
or operation which wouldn't necessarily match each entity in the data model. For example,
you would have an OrderBus business object, but likely not a LineItemBus business object since lineitems are logically associated with the Order and can be managed through an Order business object.

### Using the Business Object
Without adding any other functionality the business object is now functional and can run basic CRUD operations:

```C#
var customerBus = new busCustomer();
    
// Add a new customer
var customer = customerBus.NewEntity();
customer.LastName = "Strahl";
customer.FirstName = "Rick";
customer.Entered = DateTime.UtcNow;
    
// Save all data since last Save() operation
// Method returns true or false and sets an ErrorMessage/ErrorException
Assert.IsTrue(customerBus.Save(),customerBus.ErrorMessage)
    
// new PK gets auto-updated after save
int id = customer.Id;
    
// load a new customer instance by Pk and make a change
var customer2 = customerBus.Load(id);
customer2.Updated = DateTime.Now;

// Alternate way to add a new customer
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

### Connection Strings
By default a business object - like a DbContext object - is instantiated with a 
default constructor which looks for a connection string entry in the .config file
with the same name as the DbContext instance. This is the recommended way to set
up the business object since it's easy, yet also configurable via the connection
string entry.

If you require custom connection strings you'll need to create custom constructors
that point back at the business object base constructor and allow for custom connection
strings:

```C#
public class busCustomer : EfCodeFirstBusinessBase<Customer,WebStoreContext>
{ 
     // uses default connection string (dbContext name)
     public busCustomer() 
     { }

     public busCustomer(string connString) : base(connString)
     { }
}
``` 

The connection string can either be full connection string, or a connection string entry from your config file.

So either of the following works:

```c#
var customerBus = new busCustomer("WebStoreContext");
var orderBus = new busOrder("server=.;database=WebStore;integrated security=true");
```

### Adding to the Business Object ###
The previous operations are not that different from plain EF CodeFirst operations, except
for some simplified CRUD operations based on IDs and auto-attachment. The real value
of a business object comes from encapsulation of business operations in methods of the
business object. Internally the business object can use those same CRUD operations,
and also override a host of provide hook methods for common tasks.

Here are some common hook methods to override:

```c#
public class busCustomer : EfCodeFirstBusinessBase<Customer,WebStoreContext>
{ 
	public override void OnNewEntity(Customer entity)
	{
		entity.Entered = DateTime.UtcNow;
	}
	public override bool OnBeforeSave(Customer entity)
	{
		entity.Updated = DateTime.UtcNow;
	}
}    
```

So now when you make a simple change to the data like in the code below:

```c#
var customerBus = new busCustomer("WebStoreContext");
var cust = customerBus.Load(1);
cust.FirstName = "Ricardo";

bool result =  customerBus.Save();
if (!result)
   // do something customerBus.ErrorMessage
```

both the explicit FirstName change as well as the implicit Updated change are applied to the saved data in the database.

### Validation
The EfCodeFirstBusiness class also provides for explicit code based validation. EF already supports model validation which works well for single entity validation. However, it's limited to validating properties on the current entity. If you need to run related look up operations or access other Model Validation is not sufficient.

Using the OnValidate() method override you can create code based validations that have access to the entire model and context, so it's possible to validate across multiple entities and even perform operations against the data base. 

```c#
protected override void OnValidate(Customer entity)
{
    // dupe check if entity exists
    if (IsNewEntity(entity))
    {
        if (Context.Customers
            .Any(c => c.LastName == entity.LastName &&
                        c.FirstName == entity.FirstName))
        {
            ValidationErrors.Add("Customer already exists");
            return;
        }
    }
    
    // simple validations        
    if (string.IsNullOrEmpty(entity.LastName))
        ValidationErrors.Add("Last name can't be empty");
    if (string.IsNullOrEmpty(entity.FirstName))
        ValidationErrors.Add("First name can't be empty");      
}
```

To specify validation errors simply add ValidationErrors to the `ValidationErrors` collection. When the collection count is greater than 0 the validation fails.

### Checking for Validation
There are two ways to check for validation before saving:

* Explicitly calling the `Validate()` method
* Setting `AutoValidate=true` on the business object

Explicitly calling `Validate()` looks like this:

```c#
var customerBus = new busCustomer();

// load an existing customer to create a 'dupe'
var custExisting = customerBus.Load(1);

// assign duped values to a new customer record
var cust = new Customer()
{
    // create dupe values which should fail validation
    FirstName = custExisting.FirstName,
    LastName = custExisting.LastName,                
    Company = custExisting.Company
};
cust = customerBus.NewEntity(cust);


// this will fail 
if (!customerBus.Validate())
{
   // Customer already exists
   Console.WriteLine(customerBus.ErrorMessage);
   return;
}

if (!customerBus.Save())
{
   Console.WriteLine(customerBus.ErrorMessage);
   return;
}
```

You can use the AutoValidate property instead which causes the validation automatically to fire anytime you explicitly

```c#
var customerBus = new busCustomer()
{
    // Validates on Save automatically
    AutoValidate = true
};
           
var custExisting = customerBus.Load(1);
var cust = new Customer()
{
    // create dupe values which should fail validation
    FirstName = custExisting.FirstName,
    LastName = custExisting.LastName,
    Company = custExisting.Company
};
cust = customerBus.NewEntity(cust);

if (!customerBus.Save());
   Console.WriteLine(customerBus.ErrorMessage);
else
   Console.WriteLine("Saved");
```

### Sample Business Object
Here's a small example of a sample business object that shows a few of the different operations you might handle in a typical business object:

```c#
public class busCustomer : EfCodeFirstBusinessBase<Customer, WebStoreContext>
{
    public busCustomer()
    { }

    public busCustomer(string connectionString) : base(connectionString)
    { }

    public busCustomer(IBusinessObject<WebStoreContext> parentBusinessObject)
        : base(parentBusinessObject)
    { }


    // Typical Query Methods

    public IEnumerable<Customer> GetActiveCustomers()
    {
        DateTime dt = DateTime.UtcNow.AddYears(1);
        return Context.Customers
                      .Where(cust => cust.Updated > DateTime.UtcNow.AddYears(-2));
    }
    public IEnumerable<Customer> GetCustomerWithoutOrders()
    {
        return Context.Customers
            .Where( cust=> !Context.Orders.Any(ord=> ord.CustomerPk == cust.Id));
    }

    // Utility/Helper methods    

    public string EncodePassword(string plainPasswordText)
    {
        return Encryption.EncryptString(plainPasswordText, "seeekret1092") + "~~";
    }


    // Override injection hook methods    

    protected override bool OnBeforeSave(Customer entity)
    {
        // encode password if it isn't already
        if (!string.IsNullOrEmpty(entity.Password) && !entity.Password.EndsWith("~~"))
            entity.Password = EncodePassword(entity.Password);

        entity.Updated = DateTime.UtcNow;
        
        // true means save is allowed
        // return false to fail
        return true;
    }

    // Validation

    protected override void OnValidate(Customer entity)
    {
        // check if entity exists
        if (IsNewEntity(entity))
        {
            if (Context.Customers
                .Any(c => c.LastName == entity.LastName &&
                            c.FirstName == entity.FirstName))
            {
                ValidationErrors.Add("Customer already exists");
                return;
            }
        }
        
        if (string.IsNullOrEmpty(entity.LastName))
            ValidationErrors.Add("Last name can't be empty");
        if (string.IsNullOrEmpty(entity.FirstName))
            ValidationErrors.Add("First name can't be empty");      
    }
} 
```

### Custom DbContext
This libary also provides an optional DbContext extension that provides access to a simplified raw Data Access Layer for direct SQL commands. The interface is based on [Westwind.Utilities.DataAccess](http://west-wind.com/westwindtoolkit/docs/_3ou0v2jum.htm) which provides the DAL implementation that is part of the Westwind.Utilities support library. This can be used on any DbContext instance to provide DAL features.

#### Set up a custom Context
To use this functionality simply create your context by inheriting from `Westwind.Data.EfCodeFirstContext`:

```c#
public class WebStoreContext : EfCodeFirstContext  // instead of DbContext
{ 
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<LineItem> LineItems { get; set; }        
}
```

Once you do this your Context instance now has a `.Db` property that you can access to run queries.

Inside of a business object method you can then do something like this:

```c# 
var time = new DateTime(2000, 1, 1);
var custs = Context.Db
		.Query<Customer>("select * from Customers where entered > @0",time);

var maxId = (int) Context.Db
			.ExecuteScalar("select Max(id) from Customers")

var count = Context.Db.ExecuteNonQuery("delete for password = ''")
```

These examples are not very useful as they are easily achievable with LINQ. Custom data access can be useful for commands that require complex SQL statements that might be simpler to execute as SQL strings rather than LINQ commands. LINQ can be a bear.

DbContext does provide the Database property but it's limited to SqlQuery() and ExecuteSqlCommand(). If you need additional functionality the Db instance provides more control and options to return and execute Db commands directly in a richer way. 

This is to say if you don't need this extended functionality and can work within what EF provides natively, you don't need this functionality. Add only as needed.
