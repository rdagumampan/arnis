###TechStackTracker
A simple dependency tracker for projects using elementary parsing algorithm.

By consistently monitoring a technology stack in our application portfolio, we can perform threat modeling, plan for upgrades, monitoring licensing or decommisioning of projects and tools.

Supports these use cases:
- Identify what applications are built on Visual Studio from 2001 to 2015.
- Identify GAC and DLL dependencies used in each project in your solution.
- Extensible to suport new trackers and sinks.

####Trackers
Trackers scans the entire workspace folder to perform analysis of solutions and projects to build dependency tree.
To create your own tracker, implement IStackTracker interface.

- VisualStudioTracker
Scans working folder for solution files and identify the version of Visual Studio used.
- ReferencedAssembliesTracker
Scans working folder for project files and identify all DLL used in the project. System DLLs are also included.

####Sinks
Sinks are writes the tracker's result into specific format or destination.
To create your own sink, implement IStackReportSink interface.

- CsvStackReportSink
Save the report into CSV file

####Next Steps
- Fix bug when project files is nested deep
- Support skipping folders
- More unit tests

####Future
- Deploy as microservice in cloud where users can upload zip and returns a full dependency report
