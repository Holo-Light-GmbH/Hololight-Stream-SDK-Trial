# ISAR Virtual Microphone Driver Project

This repository contains Visual Studio project which generates a KMDF driver package associated with creating virtual audio devices on Windows Vista or newer OS. The functionality is similar to  [VB-Audio](https://vb-audio.com/) virtual cable device where the driver creates two audio devices, one for rendering audio (speakers) and one for capturing audio (microphone). The driver works as follows, audio streams that are rendered to the rendering end get automatically routed to capturing end.

The driver code has been based off the following implementations:
- [Microsoft Driver Samples](https://github.com/microsoft/Windows-driver-samples/tree/master/audio/simpleaudiosample)
- [Audio Mirror](https://github.com/JannesP/AudioMirror)

More information about the implementation can be found [here](https://holo.atlassian.net/wiki/spaces/HLSD/pages/1953103873/ISAR+Virtual+Microphone+Device+Drivers+on+Windows). 

## Prerequisites
- Install [Windows Driver Kit (WDK) for Windows 10, Version 2004](https://docs.microsoft.com/en-us/windows-hardware/drivers/other-wdk-downloads)
- Edit the Visual Studio 2019 tools to include the ones listed in the WDK installation page

## Building the Driver

- Open the `ISAR Virtual Microphone.sln` solution
- Click `Build->Build Solution` to generate the driver files
- The following files will be generated in the associated platform and runtime folder, `x64/Release`:

| File | Description |
| --- | --- |
| ISARVirtualMicrophone.sys | The driver file |
| ISARVirtualMicrophone.cat | A signed catalog file, which serves as the signature for the entire package |
| ISARVirtualMicrophone.inf | A non-componentized information (INF) file that contains information required to install the driver |


## Installing the Driver

- The files mentioned above will be automatically copied into the `Installer` directory alongside `install.bat`. If these have not been copied correctly, this can also be done manually.
- Run the `Installer/install.bat` script

An uninstaller, `Installer/uninstall.bat`, is also provided for uninstalling the driver from the system.

## Signing the Driver

Currently the driver is not signed as part of build process. This must be done manually by following the steps below:

- Enable Windows [Test Mode](https://docs.microsoft.com/en-us/windows-hardware/drivers/develop/preparing-a-computer-for-manual-driver-deployment).
- Generate the certificate:
    - Right click on the Visual Studio project and set `Properties->Driver Signing->Sign Mode` to `Test Sign`
    - Right click on the Visual Studio project and set `Properties->Driver Signing->Test Certificate` to `\<Create Test Certificate\>`
- Move to the build folder `Release\x64\` and locate the test certificate
- Install this certificate to the Windows trusted certificates by double clicking on the certificate
- Follow the Windows installation steps
