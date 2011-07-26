@echo off

rem Product Added Test

echo *** Product Added *** >> "%ProgramData%.\StackHash_TestProductAdded.log"

echo Report Type: %~1 >> "%ProgramData%.\StackHash_TestProductAdded.log"
shift

echo Product Id: %~1 >> "%ProgramData%.\StackHash_TestProductAdded.log"
shift

echo Product Name: %~1 >> "%ProgramData%.\StackHash_TestProductAdded.log"
shift

echo Product Version: %~1 >> "%ProgramData%.\StackHash_TestProductAdded.log"
echo. >> "%ProgramData%.\StackHash_TestProductAdded.log"
echo. >> "%ProgramData%.\StackHash_TestProductAdded.log"