@ECHO OFF
ECHO.

:: BatchGotAdmin
:-------------------------------------
REM  --> Check for permissions
    IF "%PROCESSOR_ARCHITECTURE%" EQU "amd64" (
>nul 2>&1 "%SYSTEMROOT%\SysWOW64\cacls.exe" "%SYSTEMROOT%\SysWOW64\config\system"
) ELSE (
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"
)

REM --> If error flag set, we do not have admin.
if '%errorlevel%' NEQ '0' (
    echo Requesting administrative privileges for uninstalling ISARVirtualMicrophone...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    set params= %*
    echo UAC.ShellExecute "cmd.exe", "/c ""%~s0"" %params:"=""%", "", "runas", 1 >> "%temp%\getadmin.vbs"

    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    pushd "%CD%"
    CD /D "%~dp0"
:--------------------------------------    


SET currPath=%~dp0
IF NOT "%OS%"=="Windows_NT" GOTO FAILMSG
 
SETLOCAL
SET SB=
FOR /F "tokens=2 delims=:()" %%A IN ('DEVCON find * ^| FIND /I "ISARVirtualMicrophone"') DO SET SB=%%A
IF DEFINED SB (
	ECHO Found ISARVirtualMicrophone device, uninstalling....
	DEVCON remove *ISARVirtualMicrophone*
    call purgeinf.bat
) ELSE (
    ECHO ISARVirtualMicrophone device not found on the system.
)
ENDLOCAL
GOTO PMessage


:FAILMSG
ECHO Uninstallation failed !
:End

:PMessage
set /p prompt=Press ENTER to continue...
:End


