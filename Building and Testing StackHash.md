# Building and Testing StackHash
Build and test on a dedicated machine. Do not run the build or tests on a live StackHash server.

Build Platform: Vista or Windows 7 with all service packs. Preferably an x64 system but will build and run on x32.

Install Visual Studio 2010 including all updates.

Install the 32bit and 64 bit versions of Microsoft Debugging Tools for Windows ([http://msdn.microsoft.com/en-us/windows/hardware/gg463009](http://msdn.microsoft.com/en-us/windows/hardware/gg463009)). Make sure you have the latest version of the debugging tools installed as older versions had problems accessing symbol servers when run from a Windows Service environment. The latest Debugging Tools for Windows installers come as part of the windows sdk installation. Select the Debugging Tools option under the Redistributable Packages to download all three versions of Debugging Tools (x86, x64, Itanium). Then run the installers for the x86 and AMD64 versions from C:\Program Files\Microsoft SDKs\Windows\v7.1\Redist\Debugging Tools for Windows. 

Install SQL Server 2008 R2 Express – including SQL Management Studio and the main SQL engine from [http://www.microsoft.com/download/en/details.aspx?id=23650](http://www.microsoft.com/download/en/details.aspx?id=23650). Install the instance as STACKHASH and create an SQL Login for NT_AUTHORITY\NETWORK_SERVICE account with SysAdmin server role.

Install Windows Live Sign-In Assistant from [http://www.microsoft.com/download/en/details.aspx?displaylang=en&id=13492](http://www.microsoft.com/download/en/details.aspx?displaylang=en&id=13492)

Install WiX 3.5 from [http://wix.codeplex.com/releases/view/60102](http://wix.codeplex.com/releases/view/60102).

The tests require that the code be built and tested from drive R:\. The drive must be accessible to the StackHash Windows Service as well as the logged on user. To do this:

# Create a batch file containing “subst r: c:\stackhash” where c:\stackhash can be replaced by the drive/folder where you want to store the stackhash code. Make sure the folder (e.g. c:\stackhash) has been created.
# Create a Task in Task Scheduler to run at System Startup using the SYSTEM account with Highest Privileges. The task should run the batch file created in step 1. This will enable the R: drive to be seen by system processes and users in elevated and non-elevated command prompts.
# Reboot, open a command prompt (elevated) and make sure the R drive is accessible.

Create a file R:\vcvars10.bat containing the following command (changing the path appropriately for your installation of Visual Studio).

call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\bin"\vcvars32.bat

## Building StackHash

# Get the latest CodePlex source to your r:\ drive. There should be r:\stackhash and r:\3rdparty folders. Note that if you download the source (rather than connecting via Team Explorer) you should right-click the ZIP, select Properties and unblock the ZIP file before extracting it.
# Open an elevated command prompt (use the Visual Studio 2010 Command Line, or make sure that you run vsvars10 so that the correct build environment is on the path. The buildall script will call this batch file if configured as above, installstackhash will not.)
# Navigate to r:\stackhash\codeplex
# Type “buildall –d –a -u -v -i >logd.txt” to build the debug version. This will display an error "System Error 1060 has occurred. The specified service does not exist as an installed service". Ignore this error, it will go away once the next step is actioned. Open logd.txt, go to the end of the file and verify that there were no errors or warnings.
# Type “installstackhash” to register the StackHash service. The StackHash service will be launched from R:\stackhash\ServiceContracts\serviceinstall\stackhashservice.exe. The build stops the service, copies the new service (debug or release as appropriate) to this location and restarts it.
# Type “buildall” with no options to see all options. –r builds the release version; -d builds the debug version; -a builds all from scratch; -i builds the installer; -u and –v update the assembly info files with the current version stored in the registry on the build machine.
# The StackHash client exe is built in R:\stackhash\Client\StackHash\$(Configuration) where $(Configuration) is debug or release.
# The -i switch builds the installer in R:\stackhash\Installer\StackHashInstaller\StackHashInstaller\bin\$(Configuration)\en-us where $(Configuration) is debug or release. Although you can run the installer on the dev machine, it is recommended that you do not do so as it will reregister the Service to be loaded from the install folder c:\program files\StackHash instead of the dev folder R:\stackhash\ServiceContracts\serviceinstall. If you do install and then wish to debug from the dev folder, you should uninstall StackHash and rerun the vcvars10.bat and installstackhash.bat file as directed above.

## Testing

The StackHash tests were designed to run on particular build machines at Cucku. However, we have attempted to release the tests as part of the package. The tests can take a number of hours to run and should be run such that they end before midnight. The tests have been run on US and UK timezones only.

The R drive must be defined for use by the StackHash service.

Manually create a folder c:\stackhashunittests and give ‘Everyone’ full access.

Create a file testmode.xml in C:\ProgramData\StackHash containing the content below corrected with your details as described.

TestMode must be set to 1 before the service is started. You should set aside one machine for build and testing only. Do not run the StackHash installer on that machine as it will overwrite the StackHash service location with c:\program files\stackhash\stackhashservice.exe instead of running the debug/release version that has just been built.

You can test the StackHash client on the build machine by setting TestMode to 0 and then restarting the service. If you run the tests with TestMode = 0, then the profiles will be deleted by the tests so make sure that TestMode=1 and the service has been restarted whenever you run the StackHash tests.

{"<?xml version="1.0"?>
  <TestMode>1</TestMode>
  <ConnectionString>Data Source=(local)\STACKHASH;Integrated Security=True;</ConnectionString>
  <WinQualUserName>myusername</WinQualUserName>
  <WinQualPassword>mypassword</WinQualPassword>
  <TestEmail1>myworkemail</TestEmail1>
  <TestEmail1Password>mysmtpserverpassword</TestEmail1Password>
  <TestEmail2>anothertestemail_canbethesameasabove</TestEmail2>
  <SmtpHost>mail.myhost.com</SmtpHost>
  <SmtpPort>587</SmtpPort>"}

To run the tests from the command line: make sure that TestMode=1 and restart the service. Then type buildall -d -a -u -v -i -t >logd.txt. All tests will be run regardless of whether they pass or not so you need to check the log file and/or the r:\results folder to determine if the tests were run successfully. 
The tests may take a couple of hours to run depending on the speed of the machine.

You can also run the tests from Visual Studio as described below.
 
## Visual Studio

There is 1 main solution for the StackHash client and 2 for the service: 

# R:\stackhash\client\stackhash\stackhash.sln – The client code.
# R:\stackhash\ServiceContracts\StackHashServiceContracts\StackHashServiceContracts.sln – contains the service infrastructure code.
# R:\stackhash\BusinessLogic\BusinessLogic\BusinessLogic.sln – contains most of the logic that you might want to debug and is thus the project you will most likely use to attach to the service during debugging.

You must load Visual Studio in ELEVATED mode for the service projects when debugging or running the unit tests. The client project can be loaded and tested non-elevated.

When testing the client, set TestMode=0 in the TestMode.xml in c:\ProgramData\StackHash. When you want to run unit tests ensure that you set TestMode=1 and restart the service. This is important as running the unit tests with TestMode=0 will delete existing profiles. Setting TestMode=1 creates the profile settings file in a subfolder of the c:\ProgramData\StackHash.

The business login project contains a number of UnitTest projects that can be run independently from within Visual Studio.

The service contracts project contains just the one UnitTest project. If you build the project from within VS, the service will be started automatically. If the tests fail, ensure that the service is running using the Service Control Manager.

To attach a debugger to the service during startup:

# Open up the business logic project in VS (elevated). Set appropriate breakpoints.
# Right click the service in the Service Control Manager.
# Enter 30000 in the “start parameters” box. This will force the service to pause for 30 seconds during startup allowing you plenty of time to attach a debugger.
# Click Start.
# Attach the VS instance to the StackHashService service.

## Test Failures

If you get test failures:

# Check that all of the required installed components are present and correct: SqlServer with a StackHash instance; LiveId client; Debugging Tools for Windows; Visual Studio 2010 + updates; all OS updates.
# Check you have testmode.xml in c:\programdata\stackhash and that its contents are correct.
# Check that you have <TestMode>1</TestMode> set in testmode.xml.
# Some tests may fail because of timing if run on a slow machine or VM.
# Some tests may fail if run across midnight boundary (local or UTC).
# Test failures may leave an inaccessible database. Before a test run use Sql Management Studio to ensure that test databases are not present (e.g. TestIndex). Delete them manually if they are. The tests attempt to do this automatically but some tests are know not to clear up properly.
# Check that the test machine is online and can access the internet. The StackHash service should be permitted to access the internet in any firewall s/w.

