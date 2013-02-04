REM This batch file updates the ..\Assemblies and template directories
REM with the latest copies of the core ..\Assemblies
copy ..\Westwind.Utilities\bin\release\Westwind.Utilities.dll 
copy ..\Westwind.Utilities\bin\release\Westwind.Utilities.xml 

copy ..\Westwind.Data\bin\release\Westwind.Data.dll 
copy ..\Westwind.Data\bin\release\Westwind.Data.xml 

copy ..\Westwind.Web\bin\release\Westwind.Web.dll 
copy ..\Westwind.Web\bin\release\Westwind.Web.xml 

copy ..\Westwind.Web.Mvc\bin\release\Westwind.Web.Mvc.dll 
copy ..\Westwind.Web.Mvc\bin\release\Westwind.Web.Mvc.xml 

pause