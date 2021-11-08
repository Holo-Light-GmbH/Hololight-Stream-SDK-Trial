# Clients

ISAR SDK currently supports the clients listed in the sections below.

Before running the client, configure [The ISAR Configuration File](../README.md#the-isar-configuration-file) for the specific client. The below table contains the default suggested configuration for each client.

| Client | Width | Height | NumViews | Bandwidth | Version |
--- | --- | --- | --- | --- | --- |
HoloLens 2 | 1440 | 936 | 2 | 35000 | Stable
Oculus Quest 2 | 1920 | 1920 | 2 | 50000 | Stable
Android | 1920 | 1080 | 1 | 35000 | BETA Version


## HoloLens
### Supported Versions
- [HoloLens 2](https://www.microsoft.com/en-us/hololens/buy)
### Install & Connect ISAR Client
- Install the ISAR Client from either the Microsoft store on the device or from the app package contained in this repository
- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the HoloLens should begin to display the scene

### Known Issues
There are no known issues with this client.

## Oculus
### Supported Versions
- [Oculus Quest 2](https://www.oculus.com/quest-2/)
### Install & Connect ISAR Client
- Install the ISAR Client from either the Oculus Store or from the app package contained in this repository
- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the Oculus should begin to display the scene

For information regarding using the Oculus in Unity, see [Oculus](./oculusQuest.md).

### Known Issues
There are no known issues with this client.

## Android
### Supported Versions
- Minimum Android 8.0
### Install & Connect ISAR Client
- Install the ISAR Client from either the Google Play Store or from the app package contained in this repository
- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the Android device should begin to display the scene

### Known Issues
- Unity HDRP Project: Touch input not working
- Unity HDRP Project: Rendering has dark grey artifacts close to light sources

