#West Wind Toolkit Change Log

### Version 2.58
*not released yet*

* **Update to jQuery CSS/Attribute Watcher Plug-in**<br />
<small>Westwind.Web/ww.jquery.js</small>
Update this plug-in to work properly with newer browser versions. Switch
to MutationObserver API for much better performance and better modern
browser support. Fix jQuery version newer than 1.8.3 bug in the plug-in.
Added support for monitoring attribute changes with the attr_ prefix
(ie. to monitor class attribute changes: attr_class). Slight interface change passing parameters as an `options` parameter

* **New HttpUtils.JsonRequest**<br/>
<small>Westwind.Utilities</small>
Added a new HttpUtils class with a JsonRequest() and JsonRequestAsync() 
methods to handle calling JSON services and automatically sending
and receiving of JSON data.

* **New HttpUtils class**<br />
<small>Westwind.Utilities</small>
Added static HttpUtils class to make it easy to make Http Requests
and specifically to make JSON service calls that can automatically
serialize and deserialize data. Class also includes a simple HTTP
retrieval routine.

* **AppConfiguration support for Nested Property Encryption**<br />
<small>Westwind.Utilities</small>
You can now specify nested fields for encryption in the provider's
PropertiesToEncrypt properties. For example, using `PropertiesToEncrypt=
"MailserverPassword,License.LicenseKey"` allows encoding the nested
license key. Supported on all configuration providers.

* **HttpClient.HttpTimings property added**<br/>
The HttpClient now supports the HttpTimings class which logs StartTime,
FirstByteTime, LastByteTime and TimeToFirstByteMs, TimeToLastByteMs

* **Split .NET 4.0 and 4.5 targets for Westwind.Utilities**<br />
<small>Westwind.Utilities</small>
Create seperate net40 target for .NET 4.0 compatible output of
Westwind.Utilities while moving forward to 4.5 with most of the
code. Start integrating a number of async features into new and
existing utility classes.

* **Improved Transaction Support for EntityBusinessBase.Save()**<br />
<small>Westwind.Data</small>
Added new overridable `CreateTransactionScope()` method that is used
to create a TransactionScope to wrap Save() operations optionally.
Save() needs to be wrapped in case `OnBeforeSave()` or `OnAfterSave()`
methods also write data that must be part of a transaction.
Scope is created only optionally now using a new `useTransaction`
parameter. `CreateTransactionScope()` and `TransactionScopeOptions`
provide the ability to customize how the scope is created.

* **Add String Access functions to Westwind.Data.MongoDb**<br/>
<small>Westwind.Data</small>
Add support for JSON string for queries that allow using MongoDb
query syntax in strings and save operations so that it's
possible to provide the common MongoDb query syntax to execute queries. 
The various FindXXXJson() functions handle queries and SaveFromJson() 
which allows saving with a JSON entity.

* **New MongoDbDataAccess Component**<br/>
<small>Westwind.Data.MongoDb</small>
Small wrapper around the MongoDb C# driver that provides simple methods
for common query and update operations. 

### Version 2.56
*October 2nd, 2014*

* **UrlEncodingParser for QueryString and Form Data Parsing**<br />
<small>Westwind.Utilities</small>
Added this parser that allows reading and writing of query string
and form data outside of System.Web. Class reads raw UrlEncoded data
or a URL and then allows access to values as a collection for reading
and writing. You can modify values and then write out the new result 
data. When working with URLs the full URL is re-written.

* **ImageUtils.RotateImage in memory**<br />
<small>Westwind.Utilities</small>
Rotate image gains the ability to use a byte array input to rotate
images in memory.

* **String.extract() function for JavaScript**<br/>
<small>Westwind.Web/ww.jquery.js</small>
Added String.prototype.extract method to ww.jquery.js, which allows
extracting a string based on delimiters with a number of options.

### Version 2.55
* August 18th, 2014 *

* **Added Slide Transition plug-in to ww.jquery**<br />
<small>Westwind.Web/ww.jquery.js</small>
This tiny plug-in provides slideUp()/slideDown() like behavior for jquery
using CSS transitions. These transitions tend to be very jerky on mobile
so having a universal replacement is a common scenario.

* **Add :containsNoCase and :startsWith jQuery Filters to ww.jquery**<br />
<small>Westwind.Web/ww.jquery.js</small>
Added these two filters that provide jQuery search filters. The former
filter is especially useful to do dynamic page searches that show only
content that matches typed text in search boxes.

