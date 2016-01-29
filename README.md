###Winterfern
**Winterfern** is a simple dependency tracker for .NET applications using elementary parsing algorithm. My vision is to create full technology stack report on each application covering platforms, components, techniques, and tools... but im very far from it ;)

By consistently monitoring the technology stack in our application portfolio, we can plan for component upgrades, monitor 3rd party usage and licenses, consolidate component versions, or strategize decommisioning of projects and tools.

Supported use cases:
- Identify applications built on Visual Studio from 2001 to 2015.
- Identify GAC and DLL dependencies used in each project in your solution.
- Extensible to suport new trackers and sinks.

####How to use
    C:\winterfern /wf:<your_workspace_folder> /sf:<your_desired_csv_file>

Example:
	C:\winterfern /wf:c:\github\winterfern /sf:c:\stackreport.winterfern.csv
    
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
- Support tracking SQL Server versions
- Support tracking web projects dependencies

####Future
- Deploy as microservice in cloud where users can upload zip and returns a full dependency report
