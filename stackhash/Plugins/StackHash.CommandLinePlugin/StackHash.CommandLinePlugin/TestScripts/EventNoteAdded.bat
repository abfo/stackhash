@echo off

rem Event Note Added Test

echo *** Event Note Added *** >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"

echo Report Type: %~1 >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
shift

echo Product Id: %~1 >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
shift

echo Product Name: %~1 >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
shift

echo Product Version: %~1 >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
shift

echo Event ID: %~1 >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
shift

echo Event Type Name: %~1 >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
shift

echo Plugin Bug Reference: %~1 >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
shift

echo Note Source: %~1 >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
shift

echo Note User: %~1 >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
shift

echo Note Entry Time: %~1 >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
shift

echo Note: >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
type %1 >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"


echo. >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"
echo. >> "%ProgramData%.\StackHash_TestEventNoteAdded.log"


