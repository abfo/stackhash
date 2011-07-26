@echo off

rem Debug Script Executed Test

echo *** Debug Script Executed *** >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"

echo Report Type: %~1 >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
shift

echo Product Id: %~1 >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
shift

echo Product Name: %~1 >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
shift

echo Product Version: %~1 >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
shift

echo Event ID: %~1 >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
shift

echo Event Type Name: %~1 >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
shift

echo Plugin Bug Reference: %~1 >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
shift

echo Cab Id: %~1 >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
shift

echo Script Name: %~1 >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
shift

echo Script Run Date: %~1 >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
shift

echo Script Results: >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
type %1 >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"


echo. >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"
echo. >> "%ProgramData%.\StackHash_TestDebugScriptExecuted.log"


