@echo off

rem Cab Updated Test

echo *** Cab Updated *** >> "%ProgramData%.\StackHash_TestCabUpdated.log"

echo Report Type: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Product Id: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Product Name: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Product Version: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Event ID: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Event Type Name: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Plugin Bug Reference: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Cab Id: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Cab Size (Bytes): %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Cab Downloaded: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Cab Purged: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo .NET Version: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Machine Architecture: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo OS Version: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo Process Uptime: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"
shift

echo System Uptime: %~1 >> "%ProgramData%.\StackHash_TestCabUpdated.log"

echo. >> "%ProgramData%.\StackHash_TestCabUpdated.log"
echo. >> "%ProgramData%.\StackHash_TestCabUpdated.log"


