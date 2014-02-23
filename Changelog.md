#West Wind Toolkit Change Log

###Version 2.51
* under construction *
* **Added Slide Transition plug-in to ww.jquery**
This tiny plug-in provides slideUp()/slideDown() like behavior for jquery
using CSS transitions. These transitions tend to be very jerky on mobile
so having a universal replacement is a common scenario.

* **Fixed up tests**
Fixed entity framework DbInitializer to properly autocreate testdata
and run. Db Tests still fail occasionally on first run, but succeed
on subsequent runs. Also fixed several tests by moving hard coded
resources into the output folder under SupportFiles.

* **Experimental: Westwind.Data.MongoDb**
Created a MongoDb version of the Westwind.Data component that provides
most of the same CRUD and Validation functionality of the Westwind.Data
libraries. Wraps many of Mongo's data features. No documentation yet.



###Version 2.50
January 20th, 2014

* **Libraries moved to .NET 4.50**<br/>
In order to update to the latest versions of MVC5, WebAPI2 and EF6
we need to target .NET 4.50. Updated libraries to match this. Only
library that's left at 4.0 is Westwind.Utilities since it has no
specific 4.5 dependencies at this point.

* **Switch to new .NET libraries for MVC5.1, WebAPI2.1 and EF6**<br/>
Updated to VS2013 RTM. Updated all libraries to use the RTM NuGet 
components and fixed up any code changes.

* **Update JSON date parsing in ww.jquery.js**
<small>Westwind.Web</small>
Updated the JSON date parsing in ww.jquery.js to be automatic
by optionally allow replacement of the JSON parser with an
parser plug-in to auto-parse dates. This provides global date
parsing on the page level. Also, added flag to opt-in for 
MS AJAX data parsing to avoid parsing overhead.

* **JSON Configuration File Provider added**<br/>
<small>Westwind.Utilies</small>
You can now store configuration optionally using JSON. The new JsonFileConfigurationProvider
uses JSON.NET to provide JSON configuration output. JSON.NET is dynamically linked so
there's no hard dependency on it. If you use this provider make sure JSON.NET is added
to your project.

* **DatabaseAccess.Timeout for CommandTimeout**<br/>
<small>Westwind.Utilies</small>
Provide ability to set the command timeout for any sql operation through the 
main SqlDataAccess class interface. This property controls how long a query
runs before it times out (maps to Command.CommandTimeout).

* **StringUtils.GetLines**<br/>
<small>Westwind.Utilies</small>
Splits a string into lines based on \r\n or \n separators.

* **StringUtils.CountLines**<br/>
<small>Westwind.Utilies</small>
Returns a line for an input string.

* **DataUtils.GetRandomNumber**
<small>Westwind.Utilies</small>
Returns a random integer in a range between high and low
values. Uses the Random class with a static key. Simple
wrapper around random API to make it easier to create
random integers in a single line of code.

* **New JsonSerializationUtils class**
<small>Westwind.Utilies</small>
Mimics behavior of the SerializationUtils class (XML) but
uses JSON.NET to provide the same string and file serialization
services with single method calls. JSON.NET dependency is a 
soft, dynamic reference so there's no hard dependency on JSON.NET.

* **Bug Fixes**
  * Westwind.Web: UserState parsing when the userID is a string
    Fixes issue where missing UserIdInt was failing and setting
    the string to 0.
  * Westwind.Web.Api: Fix BasicAuthFilter and BasicAuthMessageHandler
    for scenario where password contains a colon (:).
  * Westwind.Utilities: Fix DataAccessBase::Query<T>() parameter error 
    when first paramameter is string. Broke out Query<T> and 
    QueryWithExclusions<T> to properly separate the property exclusions.
  * Westwind.Utilities: Make SerializationUtils file read operations
    read-only to limit file access conflicts. Fix re-encryption bug
    in XML Configuration provider.
