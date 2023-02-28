# ISAR native docs

## Conceptual Overview

ISAR is based on WebRTC. That means establishing a connection needs to follow the signaling process, i.e. exchanging SDP and ICE candidates.
More info:
- https://www.webrtc-experiment.com/docs/WebRTC-Signaling-Concepts.html
- https://webrtc.org/getting-started/peer-connections#signaling

*Note:* the header files are compatible with **C11**, though internally we use **C++** to call the library functions. Both are expected to work.
On the graphics API side **only D3D11** is supported.

## Getting Started

Here's a list of high-level steps you need to get started:
- Set up your project, i.e. set include paths to the library headers and linker input to `remoting_unity.dll`
- Get the `HlrSvApi` struct so you can use the library
- Call `init()` and `init_video_track()`
- Register callbacks for signaling and rendering
- Establish a connection (signaling)
- Handle connection and rendering events
- Render and push frames

## Obtaining the API Struct and Initializing the Library

You need to call `hlr_sv_create_remoting_api()`. If everything goes well you should get back a filled out `HlrSvApi` struct.
This struct contains function pointers for all API functions. Here's how you create the API struct:
```cpp
HlrSvApi api = {};
HlrError err = hlr_sv_create_remoting_api(&api);
if (err == HlrError::eNone) {
	InitializeRemoting();
}
```

**Note:** Before including `remoting/server_api.h` (which you need to use the library), `#define WEBRTC_WIN` otherwise you might run into compile errors.

Before calling any other APIs though, you need to call `init()`. Its signature looks like this:
```cpp
HlrError init(const char* path,
			  HlrGraphicsApiConfig gfx_config,
			  HlrConnectionCallbacks callbacks,
			  HlrHandle* const connection_handle)
```

