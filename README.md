# Introduction 
The Request Management Solution Starter is a solution built to act as a starter for building solutions for managing requests.

# Getting Started
[Download the Package Deployer for the Request Management Solution](https://github.com/carltoncolter/dyn-req-mgmt/releases/download/1.0/RequestManagement-1.0.zip)

# Solution Breakdown
* Plugins: Contains the plugin source code.
* Plugins.Tests: Contains unit tests for the plugin source code
* Sample Data: Contains Sample Data XML files
* SolutionPackage: Contains the XML files used by the CRM SolutionPackager utility to package the solution
* WebResources: Contains the web resources in the solution (JavaScript, Icons, Html, etc)

# Manually Building the SolutionPackage
The solution is designed to utilize spkl.  Once you have updated the nuget packages, and have spkl, you should be able to use spkl\pack.bat to package the solution.  When you update the files from nuget, make sure it doesn't overwrite the spkl folders.

To manually package the solution without using spkl, you can run the following, with %SolutionPackage% being the path to the SolutionPackage folder in the code.

`SolutionPackager.exe /action:Pack /zipfile:"FedBizApps_RequestManagement.zip" /folder:"%SolutionPackage%\package" /packagetype:unmanaged /errorlevel:Verbose /nologo /log:packagerlog.txt /map:%SolutionPackage%\map.xml`
