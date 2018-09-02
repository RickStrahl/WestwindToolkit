# West Wind Toolkit Change Log

### 3.0.0
*not released yet*

* **Move Westwind.Utilities to separate Repository and add .NET Standard Support**  
Remove `westwind.utilities` project to a new Git Repository that is more focused. The new separated westwind.utilities package has also switched to a new SDK style project and supports .NET 4.0, .NET 4.5 and .NET Standard 2.0 targets. For this project westwind.utilities is now imported as a package reference instead of a project reference.

### 2.80

* **WebUtils.SanitizeHtml()**  
Added a rudimentary RegEx based HTML sanitation routine to remove script execution from HTML. Removes various tags (script, iframe, form, etc.), `javascript:` tags and JavaScript event handlers. This is meant as a rudimentary sanitizer. For complete sanitation it's recommended you use a dedicated tool which can configure more protection options.


### 2.72
*May 18th, 2017*

* **StringUtils.TokenizeString() and DetokenizeString()**  
<small>westwind.utilties</small>  
Added a function that looks for a string pattern based on start and end characters, and replaces the text with numbered tokens. DetokenizeString() then can reinsert the tokens back into the string. Useful for parsing out parts of string for manipulation and then re-adding the values edited out.

* **StringUtils.GetLines() optional maxLines Parameter**  
<small>westwind.utilties</small>  
Added an optional parameter to `GetLines()` to allow specifying the number of lines returned. Internally still all strings are parsed first, but the result retrieves only the max number of lines.

* **StringUtils.GenerateUniqueId() additional characters**
You can now add additional character to be included in the unique ID in addition to numbers and digits. This makes the string more resilient to avoid dupe values.

* **Add support for HMAC hashing in Encryption.ComputeHash()**
<small>westwind.utilties</small>
HMAC provides a standardized way to introduces salted values into hashes that results in fixed length hashes are not vulnerable to length attacks. ComputeHash now exposes HMAC versions of the standard hash algorithms.

* **Add Encryption.EncryptBytes() and Encryption.DecryptBytes()**  
<small>westwind.utilties</small>
Added additional overloads that allow passing byte buffer for the encryption key to make it easier to work with OS data API.


### 2.70
*December 15th, 2016*

* **Fix binary encoding for extended characters in Encryption class**  
<small>westwind.utilties</small>
Binary encoding now uses UTF encoding to encrypt/decrypt strings in order to support extended characters.

* **Encryption adds support for returning binary string data as BinHex**  
<small>westwind.utilties</small>
You can now return binary values in BinHex format in addition to the default base64 encoded string values.

* **FileUtils.GetPhsysicalPath()**  
<small>westwind.utilties</small>
This function returns a given pathname with the proper casing for the file that exists on disk. If the file doesn't exist it

* **Fix Encoding in HttpUtils**  
Fix encoding bug that didn't properly manage UTF-8 encoding in uploaded JSON content.

### 2.69
*October 4th, 2016*

* **Add ErrorHandlingMode to EfCodeFirstBusinessBase**  
<small>Westwind.Data</small>  
You can now specify wether errors are handled and returns as result values, or thrown as exceptions using the `ErrorHandlingMode` property. The default as before is `CapturedErrors` which captures errors and then returns error results and error messages, or `ThrowExceptions` which re-throws the captured exceptions.

* **Add HTTP Timeout to HttpUtils HTTP Methods**  
<small>Westwind.Utilities</small>  
All the HttpUtils methods now can set the request timeout in the `HttpRequestSettings` parameter object passed to the request to allow terminating requests after a timeout period.

* **Add FileUtils.SafeFilename**  
<small>Westwind.Utilities</small>
Creates a safe filename and path that strips out all invalid characters.

* **Add `LastSql` Property to DataAccess DAL**  
<small>Westwind.Data</small>  
The last SQL statement executed against the DataAccess object is now captured in the `LastSql` property to simplify debugging.

* **Expando object from Dictionary<string,object>**  
<small>Westwind.Utilities</small>
You can now load up an Expando object directly from a dictionary via overridden constructor.

