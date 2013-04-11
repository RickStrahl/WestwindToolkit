#Westwind.Web
###A rich library of ASP.NET helper functionality###

<div style="font-size: 0.8em;margin-left: 15px;font-style: italic">
This is version 2.0 of the West Wind Toolkit that is still
under full construction.  This version heavily refactors
the older versions, breaks components out more logically
and also puts components out on GitHub under their
separate library trees.
</div>

ASP.NET is a very rich platform and there's tons of useful functionality included in the box.
However, there are a still a number of common features that are either missing or take a bit
too much work to use and this library adds a ton of helper classes and utility components
that are commonly required in Web applications.

* **WebUtils**
  Static class of single method helpers that provide many URL and Path fixups,
  FormVariable binding, setting User Locale, accessing resources,
  Json Encoding for strings and dates, Enabling GZip encoding, forcing
  pages to refresh and much more.

* **ClientScriptProxy**
  Class that helps with embedding and loading of JavaScript in 
  Web pages both Web Forms and standalone.

* **ScriptLoader**
  Facilitates loading jquery, jquery UI and ww.jquery.js and any other
  libraries by predefining where script content loads from. CDN support
  and for jquery there's resource fallback.

* **[ResponseFilterStream](http://www.west-wind.com/weblog/posts/2009/Nov/13/Capturing-and-Transforming-ASPNET-Output-with-ResponseFilter)**
  A response filter that can be used to examine the full response and
  if desired modify it. Provides a simple event based implementation that
  makes it easy to create custom text response filters for capturing or
  modifying output.
  
* **WebErrorHandler**
  Class that accepts a .NET exception and parses error information including
  most ASP.NET request information pieces into a class. ToString() or ToHtml()
  can be used to dump out the information into a log or for display.

* Many static helper classes with tons of utility features:
	* WebUtils
    * HtmlUtils
	* ReflectionUtils
	* SerializationUtils
	* DataUtils
	* EncryptionUtils
	* FileUtils
    * TimeUtils
    * XmlUtils    


and much, much more.

It's worthwhile to browse through the source code or the documentation
to find out the myriad of useful functionality that is available, all
in a small single assembly.

This assembly is the base for most other West Wind libraries.