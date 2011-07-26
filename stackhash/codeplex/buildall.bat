ECHO OFF
REM StackHash build batch file

ECHO Building STACKHASH

REM Must be at least one parameter.
IF "%1"=="" GOTO synopsis

REM *************************************
REM **          SET DEFAULTS           **
REM *************************************
SET Configuration=None
SET Test=false
SET TestConfig=Debug
SET GetLatestSource=false
SET RebuildAll=false
SET Platform=
SET UpdateVersions=false
SET IncrementVersion=false
SET BuildInstaller=false
SET LabelSource=false
SET CopySymbols=false
SET IndexSource=false

REM *************************************
REM **          CHECK PARAMETERS       **
REM *************************************

FOR %%A IN (%*) DO (call :CheckParam %%A)


IF NOT "%Configuration%" == "None" SET TestConfig=%Configuration%


REM *************************************
REM **    OUTPUT SELECTED PARAMETERS   **
REM *************************************
IF NOT "%Configuration%" == "Debug" GOTO out1
ECHO Configuration=Debug

:out1
IF NOT "%Configuration%" == "Release" GOTO out2
ECHO Configuration=Release

:out2
IF NOT "%GetLatestSource%" == "true" GOTO out3
ECHO GetLatestSource=TRUE

:out3
IF NOT "%GetLatestSource%" == "false" GOTO out4
ECHO GetLatestSource=FALSE

:out4
IF NOT "%Test%" == "true" GOTO out5
ECHO Test=TRUE

:out5
IF NOT "%Test%" == "false" GOTO out6
ECHO Test=FALSE

:out6
IF NOT "%RebuildAll%" == "true" GOTO out7
ECHO RebuildAll=TRUE

:out7
IF NOT "%RebuildAll%" == "false" GOTO out8
ECHO RebuildAll=FALSE

:out8
IF NOT "%UpdateVersions%" == "false" GOTO out9
ECHO UpdateVersions=FALSE

:out9
IF NOT "%UpdateVersions%" == "true" GOTO out10
ECHO UpdateVersions=TRUE

:out10
IF NOT "%BuildInstaller%" == "false" GOTO out11
ECHO BuildInstaller=FALSE

:out11
IF NOT "%BuildInstaller%" == "true" GOTO out12
ECHO BuildInstaller=TRUE

:out12
IF NOT "%IncrementVersion%" == "false" GOTO out13
ECHO IncrementVersion=FALSE

:out13
IF NOT "%IncrementVersion%" == "true" GOTO out14
ECHO IncrementVersion=TRUE

:out14
IF NOT "%LabelSource%" == "false" GOTO out15
ECHO LabelSource=FALSE

:out15
IF NOT "%LabelSource%" == "true" GOTO out16
ECHO LabelSource=TRUE

:out16
IF NOT "%CopyRelease%" == "false" GOTO out17
ECHO CopyRelease=FALSE

:out17
IF NOT "%CopyRelease%" == "true" GOTO out18
ECHO CopyRelease=TRUE

:out18

IF NOT "%IndexSource%" == "true" GOTO out19
ECHO IndexSource=TRUE

:out19

IF NOT "%IndexSource%" == "false" GOTO out20
ECHO IndexSource=FALSE

:out20



REM *************************************
REM **  INVOKE THE BUILD PROJECT       **
REM *************************************

IF "%VcvarsCalledAlready%"=="true" GOTO jumpvcvars
CALL r:\vcvars10
:jumpvcvars
SET VcvarsCalledAlready=true

REM Make sure the stackhash service is stopped.
net stop "StackHashService"
msbuild buildall.proj /p:TrackFileAccess=false

GOTO end


:synopsis
ECHO ****************************************************************
ECHO **                STACKHASH build batch file.                 **
ECHO **                                                            **
ECHO ** Usage: buildall [-switch1] [-switch2]...                   **
ECHO **                                                            **
ECHO ** where:                                                     **
ECHO **     -r or -R   = Release (default is debug).               **
ECHO **     -d or -D   = Debug (default is debug).                 **
ECHO **     -t or -T   = Runs all tests (default is not).          **
ECHO **     -p4 or -P4 = Gets the latest version from source ctrl  **
ECHO **                  (default is not to)                       **
ECHO **     -a or -A   = Rebuild (default just builds out of date) **
ECHO **     -u or -U   = Increment the Cucku version number        **
ECHO **     -v or -V   = Update AssemblyInfo file versions         **
ECHO **     -i or -I   = Include the installer in the build        **
ECHO **     -l or -L   = Label the source code                     **
ECHO ****************************************************************

GOTO end



:CheckParam

SET Param=%1

ECHO %Param%

IF NOT "%Param%" =="-d" GOTO L1
SET Configuration=Debug
GOTO success

:L1
IF NOT "%Param%" == "-D" GOTO L2
SET Configuration=Debug
GOTO success

:L2
IF NOT "%Param%" == "-r" GOTO L3
SET Configuration=Release
GOTO success

:L3
IF NOT "%Param%" == "-R" GOTO L4
SET Configuration=Release
GOTO success

:L4
IF NOT "%Param%" == "-t" GOTO L5
SET Test=true
GOTO success

:L5
IF NOT "%Param%" == "-T" GOTO L6
SET Test=true
GOTO success

:L6
IF NOT "%Param%" == "-p4" GOTO L7
SET GetLatestSource=true
GOTO success

:L7
IF NOT "%Param%" == "-P4" GOTO L8
SET GetLatestSource=true
GOTO success

:L8
IF NOT "%Param%" == "-a" GOTO L9
SET RebuildAll=true
GOTO success

:L9
IF NOT "%Param%" == "-A" GOTO L10
SET RebuildAll=true
GOTO success

:L10
IF NOT "%Param%" == "-v" GOTO L11
SET UpdateVersions=true
GOTO success

:L11
IF NOT "%Param%" == "-V" GOTO L12
SET UpdateVersions=true
GOTO success

:L12
IF NOT "%Param%" == "-i" GOTO L13
SET BuildInstaller=true
GOTO success

:L13
IF NOT "%Param%" == "-I" GOTO L14
SET BuildInstaller=true
GOTO success

:L14
IF NOT "%Param%" == "-u" GOTO L15
SET IncrementVersion=true
GOTO success

:L15
IF NOT "%Param%" == "-U" GOTO L16
SET IncrementVersion=true
GOTO success

:L16
IF NOT "%Param%" == "-l" GOTO L17
SET LabelSource=true
GOTO success

:L17
IF NOT "%Param%" == "-L" GOTO L24
SET LabelSource=true
GOTO success

:L24
ECHO ********ERROR - unrecognised parameter.
ECHO "%Param%"
GOTO synopsis


:success
GOTO:EOF

:end
