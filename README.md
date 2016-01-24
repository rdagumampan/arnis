#TechStackTracker
A simple dependency tracker for projects using elementary parsing algorithm.

By consistently monitoring a technology stack in our application portfolio, we can perform threat modeling, plan for upgrades, monitoring licensing or decommisioning of projects and tools.

- You may have applications build on all version of Visual Studio from 2001 to 2015
- A critical security report requires all applications using a component to upgrade (OpenCA)
- You maybe suprised your company uses 3 types of OR mapping tool, 5 kinds of DI frameworks, runs unit tests on nUnit, MSTEST, xUNit etc ;)

###Trackers
Implements IStackTracker which scans the entire folder to perform analysis.

- VisualStudioTracker
Scans working folder for solution files and identify the version of Visual Studio used.
- ReferencedAssembliesTracker
Scans working folder for project files and identify all DLL used in the project. System DLLs are also included.

###Sinks
Implements IStackReportSInk to write specific format.

- CsvStackReportSink
Save the report into CSV file

###Next Steps
- Fix bug when project files is nested deep
- Support skipping folders
- More unit tests