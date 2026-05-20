# saSamplesAndTests
## Net48_Service
This sample creates a windows service.  
The service does nothing except record its start and end in a log file.
But it's enough for a sample.

### Important NOTE
In most cases you must run this example with administrator privileges.

### Command line
```
Net48_Service.exe [/i|/u|/s|/c] [InstanceName]
```
- /i installs the service named Net48(InstanceName). 
- /u removes the service named Net48(InstanceName). 
- /s starts the service named Net48(InstanceName). 
- /c (default) starts Net48(InstanceName) as an console application.

The default for InstanceName is "default".

### class WinService_Service
WinService_Service is derived from System.ServiceProcess.ServiceBase and is called from
windows service manager to start and stop the application called as service.

### class WinService_Installer
WinService_Service is derived from System.Configuration.Install.Installer.
It provides the informations for the install an uninstall routines.

Additionally, the static functions InstallMe and UninstallMe are provided to enable installation without an external utility. On top changes InstallMe the registry value "ImagePath" to support multiple named instances of this service.  

### class WinService_Controller
WinService_Controller represents the actual application.

You should to modify the Start and Stop functions so that they actually perform a meaningful task.  
Currently, nothing happens here at all, other than the logging of the Start and Stop events.
