###Winterfern
**Winterfern** is a no-brainer dependency tracker for .NET applications using elementary parsing algorithm. 

At the moment, you can:
- track applications built on Visual Studio from 2001 to 2015.
- track target framework versions
- track referenced assemblies from nuget packages and GAC/Referenced Assesmblies folder.
- extendibe to support new trackers and sinks.

My vision is to create full technology stack report on each application covering platforms, components, techniques, and tools... but im very far from it ;). I believe that by consistently monitoring the technology stack in our application portfolio, we can plan for component upgrades, monitor 3rd party usage, consolidate component versions, or strategize decommisioning of projects and tools.

####How to use
    c:\winterfern /wf:"<your_workspace_folder>" /sf:"<your_desired_csv_file>" /skf:<skip_these_folders>

Example (simple):

	c:\winterfern /wf:"c:\github\winterfern" /sf:"c:\stackreport.winterfern.csv"

Example (with skip file):

	c:\winterfern /wf:"c:\github\winterfern" /sf:"c:\stackreport.winterfern.csv" /skf:"c:\skip.txt"
    
    where skip.txt contains
    c:\winterfern\bin\debug
    c:\winterfern\packages

####How it works
Winterfern scans your target workspace folder and perform analysis of solutions and projects. The tracker's results are consolidated to form a dependency tree .

Sinks are created to save result into specific format or destination. Currently, only CSV file format is supported.

####Trackers
Trackers scans the entire workspace folder to perform analysis of solutions and projects to build dependency tree.
To create your own tracker, implement IStackTracker interface.

- **VisualStudioTracker**
Scans working folder for solution files and identify the version of Visual Studio used.
- **ReferencedAssembliesTracker**
Scans working folder for project files and identify all DLL used in the project. System DLLs are also included.

####Sinks
Sinks writes the tracker's result into specific format or destination.
To create your own sink, implement IStackReportSink interface.

- **CsvStackReportSink**
Save the report into CSV file

####Next Steps
- Support tracking web projects dependencies

####Disclaimer
Winterfern is a tool i initially developed for personal use and and no guarantee of 100% reliability. This is not runtime dependency tracer. If you need more sophisticated runtime analysis I recommend [Dependency Walker](http://www.dependencywalker.com/), [ILSPy](https://github.com/icsharpcode/ILSpy), [NDepend](http://www.ndepend.com/) or [Reflector](http://www.red-gate.com/products/dotnet-development/reflector/) tools.

####Future
- Create a sink targetting a Web API in azure and deploy simple site to view the full dependency report