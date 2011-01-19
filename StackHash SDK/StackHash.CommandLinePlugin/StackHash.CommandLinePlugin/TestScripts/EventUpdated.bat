@echo off

rem Event Updated Test

echo *** Event Updated *** >> "%ProgramData%.\StackHash_TestEventUpdated.log"

echo Report Type: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Product Id: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Product Name: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Product Version: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Event ID: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Event Type Name: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Bug Reference: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Plugin Bug Reference: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Total Hits: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Application Name: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Application Version: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Application Time Stamp: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Module Name: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Module Version: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Module Time Stamp: %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Exception Code:  %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
shift

echo Offset:  %~1 >> "%ProgramData%.\StackHash_TestEventUpdated.log"
echo. >> "%ProgramData%.\StackHash_TestEventUpdated.log"
echo. >> "%ProgramData%.\StackHash_TestEventUpdated.log"

rem Return a >0 exit code to set the plugin bug reference
exit 4321
