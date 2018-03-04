# TNCServicesPlatform
Cloud Services Platform for TNC AI Project

# Dev Environment Setting
1. Install Visual Studio 2017
2. Install .NETCore SDK (2.0+)
    https://www.microsoft.com/net/download/windows

# Adding New Service
1. Add new C# library project 
2. Add project reference to the new C# library project rom #1 in TNCServicesPlatform.APIHost project
3. Register API Library Assembly in Startup.cs in TNCServicesPlatform.APIHost project
