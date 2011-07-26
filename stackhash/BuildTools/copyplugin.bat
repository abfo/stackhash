REM Copies the TestPlugin to C:\programdata\stackhash\BugTrackerPlugins

IF EXIST "C:\programdata\stackhash\test\BugTrackerPlugins" GOTO folderexists
mkdir "C:\programdata\stackhash\test\BugTrackerPlugins"

:folderexists

xcopy /Y R:\stackhash\BusinessLogic\BusinessLogic\StackHashTestBugTrackerPlugin\obj\%1\StackHashTestBugTrackerPlugIn.dll C:\programdata\stackhash\test\BugTrackerPlugins


