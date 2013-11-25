#West Wind Toolkit Change Log


###Version 3.00
*not released yet

* **Switch to new .NET libraries for MVC5, WebAPI2 and EF6**<br/>
Updated to the latest version of the VS2013 release cycle. Updated
all libraries to use the latest NuGet components and fixed up any 
code changes

* **JSON File Provider added**<br/>
You can now store configuration optionally using JSON. The new JsonFileConfigurationProvider
uses JSON.NET to provide JSON configuration output. JSON.NET is dynamically linked so
there's no hard dependency on it. If you use this provider make sure JSON.NET is added
to your project.

* **DatabaseAccess.Timeout for CommandTimeout**
Provide ability to set the command timeout for any sql operation through the 
main SqlDataAccess class interface. This property controls how long a query
runs before it times out (maps to Command.CommandTimeout).