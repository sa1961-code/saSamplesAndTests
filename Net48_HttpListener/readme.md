# saSamplesAndTests
## Net48_HttpListener

### Important NOTE
You must run these examples with administrator privileges.

To avoid requiring administrator privileges, you can alternatively open Port 8001 using the netsh program.  
To do this, launch a Command Prompt with administrator privileges and enter the following command.
```
netsh http add urlacl url=http://*:8001/ user=\Jeder
```

To remove the share, enter the following command.
```
netsh http delete urlacl http://*:8001/
```

### Sample01_Microsoft
- Run this example to listen once on localhost, port 8001.
- Open a Browser and visit http://localhost:8001.

### Sample02_Microsoft
- Run this example to listen asynchronously in a loop on localhost, port 8001.
- Open a Browser and visit http://localhost:8001.

### Sample03_MultiThreaded
- Run this example to listen asynchronously in a loop on localhost, port 8001, 
  and handle each request in a separate thread.
- Open a Browser and visit http://localhost:8001.
