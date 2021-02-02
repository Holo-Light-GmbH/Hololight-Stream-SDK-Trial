# ISAR SDK

<p align="center">
	<img src="Docs/imgs/ISAR_Icon.png" width="180px">
</p>

# What is ISAR SDK? 

<em>ISAR SDK – Crafted for Developers, by Developers</em><br>
We’ve taken our approach to Interactive Streaming a step further by developing ISAR SDK. Allowing you to stay in control, crafting your own bespoke remote rendering AR solution. We’ve diligently focused on simplicity of integration and stability. Whether in managed environments, hosted clouds or on-premises solutions.

# Getting Started

> :warning: We don't recommend changing any source files delivered with Isar. If you do so, we can't guarantee support.

## Prerequisites

- Minimum Unity 2019.4.x (tested with 2019.4.18f1)
- Holo-Light ISAR SDK Package → com.hololight.isar
- HoloLens Client Application → ISAR_Client_x.x.x.x
- Mixed Reality Toolkit (tested with MRTK 2.5.3)

## Installation Guide of ISAR SDK

0. Create a new Unity 3D Project (skip this, if you already have a Project)

1. Import **MRTK** into your Project 
	- <a target="_blank" href="https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Installation.html#1-get-the-latest-mrtk-unity-packages">Getting Started</a>
2. Import **ISAR SDK**
	- Make sure you are connected to the Internet
	- Open **Package Manager** in Unity Editor (`Window → Package Manager`)
	- Choose **Add package from disk**. Select the file `package.json` from `com.hololight.isar`. 
3. Continue with **Project Configration**

## Project Configuration

1. Navigate to `Edit → Project Settings → XR Plug-in Management`
	- Tick on **ISAR XR** for both Platforms
2. Open `File → Build Settings`
	- Supported Platforms: **PC, Mac & Linux Standalone** or **Universal Windows Platform (UWP)**
	- Architecture: **x86_64** or **x64 (UWP)**
3. Continue with **Scene Configuration**

## Scene Configuration
### 1-Click Scene Config
- Choose `ISAR → Configure MRTK` to configure your Scene to work with ISAR. It will add the neccessary objects to your Scene and configure them properly. 
- Once this is done, you have sucessfully configured ISAR and can go on with **First Run**

### Manual Config
If you need the manual way (custom MRTK Configurations), then do the following steps.

- Add **MixedRealityToolkit** Object to the Scene by selecting the menu option `Mixed Reality Toolkit->Add to Scene and Configure...` in top menu bar of Unity.
- Change **Camera** profile from Default to **IsarXRSDKCameraProfile**
- Change **Input** Profile to **IsarXRSDKInputSystemProfile** 

### Checklist

If...

- ☑️ ...MixedRealityToolkit -> Input -> **Input Data Providers** contains **ISAR XRSDK Device Manager**

- ☑️ ...MixedRealityToolkit -> Camera -> **Camera Settigs Providers** contains **XR SDK Camera Settings**

- ☑️ ...you followed the steps from Project Configuration correctly

- ☑️ ...then you have set up everything correctly! 👍

## First Run

Enter play mode in Unity Editor or build a Standalone Application with the following settings:

- Platform: PC, Mac & Linux Standalone
- Target Platform: Windows
- Architecture: x86_64

The ISAR Server Application will start listening on TCP port 9999 of your computer. The HoloLens ISAR Client Application can then use that Port to establish the Streaming Session. 


## HelloIsar Example

You can check out the preconfigured Project **HelloIsar**. Navigate to the Folder HelloIsar and open it with Unity. When opening the project for the first time, you may have a issue with wrong package reference. Click on "Continue" and let the project load. Then follow the Step **2. Import ISAR SDK** from Installation Guide of ISAR SDK. 

# Install and run HoloLens Client Application

- Install the ISAR_Client app-package on HoloLens (e.g. via Device Portal) and start the Application.
- Insert the servers IP address in the client application and press on "Connect".
- You can as well let the Client App search for local Servers and press on the found server and press "Connect".
- Once the client connects successfully, you should see the Objects from your Scene. 

**Congratulations! 🙌🙌🙌**

## Need QR Code Support?

- <a href="Docs/qrcode.md">Check out how to easily integrate QR Code Support</a>

## Having Troubles? All Good:

- If the client fails to connect, check if the correct IP is written (e.g. 192.168.0.122 - WITHOUT the Port Number)
- If the client fails to connect, click on `Servers -> List servers` and see if it finds the server, then you can connect by clicking on the item -> `Connect`
- Check if the Firewall is blocking the connection (you can deactivate it for a short time to check that)
- When using Universal Render Pipeline (URP) please check the property **Post Processing**, which you can find **Main Camera->Rendering->Post Processing** 
- When your build target is UWP, please remember to check the following capabilities (InternetClient, InternetClientServer & PrivateNetworkClientServer)

## Issues with HelloIsar Example?

- When opening the project for the first time, you might get missing reference to **com.hololight.isar**. 
- Thats fine, just press **Continue** and wait until the project gets opened.
- Once the project loaded click on top `Window->Package Manager` to open the Package Manager. 
	- Click on the **+** Icon and add package from disk. 
	- Now select the **package.json** file inside the **com.hololight.isar** folder. 
- Now the SDK should get correctly loaded. 
