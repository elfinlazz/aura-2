@echo off
rem Aura
rem Server start up script
rem -------------------------------------------------------------------------
rem Tries to start the server passed as first argument (LoginServer,
rem WorldServer, MsgrServer), from bin\ or its sub-folders
rem (Release and Debug). If no argument is passed it calls all start bats,
rem to start all servers.
rem -------------------------------------------------------------------------

if "%1" == "" goto NO_ARGS

set FILENAME=%1

rem Check for a build in bin\ first
if not exist bin\%FILENAME%.exe goto SUB_RELEASE
set PATH=bin\
goto RUN

rem Huh, maybe there's a build in bin\Release?
:SUB_RELEASE
IF NOT EXIST bin\Release\%FILENAME%.exe GOTO SUB_DEBUG
set PATH=bin\Release\
goto RUN

rem Mah... come here debug!
:SUB_DEBUG
IF NOT EXIST bin\Debug\%FILENAME%.exe GOTO ERROR
set PATH=bin\Debug\

rem Go, go, go!
:RUN
echo Running %FILENAME% from %PATH%
%windir%\system32\ping -n 2 127.0.0.1 > nul
cls
cd %PATH%
%FILENAME%.exe
exit

rem Now I'm a saaad panda qq
:ERROR
echo Couldn't find %FILENAME%.exe
pause

exit

:NO_ARGS
start start-login
start start-channel
start start-web
