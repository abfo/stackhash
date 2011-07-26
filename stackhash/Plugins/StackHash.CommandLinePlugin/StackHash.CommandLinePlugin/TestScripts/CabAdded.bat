@echo off

rem Cab Added Test

echo *** Cab Added *** >> "%ProgramData%.\StackHash_TestCabAdded.log"

echo Report Type: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Product Id: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Product Name: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Product Version: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Event ID: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Event Type Name: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Plugin Bug Reference: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Cab Id: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Cab Size (Bytes): %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Cab Downloaded: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Cab Purged: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo .NET Version: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Machine Architecture: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo OS Version: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo Process Uptime: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"
shift

echo System Uptime: %~1 >> "%ProgramData%.\StackHash_TestCabAdded.log"

echo. >> "%ProgramData%.\StackHash_TestCabAdded.log"
echo. >> "%ProgramData%.\StackHash_TestCabAdded.log"