* **Fix Expando JSON Serialization**  
<small>Westwind.Utilities</small>
Expando now properly serializes JSON for dynamically added values.

* **Add better Parameter Support to Sql DataAccess**  
<small>Westwind.Data</small>   
The DAL Data access components now allow you to specify how the higher level functions generate parameters. A new `UsePostionalParameters` writes only the `ParameterPrefix` now, not the name. You can also specify the name of the character for wrapping high level SQL commands fired from within methods using `LeftFieldBracket` and `RightFieldBracket`.


### Version 2.66
*January 12, 2016*

* **SqlDataAccess.QueryList<T>()**  
<small>Westwind.Utilities</small>  
New method that returns List<T> for a query into an entity. This method directly returns a list and is slightly more efficient than the Query<T>() (which returns an IEnumerable) plus .ToList() as it just directly grabs elements into the list.

* **resizable jQuery Plugin in ww.jquery.js**  
<small>Westwind.Web</small>    
Added a minimal jQuery().resizable() plugin to allow simple resizing of elements via sizing handle element.

* **debounce() function in ww.jquery.js**  
<small>Westwind.Web</small>
Added a debounce() function that delays multiple quick firing events to a specified timeout. Minimizes many often useless repeating events on UI operations such as resizing or dragging.

* **ww.jquery.js refactoring**
<small>Westwind.Web</small>
Cleaned up a number of minor issues in ww.jquery.js. Fix small issue in jquery-watch when using old IE versions and MutationObserver option in FireFox. Remove shadow functionality from the tooltip plug-in for IE old - now only CSS is used which removes a bunch of otherwise redundant code. Removed other IE old shadow effects from various plugins resulting in reduction of size.

* **Add TextWriter support to MVC ViewRenderer**  
<small>Westwind.Web.Mvc</small>  
Add TextWriter output support to the ViewRenderer class for better compatibility with MVC and to support output from large templates directly to stream or file rather than to string first.

* **Fixes to UrlEncodingParser**   
<small>Westwind.Utilities</small>  
Fix a number of small issues in the UrlEncodingParser class used to create and parse UrlEncoded content easily. Fixed several null exception scenarios.

* **HttpClient.UseGZip optimizations**   
<small>Westwind.Utilities</small>  
The HttpClient class now uses the built in decompression tooling in .NET for HttpWebRequest to handle GZip/Deflate compression on request and response content removing the manual filtering. 

* **HttpClient.HttpTimings object to track Request Time**  
<small>Westwind.Utilities</small>  
HttpClient now includes an HttpTimings sub object that contains started and end times for a request. End times provide for first byte and last byte timings as part of every request (except for stream results).

### Version 2.64
*June 25th, 2015*

* **JsonVariables prevent XSS by encoding < and > in JSON**<br/>
<small>Westwind.Web</small>  
The JsonVariables utility that allows embedding of server side data into client script has been updated to generate < and > tags as encoded strings to prevent XSS attacks when rendering.

* **CallbackHandler improved JSON.NET Suppport**<br/>
<small>Westwind.Web</small>   
Switched to hard linked JSON.NET support in CallbackHandler instead of the previous dynamic loading to avoid the assembly reference to JSON.NET. This fixes odd version incompatibilities that have been reported as well as improving JSON performance slightly.

* **Add Async Support for HttpClient**
<small>Westwind.Utilities</small>   
Added support for Async methods to the HttpClient Class for DownloadBytesAsync() and DownloadStringAsync(). Also optimized throughput and fixed explicit disposal of one of the internal streams that previously slowed down high volume requests.
 
* **ImageUtils.NormalizeJpgImageRotation**  
<small>Westwind.Utilities</small>  
Method that looks at Exif Orientation data in a jpeg file and rotates the image to match the orientation before removing the Exif data. Useful when capturing images from mobile device which often are natively rotated and contain.

* **ImageUtils.StripJgpExifData**  
<small>Westwind.Utilities</small>  
Removes Exif data from Jpg images. Helps reduce size of images and normalizes images and keeps them from auto-rotating.


