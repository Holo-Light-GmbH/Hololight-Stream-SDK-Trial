@ECHO OFF
ECHO.
setlocal enabledelayedexpansion

SET isarINFName="isarvirtualmicrophone.inf"
SET currPublishedName=""
SET isarPublishedName=""

FOR /F "skip=2 tokens=1 delims=" %%e IN ('%windir%\system32\pnputil /enum-drivers') DO (
    FOR /F "tokens=1,2 delims=:" %%u IN ("%%e") DO (
        IF "%%u"=="Published Name" (set "currPublishedName=%%v")
        IF "%%u"=="Original Name" FOR /F "tokens=1,2 delims= " %%k IN ("%%v") DO (
            IF "%%k"==!isarINFName! set "isarPublishedName=!currPublishedName!"
        )
    )
)

IF !isarPublishedName!=="" (
    ECHO ISARVirtualMicrophone driver installation files are not found on system.
) else (
    ECHO Uninstalling ISARVirtualMicrophone driver installation files......
    FOR /F "tokens=1,2 delims= " %%z IN ("!isarPublishedName!") DO (
        %windir%\system32\pnputil /delete-driver %%z
    )
)
