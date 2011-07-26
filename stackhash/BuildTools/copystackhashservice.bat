IF EXIST "r:\stackhash\servicecontracts\serviceinstall" GOTO folderexists
mkdir "r:\stackhash\servicecontracts\serviceinstall"
IF EXIST "r:\stackhash\servicecontracts\serviceinstall" GOTO folderexists
ECHO ***ERROR Failed to create folder r:\stackhash\servicecontracts\serviceinstall
GOTO end


:folderexists

if EXIST "r:\stackhash\servicecontracts\serviceinstall\psscor2" GOTO createdir1
mkdir "r:\stackhash\servicecontracts\serviceinstall\psscor2"

:createdir1

if EXIST "r:\stackhash\servicecontracts\serviceinstall\psscor4" GOTO createdir2
mkdir "r:\stackhash\servicecontracts\serviceinstall\psscor4"
:createdir2

robocopy /E r:\3rdparty\psscor2 "r:\stackhash\servicecontracts\serviceinstall\psscor2"
robocopy /E r:\3rdparty\psscor4 "r:\stackhash\servicecontracts\serviceinstall\psscor4"




if "%1"=="Debug" goto debug
if "%1"=="debug" goto debug
if "%1"=="-d" goto debug
if "%1"=="-D" goto debug

if "%1"=="Release" goto release
if "%1"=="release" goto release
if "%1"=="-r" goto release
if "%1"=="-R" goto release

echo *** ERROR: Must specify either Debug or Release Unknown.
goto end


:debug
ECHO copying debug service files.
copy r:\stackhash\servicecontracts\debug\stackhash* r:\stackhash\servicecontracts\serviceinstall\*
copy r:\stackhash\servicecontracts\debug\*.dll r:\stackhash\servicecontracts\serviceinstall\*
goto end

:release
ECHO copying release service files.
copy r:\stackhash\servicecontracts\release\stackhash* r:\stackhash\servicecontracts\serviceinstall\*
copy r:\stackhash\servicecontracts\release\*.dll r:\stackhash\servicecontracts\serviceinstall\*
goto end


:end



