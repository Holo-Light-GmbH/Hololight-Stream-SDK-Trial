# Clients

ISAR SDK currently supports the clients listed in the sections below.

Before running the client, configure the [ISAR Configuration File](README.md#isar-configuration-file) for the specific client. The below table contains the default suggested configuration for each client.

| Client    | Width | Height | NumViews | Bandwidth |
|---------- | ----- | ------ | -------- | --------- |
|HoloLens 2 | 1440  | 936    | 2        | 35000     |
|Quest 2    | 1920  | 1920   | 2        | 50000     |
|iOS        | 1920  | 1080   | 1        | 35000     |


## HoloLens
### Supported Versions
- [HoloLens 2](https://www.microsoft.com/en-us/hololens/buy)
### Install & Connect ISAR Client
- Install the ISAR Client from either the Microsoft store on the device or from the app package contained in this repository
- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the HoloLens should begin to render the scene

### Known Issues
There are no known issues with this client.

## Meta
### Supported Versions
- [Quest 2](https://www.meta.com/gb/en/quest/products/quest-2/)
### Install & Connect ISAR Client
- Install the ISAR Client from either the AppLab store or from the app package contained in this repository
- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the Quest 2 should begin to render the scene

### Known Issues
There are no known issues with this client.

## iOS
### Supported Versions
- Minimum 15
### Install & Connect ISAR Client
- Install the ISAR Client from either the Apple Store or from the app package contained in this repository
- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the iOS device should begin to render the scene

### Known Issues
- Unity HDRP Project: Touch input not working
- Unity HDRP Project: Rendering has dark grey artifacts close to light sources

