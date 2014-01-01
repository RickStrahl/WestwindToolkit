#West Wind Toolkit Change Log


###Version 3.00
*not released yet

* **Libraries moved to .NET 4.51**<br/>
In order to update to the latest versions of MVC5, WebAPI2 and EF6
we need to target .NET 4.51. Updated libraries to match this. Only
library that's left at 4.0 is Westwind.Utilities since it has no
specific 4.5 dependencies at this point.

* **Switch to new .NET libraries for MVC5, WebAPI2 and EF6**<br/>
Updated to VS2013 RTM. Updated all libraries to use the RTM NuGet 
components and fixed up any code changes.

* **JSON File Provider added**<br/>
You can now store configuration optionally using JSON. The new JsonFileConfigurationProvider
uses JSON.NET to provide JSON configuration output. JSON.NET is dynamically linked so
there's no hard dependency on it. If you use this provider make sure JSON.NET is added
to your project.

* **DatabaseAccess.Timeout for CommandTimeout**<br/>
Provide ability to set the command timeout for any sql operation through the 
main SqlDataAccess class interface. This property controls how long a query
runs before it times out (maps to Command.CommandTimeout).

* **StringUtils.GetLines**<br/>
Splits a string into lines based on \r\n or \n separators.

* **DataUtils.GetRandomNumber**
Returns a random integer in a range between high and low
values. Uses the Random class with a static key.