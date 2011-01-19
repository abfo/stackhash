@echo off

rem Cab Note Added Test

echo *** Cab Note Added *** >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"

echo Report Type: %~1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
shift

echo Product Id: %~1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
shift

echo Product Name: %~1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
shift

echo Product Version: %~1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
shift

echo Event ID: %~1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
shift

echo Event Type Name: %~1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
shift

echo Plugin Bug Reference: %~1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
shift

echo Cab Id: %~1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
shift

echo Note Source: %~1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
shift

echo Note User: %~1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
shift

echo Note Entry Time: %~1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
shift

echo Note: >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
type %1 >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"


echo. >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"
echo. >> "%ProgramData%.\StackHash_TestCabNoteAdded.log"


