####Arnis 
**Arnis.NET** is a no-brainer dependency tracker for .NET applications using elementary parsing algorithm. 

![](https://ci.appveyor.com/api/projects/status/github/rdagumampan/arnis?branch=master&svg=true)

At the moment, you can:
- track applications built on Visual Studio from 2001 to 2015.
- track target framework versions
- track referenced assemblies from nuget packages and GAC/Referenced Assesmblies folder.
- extensible to support new trackers and sinks.

By consistently monitoring the technology stack in our application portfolio, we can better plan for component upgrades, monitor 3rd party usage, consolidate component versions, or strategize decommisioning of projects and tools.

####How to use
    arnis.exe /wf:"<your_workspace_folder>" /sf:"<your_desired_csv_file>" /skf:<skip_these_folders>

Example (simplified):

	arnis.exe /wf:"c:\github\arnis" /sf:"c:\stackreport.arnis.csv"

Example (with skip file):

	arnis.exe /wf:"c:\github\arnis" /sf:"c:\stackreport.arnis.csv" /skf:"c:\skip.txt"
    
    where skip.txt contains
    c:\arnis\bin\debug
    c:\arnis\packages

####How it works
Arnis.NET scans the target workspace folder and perform analysis of solutions and projects. Then the tracker's results are consolidated to form a dependency tree .

- **VisualStudioTracker**
Scans working folder for solution files and identify the version of Visual Studio used.
- **ReferencedAssembliesTracker**
Scans working folder for project files and identify all DLL used in the project. System DLLs are also included.

Sinks saves the result into specific format or destination. Currently, only CSV file format is supported.

- **CsvStackReportSink**
Save the report into CSV file

![](https://rdagumampan.files.wordpress.com/2016/02/arnisresult.png)

####Disclaimer
Arnis.NET does not guarantee 100% reliability. This is not a runtime dependency tracer. For more sophisticated runtime analysis, consider [Dependency Walker](http://www.dependencywalker.com/), [ILSPy](https://github.com/icsharpcode/ILSpy), [NDepend](http://www.ndepend.com/) or [Reflector](http://www.red-gate.com/products/dotnet-development/reflector/) tools.

####Future
- support tracking web projects dependencies
- a sink targetting a Web API in azure and deploy simple site to view the full dependency report
- a webui to visualize dependency tree
