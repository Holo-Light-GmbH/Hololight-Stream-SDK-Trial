<div id="top"></div>

<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]



<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial">
    <img src="./Docs/images/ISAR_Icon.png" alt="Logo" width="80" height="80">
  </a>

<h2 align="center">ISAR SDK Trial</h2>

  <p align="center">
    Interactive Streaming for Augmented Reality (or just ISAR) is a plugin, allowing interactive (data/telemetry being transmitted both ways) streaming of ANY application to XR devices.
    <br />
    <a href="https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial/Docs"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://youtu.be/pddfBpwvFPI">View Demo</a>
    ·
    <a href="https://www.youtube.com/watch?v=-CKM4XonzI0">View Integration</a>
    ·
    <a href="https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial/issues">Report Bugs</a>
    ·
    <a href="https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial/issues">Request Features</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

[![Product Name Screen Shot][product-screenshot]](https://holo-light.com/products/isar-sdk/)

The following documentation specifically references usage with the Unity Game Engine, however the ISAR SDK can be used independently of Unity. For more support regarding usage outside of Unity, see the Support section of our website. 

<p align="right">(<a href="#top">back to top</a>)</p>

### Built With

* [WebRTC](https://webrtc.org/)
* [C/C++](https://docs.microsoft.com/en-us/cpp/?view=msvc-170)

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- TRIAL NOTICE -->
## Trial Notice

Since this is a trial of the ISAR SDK, it comes with an expiration date. We always update the package/repo prior to the date running out. Your application will no longer stream once the trial expires! So keep an eye out for udpates.

<center>⚠️<strong> This trial will expire/renew March 1st, 2022 </strong> ⚠️</center>

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

Now with that out of the way, let's get you started on rolling with ISAR SDK. We recommend you check out the [quick integration tutorial video](https://www.youtube.com/watch?v=-CKM4XonzI0) as well as an [overview of the architecture](https://youtu.be/pddfBpwvFPI).

### Prerequisites

* Minimum Unity 2019.4.x (tested with 2019.4.21f1)
* Disable your Firewall (or check below for specific ports to open for the stream)

### First Time Installation

1. Open an existing Unity 3D project or create a new one
2. Remove previous versions of ISAR from the project (skip this if the project does not currently have ISAR installed)
    - Open **Package Manager** in the Unity editor (`Window -> Package Manager`)
    - Find the **ISAR Core** package and choose **Remove**
    - Find the **ISAR MRTK Extensions** package, if installed, and choose **Remove**
3. Import **ISAR Core** into the project
    - Open **Package Manager** in the Unity editor (`Window -> Package Manager`)
    - If installed, remove the **Version Control** package from the project
    - Click the **+** then **Add package from disk...** and select the file `package.json` from the `com.hololight.isar` directory contained in this repository
4. Add the desired toolset following the instructions below
    - [MRTK](./Docs/mrtkextension.md); supports HoloLens 2, Oculus Quest 2 and Android clients
    - [XR Interaction Toolkit](./Docs/xrInteractionToolkit.md); supports Oculus Quest 2 client

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- Project Config -->
## Project Configuration

1. Navigate to `Edit -> Project Settings -> XR Plug-in Management`
    - Enable **ISAR XR**
2. Open `File -> Build Settings...`
    - Ensure the following configuration is selected:
        - Platform: **PC, Mac & Linux Standalone** + Architecture: **x86_64**

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- Project File -->
## ISAR Configuration File

The configuration file for ISAR is located within the ISAR Core package in `Runtime/Resources`. A copy of this file is placed in the `Assets/StreamingAssets` directory of the Unity project when the server is run. Changes to the configuration file in `StreamingAssets` will be overwritten each run and each build; therefore it is recommended not to make changes to the copied file.

Below is a description of each section within the configuration file that can be changed by the developer. The suggested default values for each client can be found [here](./Docs/clients.md).

- `width` and `height` determine the resolution of the rendered image per view/eye. Careful, the resolution needs to be supported by the H264 hardware decoder of the client device
- `numViews` determines the number of views/eyes rendered on the client device. Valid values are:
    - `1` for mono rendering, e.g. Tablets
    - `2` for stereo rendering, e.g. HoloLens
- `bandwidth` allows the developer to specify the bitrate for the encoder. If no value is set, a default value of 20mbit is used.

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- First Run -->
## First Time Run

Enter play mode in the Unity editor or build a standalone application with the following settings:

- Platform: **PC, Mac & Linux Standalone**
- Target Platform: **Windows**
- Architecture: **x86_64**

The ISAR server application will start listening on the TCP port 9999. The ISAR client application is using this port to establish the streaming session. If the connection fails, ensure that no firewall is blocking it.

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- Disable ISAR -->
## Disabling ISAR

The ISAR SDK can be disabled while remaining as a package in the project. To do so, follow the below steps:

- Click `Edit->Project Settings` and move to the **XR Plug-in Management** section
- Uncheck **ISAR XR**
- Check **Unity Mock HMD**
- Ensure not to call the `Isar` class, or any of the inherited classes (`IsarViewPose`, `IsarAudio`, `IsarCustomSend` or `IsarQr`), constructor within the code
- Disable the `QrSupport` script if it is enabled

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- Clients -->
## Clients

ISAR SDK supports several clients that can connect to the server. For information on installing and connecting with specific clients, refer to [Clients](./Docs/clients.md).

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- Examples live here -->
## Example

A preconfigured example is available within the repository. For information on how to load this example, refer to [Example](./Docs/example.md).

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- Add. Features -->
## Additional Features

<!-- QR Codes -->
### QR Code

To find out how to easily integrate QR code support, see [here](./Docs/qrcode.md).

<!-- Image Tracking -->
### Image Tracking

To find out how to enable and use image tracking, see [here](./Docs/imagetracking.md).

_For more examples, please refer to the [Docs](./Docs/)._

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- ROADMAP -->
## Roadmap

- [ ] Depth (Stabilization)
- [ ] Spatial Understanding
- [ ] ISAR 3.0
    - [ ] Improved Audio
    - [ ] Improved Performance
- [ ] iOS Support

See the [open issues](https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- LICENSE -->
## License

Distributed under Holo-Light GmbH's Proprietary License. See `License.txt` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Holo-Light GmbH - [@Holo_LightGmbH](https://twitter.com/Holo_LightGmbH) - support@holo-light.com

Project Link: [https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial](https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial)

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
<!-- ## Acknowledgments

* []()
* []()
* []()

<p align="right">(<a href="#top">back to top</a>)</p> -->



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/Holo-Light-GmbH/ISAR-SDK-Trial.svg?style=for-the-badge
[contributors-url]: https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Holo-Light-GmbH/ISAR-SDK-Trial.svg?style=for-the-badge
[forks-url]: https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial/network/members
[stars-shield]: https://img.shields.io/github/stars/Holo-Light-GmbH/ISAR-SDK-Trial.svg?style=for-the-badge
[stars-url]: https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial/stargazers
[issues-shield]: https://img.shields.io/github/issues/Holo-Light-GmbH/ISAR-SDK-Trial.svg?style=for-the-badge
[issues-url]: https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial/issues
[license-shield]: https://img.shields.io/github/license/Holo-Light-GmbH/ISAR-SDK-Trial.svg?style=for-the-badge
[license-url]: https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial/blob/main/Licenses/License.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://www.linkedin.com/company/hololightgmbh
[product-screenshot]: https://github.com/Holo-Light-GmbH/ISAR-SDK-Trial/blob/main/Docs/images/ISAR_Architecture.png