### Version 2.63
*April 30th, 2015*

* **CallbackExceptionHandlerAttribute for MVC Controllers**<br/>
<small>Westwind.Web.Mvc</small><br/>
Added CallbackExceptionHandlerAttribute to make it easy to throw 
CallbackException objects and have those exception objects handled
and returned as JSON errors with appropriate HTTP status codes. Simplifies
explicit application error responses to clients. Handler also captures
other exceptions but as generic 500 errors using consistent format.

* **CallbackResponseMessage and CallbackErrorResponseMessage Classes for JSON Results**<br/>
<small>Westwind.Web.Mvc</small><br/>
Added explicit CallbackErrorResponseMessage and CallbackResponseMessage 
classes responsible for returning properly JSON formatted message to clients.
Used to return error results from JSON callbacks in a consistent manner with
a isError flag used to determine error status. Works in conjunction with
CallbackException() in CallbackHandler implementation and in MVC BaseController.

* **AppConfiguration ConfigurationFileConfigurationProvider Property Only Serialization**<br/>
<small>Westwind.Utilities</small><br/>
Changed behavior of the Config file configuration provider to only serialize/deserialize properties. Originally both properties and fields were serialized, but in light of all the other serializers only working with properties removed the field serialization feature. This also makes it easier to create non-serialized fields that might still have to be externally visible to other classes which caused a number of reported issues in the past.

### Version 2.62
*March 31st, 2015*

* **New AlbumViewerAngular Sample Application**</br>
Added a new sample application that uses Angular JS and demonstrates using the various West Wind tools in an SPA style ASP.MVC application using Westwind.Data and Sql Server. 

* **UrlEncodingParser.DecodePlusSignsAsSpaces**</br>
<small>Westwind.Utilties</small>
Add option to support parsing + signs as spaces in UrlEncoded content. By default spaces are expected to be encoded with %20, but some older applications still use + as the space encoding character. Off by default and should be set using the constructor.

* **Add JpegCompression Option to ImageUtils.ResizeImage and RotateImage**</br>
<small>Westwind.Utilties</small>
You can now specify the jpeg quality by providing a jpeg compression level between 0 and 100. This allows control over the compression level unlike previously which used the relatively low default compression level used when no custom encoder is used. This allows for creating higher quality jpeg images.

* **CallbackHandler JSON.NET Improvements**</br>
<small>Westwind.Web</small>
Added default support for enum as string handling to CallbackHandler so that enums serialize/deserialize from string values rather than ordinals. Implemented JSON.NET instance caching rather than dynamic loading to improve performance of JSON.NET serialization.

* **CallbackException StatusCode**</br>
<small>Westwind.Web</small>
Added a status code property to the CallbackException instance in order to allow anything that uses CallbackException like CallbackHander to decide what status code to return on exceptions. 

* **JsonSerializationUtils.FormatJsonString() to prettify Json**</br>
<small>Westwind.Utilties</small>
Added method to format an input JSON string to a nicely formatted JSON string.

* **ww.angular.js Helper for a few AngularJs Tasks**</br>
<small>Westwind.Web</small>
Capture and parse $http service errors consistently. Turn regular $q promises into 
$http service compatible promises. Resolve/Reject $q promise helpers.

### Version 2.59
*January 21st, 2015*

* **Update to jQuery CSS/Attribute Watcher Plug-in**</br>
<small>Westwind.Web/ww.jquery.js</small>
Update this plug-in to work properly with newer browser versions. Switch
to MutationObserver API for much better performance and better modern
browser support. Fix jQuery version newer than 1.8.3 bug in the plug-in.
Added support for monitoring attribute changes with the attr_ prefix
(ie. to monitor class attribute changes: attr_class). Slight interface change passing parameters as an `options` parameter

* **New HttpUtils.JsonRequest**</br>
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

* **Fix Expando Object Serialization**<br/>
* <small>Westwind.Utilities</small>
Fixed bug that caused Expando objects to only serialize dynamic properties. Fixed code to ensure both static and dynamic properties are serialized in JSON.NET and XMLSerializer. 


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
