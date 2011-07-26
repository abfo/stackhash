echo off
if "%1"=="" goto error
if "%1"=="debug" goto paramok
if "%1"=="Debug" goto paramok
if "%1"=="DEBUG" goto paramok
if "%1"=="release" goto paramok
if "%1"=="Release" goto paramok
if "%1"=="RELEASE" goto paramok
goto error


:paramok

if exist r:\stackhash\results goto checksubfolder
mkdir r:\stackhash\results
:checksubfolder
if exist r:\stackhash\results\%1 goto starttest
mkdir r:\stackhash\results\%1

:starttest

set component=stackhashbugtrackerinterfaceunittests

if not exist r:\stackhash\results\%1\%component%.xml goto runtest
del r:\stackhash\results\%1\%component%.xml


:runtest
mstest /testcontainer:"r:\stackhash\plugins\StackHashBugTrackerPlugInV1\%component%\bin\%1\%component%.dll" /resultsfile:"r:\stackhash\results\%1\%component%.xml"


goto end

:error
ECHO You must specify debug or release as a parameter.

:end