`path` is a path to a [config file](#config-file-sample), the `gfx_config` struct should get a pointer to your D3D11 device (only `d3d_device`, the other members are not necessary for `init` to work).  
`callbacks` contains callbacks required for signaling. Make sure to fill them out, otherwise you won't be able to establish a connection.  
`connection_handle` is an out parameter for a handle which, if valid, enables you to call any other API function.

**Note:** The `path` parameter is internally passed into the `std::ifstream` constructor, i.e. path resolution depends on its behavior.

With all of that in mind, you can call `init()` like this:
```cpp
void InitializeRemoting()
{
	HlrHandle handle = HLR_INVALID_HANDLE;
	std::string path = "C:\\YourProgramDir\\remoting-config.cfg";
	HlrConnectionCallbacks callbacks = {};

	//We assume you have 2 functions, OnSdpCreated and OnIceCandidateCreated with valid signatures.
	callbacks.sdp_created_cb = OnSdpCreated;
	callbacks.local_ice_candidate_created_cb = OnIceCandidateCreated;

	HlrError err = api.connection.init(
		  path.c_str(), gfx_config, callbacks, &handle);

	if (err == HlrError::eNone)
	{
		//set this to something useful for your case (or nullptr if you don't need it)
		void* userData = nullptr;
		//userData = ...;
		//register callback to get notified when connection state changes.
		//OnConnectionStateChanged is assumed to exist somewhere in your code.
		api.connection.register_connection_state_handler(handle, OnConnectionStateChanged, userData);
	}
}
```

## Initializing Video

Video init is separate from `init()`, due to some D3D11 applications having an extra thread that is the only one allowed to use the D3D11 API.  
Before you can establish a connection and `push_frame()` into the library, you need to call `api.connection.init_video_track()` *from your D3D11 thread*, because it needs to allocate an intermediate texture.

_Note:_ In debug builds the library checks each call to `push_frame()` to see if it was called from the same thread that called `init_video_track()`. This might be helpful for debugging.



```cpp
void InitializeVideo()
{
	//GetD3D11Device(), GetRenderTarget() are assumed to be in your code.

	HlrGraphicsApiConfig gfx_config = {};
	ID3D11Device* d3d_device = GetD3D11Device();
	ID3D11DeviceContext* d3d_context = nullptr;
	d3d_device->GetImmediateContext(&d3d_context);

	gfx_config.d3d_device = d3d_device;
	gfx_config.d3d_context = d3d_context;

	// This texture should be the one that contains your final, rendered frame
	ID3D11Texture2D* d3d11_texture = GetRenderTarget();
	D3D11_TEXTURE2D_DESC d3d_desc;
	d3d11_texture->GetDesc(&d3d_desc);

	gfx_config.render_target_desc = &d3d_desc;
	//"handle" is the one you got from calling init()
	HlrError err = api.connection.init_video_track(handle, gfx_config);
	if (err != HlrError::eNone) {
	  printf("Failed to initialize video track\n");
	}
}
```

After successfully initializing video you can go on with registering for view pose/input events and establishing a connection.

## Registering for View Pose/Input Events

After connecting the client will send you view pose and input events. The first one you need to handle because they contain data required for rendering, i.e. view and projection matrices supplied by the HoloLens.  
Registration works the same way as with the connection state callback before:
```cpp
//this is the handle you got from init()
HlrHandle handle = GetHandleFromInit();

void* userData = nullptr;
//set userData to something useful for you, it will be returned to you in the callback
//userData = ...

//OnPoseReceived is a method supplied by you with the required signature
api.connection.register_view_pose_handler(handle, OnPoseReceived, userData);

//Input works the same way:
api.connection.register_input_event_handler(handle, OnInputReceived, userData);
```

The client will send you matrices that you _must_ render your scene with (otherwise it'll probably look wrong when displayed on the client).

## Establishing a Connection (ISAR-Client-compatible Signaling)

If you're using the regular **ISAR Client** from Microsoft Store, your signaling protocol needs to be compatible with it. Currently it looks like this:
- Plain TCP socket on port 9999
- A message on the socket consists of the following:
  - 32-bit integer length
  - UTF-8 encoded XML content


First thing you need to do after connecting is to send the client your library version so it can check whether it is supported. You can obtain the version via `HlrSvApi.protocol_version`.
Proper versioning is a work-in-progress and *not yet functional* in this release, so the client won't tell you if your version is unsupported. However, if you use the newest ISAR Client version at the time of writing, you should be fine.

After sending the version, the client expects to get an SDP offer. You get the offer by calling `HlrSvApi.signaling.create_offer()` which will trigger the callback you passed to `init()` via `HlrConnectionCallbacks.sdp_created_cb`. Wrap its content into the expected message format and send it to the client.
The client will then send you the SDP answer. After reading it from the socket, call `api.signaling.set_remote_answer()`.

ICE candidates work the same; after creating the offer you will get a callback when an ICE candidate has been created (via `HlrConnectionCallbacks.local_ice_candidate_created_cb`). Send those to the client as you did with the SDP.
When the client sends you an ICE candidate of its own, you need to call `api.signaling.add_ice_candidate()` with the proper parameters.

When the connection state changes, you will get the callback that you registered via `api.connection.register_connection_state_handler()`. If you're connected, the client will start sending you data for rendering and input events.

For reference, the Unity C# side implementation of signaling (`DebugSignaling.cs` and `IsarXmlReaderWriter.cs`) should be included in the code package you received. Use this to get the XML messages in the proper format the client expects.

## Rendering frames

You'll receive pose data from a callback you registered previously:
```cpp
void OnPoseReceived(HlrXrPose const* pose)
{
    //Save pose somewhere so you can use it in the rendering loop later.
    //This is called by the lib from a different thread than your main loop,
    //so synchronizing access to the variable you save your data in
    //(i.e. a class member like m_renderPose) is vital.
    std::lock_guard{m_poseMutex};
    m_renderPose = *pose;
}
```

You need to use the data (i.e. the view and projection matrices for left and right eyes) to render a frame.
When you're done, call `push_frame()` to submit the frame for transmission to the ISAR client app.

**Note**: this doesn't guarantee that the client will receive it. Your frame might get dropped, e.g. when the network is congested or the encoder queue is full.

Calling `push_frame()` looks like this:

```cpp
void PushFrame()
{
  HlrHandle handle = GetHlrHandle();
  ID3D11Texture2D* d3d11_texture = GetRenderedFrame();
  D3D11_TEXTURE2D_DESC d3d_desc;
  d3d11_texture->GetDesc(&d3d_desc);

  HlrGraphicsApiFrame frame = {};
  frame.d3d_frame = d3d11_texture;
  frame.subresource_index = 0;

  //It is important to copy the timestamp variable from the HlrXrPose you rendered this frame with,
  //as the client needs that information to display it correctly.
  frame.timestamp = m_render_pose.timestamp;

  HlrError err = m_server_api.connection.push_frame(handle, frame);
  if (err != HlrError::eNone)
  {
    printf("push_frame failed with error %i\n", err);
  }
}
```

For best quality results, make sure that you push frames every `16.6ms`.
Dropping below that could lead to hologram instability.

### Supported render target formats

`push_frame()` supports textures with the following DXGI formats:

- `DXGI_FORMAT_R8G8B8A8_UNORM`
- `DXGI_FORMAT_R8G8B8A8_TYPELESS`

The texture you submit can be either double-wide (i.e. left and right eyes side-by-side) or a texture array where left eye is at index 0 and right eye is at index 1.

**Using different, unsupported texture formats will very likely trigger a runtime check that crashes the library.**


## Shutting down
The counterpart to `init()` is `close()`; call it when you're completely done using the library and want to shut down your application.

**Caution:** Make sure there are no outstanding calls to the library (such as `push_frame()`) at the time you call `close()`, otherwise you will likely crash.  
Also, don't forget to call `close()` before shutting down your app. It might freeze your process.

## Resetting after losing the connection

The video streaming connection can be lost for various reasons, e.g. networking problems or the client app being closed by a user.
In that case you *don't need to* `close()` the entire library and initialize everything again; instead you can use the `reset()` API.  
Calling `reset()` brings the library to a state where you can go through the signaling flow again and establish a new connection.

## Config file sample

Look into `remoting-config.cfg` for a sample config file for a server. The file format itself is documented in `config-file-format.md`.

## Threading Remarks

- **Never** call an API function from a callback that comes from the library. This can and *likely will* lead to deadlocks and/or crashes.
- `api.connection.init_video_track()` needs to be called from the same thread your app uses for Direct3D 11 calls, otherwise you might run into undefined behavior.
- Callbacks come from library-internal threads. If you need to save the data they carry (i.e. pose data that is later used in the render loop) make sure to synchronize access with mutexes or other sync primitives
