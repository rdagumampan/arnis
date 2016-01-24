#TechStackTracker
A simple dependency tracker for projects using elementary parsing algorithm.

By consistently monitoring a technology stack in our application portfolio, we can perform threat modeling, plan for upgrades, monitoring licensing or decommisioning of projects and tools.

- You may have applications build on all version of Visual Studio from 2001 to 2015
- A critical security report requires all applications using a component to upgrade (OpenCA)
- You maybe suprised your company uses 3 types of OR mapping tool, 5 kinds of DI frameworks, runs unit tests on nUnit, MSTEST, xUNit etc ;)

##Trackers##
Implements IStackTracker which scans the entire folder to perform analysis.

##Sinks##
Implements IStackReportSInk to write specific format.
Default format is CSV.