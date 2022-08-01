# ISAR Extension for MRTK

This Unity package features access to Holo-Light's remote rendering library ISAR. ISAR is easily integrated into augmented reality (AR) projects by Unity's Package Manager UI. ISAR is used to create server-side applications for augmented and mixed reality to which you can connect from AR devices using Holo-Hight's ISAR client application. This package is an extension of ISAR Core to include MRTK support. The package contains two tutorial projects **HelloMRTK** demonstrating the use of the ISAR library:

## HelloMRTK (Mixed Reality Toolkit)
Sample project showcasing a simple cube with basic HoloLens1 interaction. This project offers the same functionality as the HelloIsar sample but it requires Microsoft's Mixed Reality Toolkit (MRTK v.2.2.0) to use HoloLens input interactions.

## Notes
- The samples support Unity 2018.4.x . Importing a sample by double clicking on the particular scene file is not supported by Unity 2018. If the sample does not start just copy the particular scene file manually into Unity's Assets folder and start it from there.

- Installing the ISAR package will create a **StreamingAssets** folder in your project containing the config file **remoting-config.cfg** which is needed to provide some basic settings for ISAR remoting library.

Copyright &copy; 2015 - 2021 Holo-Light GmbH