* **Add: .searchFilter() Plugin to ww.jQuery**<br />
<small>Westwind.Web/ww.jquery.js</small>
Added .searchFilter() which can be bound to a textbox and then 
tied to a list of items via selector that are filtered based on
the search text. A nice and easy way to filter longer lists
based on text input and show only matching entries.

* **ConfigurationFile Configuration Provider support for Complex Types**<br />
<small>Westwind.Utilies</small>
Added another option for serialization of flat complex objects, by 
implementing additional checks for a static FromString() method that
if found can be used to deserialize object. [more info](http://west-wind.com/westwindtoolkit/docs/?page=_1cx0ymket.htm)

* **ConfigurationFile Configuration Provider support for IList**<br />
You can now also add properties based on IList that can create simple
enumerations in your key value configs. List elements are rendered
as ListName1, ListName2, ListName3. Lists and list elements must
have parameterless constructors in order to be readable.

* **Add NegotiatedResult**<br />
<small>Westwind.Web.Mvc</small>
Add a NegotiatedResult ActionResult that returns XML, JSON, HTML
or plain text based on the Accept header. This allows the client
to determine which output serialization or view is applied. Supports
XML/JSON serialization as well as optional View to show HTML output.
JSON Serialization uses JSON.NET (unlike standard JSON response)

* **Add JsonNetResult**<br />
<small>Westwind.Web.Mvc</small>
Add JsonNet ActionResult class that allows returning JSON using JSON.NET
formatting instead of the default JavaScriptSerializer. JSON.NET is faster
and serializes more cleanly. (Note: this affects only JSON output - not 
inbound JSON parsing. Since formatting differs slightly for some times - 
namely dictionaries - you might not get two-way fidelity).

* **Add RequireSslAttribute**<br />
<small>Westwind.Web.Mvc</small>
Add RequireSslAttribute that allows to dynamically assign the flag
that decides whether SSL is used. Use a configuration setting,
a static 'delegate' method or an explicit constant bool value.

* **JsonVariables Component**<br />
<small>Westwind.Web</small>
Component that helps with embedding server side data into client side
code. From simple serialization to creating a global object you can
construct at runtime with many values, that are rendered into client
script code. Attach to global vars or properties of existing objects.
Useful for shuttling server data to client side JavaScript code.
This is a stripped down version of the older ScriptVariables component
that is optimized for string output usage in MVC or Web Pages 
removing all the WebForms related cruft.

* **WebUtils.SetUserLocale allowedLocales Option**<br />
<small>Westwind.Web</small>
Method now adds a allowedLocales parameter where you can specify
any language codes you want to support. Any non-supported languages
or language prefixes are automatically mapped to the default locale
of the server. This reduces the amount of lookups for invalid locales
in your localization providers when automatically mapping browser
resources to localized resources as each locale referenced must be
looked up in the resource loaders.

* **TimeUtils.Truncate to Truncate DateTime values**<br />
<small>Westwind.Utilies</small>
Strip off milliseconds, seconds, minutes, hours etc. from
date time values to 'flatten' date values easily.

* **Fixed up tests**<br />
<small>Westwind.Utilies</small>
Fixed entity framework DbInitializer to properly autocreate testdata
and run. Db Tests still fail occasionally on first run, but succeed
on subsequent runs. Also fixed several tests by moving hard coded
resources into the output folder under SupportFiles.

* **Fix auto Gzip/Deflate decompression for in Memory Results**<br />
<small>Westwind.Utilies</small>
Fix automatic Gzip/Deflate decompression in HttpClient class. This was
previously working for file and stream based responses but not for 
string and byte[] results of the HttpClient class.

* **Fix image resizing algorithm for square images**<br />
<small>Westwind.Utilies</small>
Fix small bug with image resizing when the image is square. Now
properly resizes to the largest width/height dimension specified.
Previously always used width. Also added ImageInterpolationMode
to the full signature. Thanks to Matt Slay for his help on these.

* **Experimental: Westwind.Data.MongoDb**<br />
<small>Westwind.Data.MongoDb</small>
Created a MongoDb version of the Westwind.Data component that provides
most of the same CRUD and Validation functionality of the Westwind.Data
libraries. Wraps many of Mongo's data features. No documentation yet.

* **Experimental: MongoDb Log Adapter**<br />
<small>Westwind.Data.MongoDb, Westwind.Utilities</small>
Add a new 


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
