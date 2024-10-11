# Clients

Hololight Stream currently supports the clients listed in the sections below.

## HoloLens

### Supported Versions

- [HoloLens 2](https://www.microsoft.com/en-us/hololens/buy)

### Install & Connect

- Install the Hololight Stream Client from either the Microsoft store on the device or from the app package contained in this repository
- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the HoloLens should begin to render the scene

### Known Issues

- There can be undefined behavior when connecting with a connection timeout less than 4 seconds.

## Meta

### Supported Versions

- [Quest 2](https://www.meta.com/us/en/quest/products/quest-2/)
- [Quest Pro](https://www.meta.com/us/quest/quest-pro/)
- [Quest 3](https://www.meta.com/us/en/quest/quest-3/)

### Install & Connect

- Install the Hololight Stream Client from either the AppLab store or from the app package contained in this repository
- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the device should begin to render the scene

### Known Issues

There are no known issues with this client.

## iOS/iPadOS

### Supported Versions

- Minimum 15

### Install & Connect

- Install the Hololight Stream Client from either the Apple Store or from the app package contained in this repository
- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the device should begin to render the scene

### Known Issues

- Unity HDRP Project: Touch input is not working.
- Unity HDRP Project: Rendering has dark grey artifacts close to light sources.

## Magic Leap

### Supported Versions

- [Magic Leap 2](https://www.magicleap.com/magic-leap-2)

### Install & Connect

- Follow the instructions mentioned [here](https://www.magicleap.care/hc/en-us/articles/5951496255885-Installing-and-Uninstalling-Apps) to install the apk
- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the device should begin to render the scene

### Known Issues

There are no known issues with this client.

## Lenovo

### Supported Versions

- [ThinkReality VRX](https://www.lenovo.com/us/en/thinkrealityvrx)

### Install & Connect

- Follow the [VRX Developer Guide](https://thinkreality.uds-dev.lenovo.com/developer-vrx/) to install the apk included in the Clients directory

  - Follow the [Developer Mode](https://thinkreality.uds-dev.lenovo.com/developer-vrx/developer-mode.html) and [Android Debug Bridge](https://thinkreality.uds-dev.lenovo.com/developer-vrx/adb.html) sections for sideloading apps

- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the device should begin to render the scene

### Known Issues

- The client only supports controller interactions. Hand interactions are not supported.

## Desktop

### Supported Versions

- Windows 11 64-bit

### Install & Connect

- Use the installer included in the Clients directory to install the Desktop Client
- Run the installed application
- On the home screen, insert the servers IP address in the client application and press "Connect"
- Once connected successfully, the application should begin to render the scene

### Known Issues and Limitations

- Desktop client is limited to Full HD (1920x1080) resoultion.
- There can be undefined behavior when connecting with a connection timeout less than 4 seconds.
