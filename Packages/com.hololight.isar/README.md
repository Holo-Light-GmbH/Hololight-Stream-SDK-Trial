# ISAR - "com.hololight.isar"

This Unity package features access to Holo-Light's remote rendering library ISAR. ISAR is easily integrated into augmented reality (AR) projects by Unity's Package Manager UI. ISAR is used to create server-side applications for augmented and mixed reality to which you can connect from AR devices using Holo-Hight's ISAR client application. The package contains two tutorial projects **HelloIsar** and **HelloMRTK** demonstrating the use of the ISAR library:

## HelloIsar
Sample project showcasing a simple cube with basic Microsoft HoloLens1 interaction. When the user gazes at the cube it can be moved by drag gestures. Aside from that spheres can be created by tapping at the cube. In order to use this sample, no additional third party software is needed.

## Notes
- The samples support Unity 2018.4.x . Importing a sample by double clicking on the particular scene file is not supported by Unity 2018. If the sample does not start just copy the particular scene file manually into Unity's Assets folder and start it from there. 

- Installing the ISAR package will create a **StreamingAssets** folder in your project containing the config file **remoting-config.cfg** which is needed to provide some basic settings for ISAR remoting library.

Copyright &copy; 2015 - 2020 Holo-Light GmbH