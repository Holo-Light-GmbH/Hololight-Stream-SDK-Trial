/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

#ifndef REMOTING_API_TYPES_H
#define REMOTING_API_TYPES_H

#include "remoting/internal/HLR_API.h"
#include "remoting/internal/graphics_api_config.h"

#include "remoting/input_types.h"
#include "remoting/version.h"

#include <assert.h>
#include <stdbool.h>
#include <stdint.h>

HLR_CPP_NS_BEGIN

// Type aliases
// -----------------------------------------------------------------------------

typedef void* HlrHandle;
const HlrHandle HLR_INVALID_HANDLE = (void*)-1;
const HlrHandle HLR_VALID_HANDLE = (void*)0xc0ffee;

// Structs
// -----------------------------------------------------------------------------

typedef enum HlrSdpType {
  HlrSdpType_OFFER = 0,
  HlrSdpType_ANSWER,

  HlrSdpType_INTERNAL_FORCE_INT32_SIZE = 0xFFFFFFFF
} HlrSdpType;
static_assert(sizeof(HlrSdpType) == sizeof(int32_t), "enums need to be 32 bits for foreign function bindings");

typedef enum HlrConnectionState {
  HlrConnectionState_DISCONNECTED,
  HlrConnectionState_CONNECTED,
  HlrConnectionState_FAILED,

  HlrConnectionState_INTERNAL_FORCE_INT32_SIZE = 0xFFFFFFFF
} HlrConnectionState;
static_assert(sizeof(HlrConnectionState) == sizeof(int32_t), "enums need to be 32 bits for foreign function bindings");

/* Allows user to send custom data over the DataChannel.
 * Buffer size is limited to 16KB due to WebRTC recommendations, see
 * RFC 8831 section 6.6 for more info. */
typedef struct HlrCustomMessage {
  uint32_t length; // TODO: rename to size, also switch field order
  uint8_t const* data;
} HlrCustomMessage;

// Callbacks
// -----------------------------------------------------------------------------

// PeerConnection callbacks
// --------------------
typedef void(*HlrConnectionStateChangedCallback)(HlrConnectionState new_state, void* user_data);
typedef void(*HlrSdpCreatedCallback)(HlrSdpType type, const char* sdp, void* user_data);
// TODO: for correctness, maybe we should also have a set sdp callback because
// that can fail... although, we don't handle that at all currently and not sure
// how a client could given the information.
typedef void (*HlrLocalIceCandidateCreatedCallback)(
    const char* sdp_mline, int32_t mline_index, const char* sdpized_ice_candidate, void* user_data);
typedef void (*HlrCustomMessageCallback)(HlrCustomMessage const* const message, void* user_data);

// DataChannel callbacks
// ---------------

// Theoretically we could have c++-style constructors like d3d11 does, though that'd mean lots of ifdefs.
// contains all callbacks (client and server), maybe separate them sometime later
typedef struct HlrConnectionCallbacks {
  HlrConnectionStateChangedCallback connection_state_changed_cb; // DEPRECATED (at least on the server) TODO: unify with client
  HlrSdpCreatedCallback sdp_created_cb; // see CreateOffer/CreateAnswer
  HlrLocalIceCandidateCreatedCallback local_ice_candidate_created_cb;

  // do we need more than 1 of these? Currently this is only used on the
  // client because C# bindings don't need it right now.
  void* user_data;
} HlrConnectionCallbacks;

// TODO(viktor): those errors are way to specific, we should keep them as
// generic as possible and write proper error messages
typedef enum HlrError {
  eNone = 0,                  // This should only be used at the API boundary.
  eAlreadyInitialized,        // Init has already been called
  eInvalidHandle,             // handle is invalid or doesn't match global_handle
  ePeerConnectionFactory,     // CreatePeerConnectionFactory failed
  ePeerConnection,            // CreatePeerConnection failed
  eDataChannel_Creation,      // CreateDataChannel failed
  eDataChannel_Send,          // Sending via DataChannel
  eDataChannel_Send_NotOpen,  // Sending via DataChannel while state is != open // TODO: deprecated
  eAddTrack,                  // AddTrack failed
  eVideoSource,
  eVideoTrack,
  eStartRtcEventLog,  // StartRtcEventLog failed
  eConfig_UnsupportedOrMissingRole,
  eConfig_UnsupportedOrMissingEncoder,
  eConfig_UnsupportedOrMissingDecoder,
  eConfig_UnsupportedOrMissingVideoSource,
  eConfig_SignalingInvalidOrMissing,
  eConfig_SignalingIpInvalidOrMissing,
  eConfig_SignalingPortInvalidOrMissing,
  eNotConnected,        // user wants to do something but isn't connected
  eFileOpen,            // failed to open file
  eConfigParse,         // failed to parse config file
  eSdpParse,            // failed to parse session decription or ICE candidate
  eNoFrame,             // user tried to pull frame but there is none
  eUnsupportedVersion,  // user tried to create api with unsupported version
  eInvalidArgument,
  eMessageTooLong,      // custom message data too long
  eAudioTrack,          // failed to initialize audio track
  eAudioTrack_NotInitialized, // tried to perform an operation on audio track but there is none
  // eInput_NotSupported,

  eUnknown = 0xFFFFFFFF,  // force sizeof int32
} HlrError;
static_assert(sizeof(HlrError) == sizeof(int32_t), "enums need to be 32 bits for foreign function bindings");

#define HLR_RETURN_ON_ERROR(expr) if (HlrError const err = expr) return err

// Configuration types
// -----------------------------------------------------------------------------

typedef enum Role { Server, Client } Role;

typedef enum VideoSource { None, D3D11, Webcam } VideoSource;

typedef enum Encoder {
  // libwebrtc built-in software encoders for H264, VP8, VP9 formats.
  Encoder_Builtin,
  // Media Foundation (with Windows 10 features). H264 only.
  Encoder_H264_UWP
} Encoder;

typedef enum Decoder {
  // libwebrtc built-in software decoders for H264, VP8, VP9 formats.
  Decoder_Builtin,
  // Media Foundation (with Windows 10 features). H264 only.
  Decoder_H264_UWP
} Decoder;

typedef enum DiagnosticOptions{
    Diagnostics_Disabled = 0,
    EnableTracing = (1 << 0),
    EnableEventLog = (1 << 1),
    EnableStatsCollector = (1 << 2)
} DiagnosticOptions;

typedef struct IceServerConfig {
    const wchar_t* url;
    const wchar_t* username;
    const wchar_t* password;
} IceServerConfig;

typedef enum AudioDevice {
    Default,
    Audio_Disabled
} AudioDevice;

typedef struct RenderConfig {
    uint32_t width;
    uint32_t height;
    uint32_t num_views;
    uint32_t encoder_bitrate_kbps;
    uint32_t framerate;
} RenderConfig;

typedef struct HlrConfig
{
  Role role_;
  VideoSource video_source_;
  Encoder encoder_;
  Decoder decoder_;
  DiagnosticOptions diagnostic_options_;
  AudioDevice audio_device_;
  uint32_t num_ice_servers_;
  IceServerConfig* ice_servers_;
  RenderConfig render_settings_;
} HlrConfig;

// Shared function pointers (public shared_api.h)
// -----------------------------------------------------------------------------

// Initializes peer connection to server.
typedef HlrError(*HlrInit)(const char* path, HlrGraphicsApiConfig gfx_config,
                           HlrConnectionCallbacks callbacks, HlrHandle* const OUT connection_handle);
typedef HlrError(*HlrInit2)(const HlrConfig* config, HlrGraphicsApiConfig gfx_config,
                           HlrConnectionCallbacks callbacks, HlrHandle* const OUT connection_handle);
// Destroys all resources represented by the handle.
// If there is a connection it is disconnected and disposed as well.
typedef HlrError(*HlrClose)(HlrHandle* connection_handle);
typedef     void(*HlrProcessMessages)(void);

typedef HlrError(*HlrPushCustomMessage)(HlrHandle h, HlrCustomMessage* message);
typedef void (*HlrRegisterCustomMessageHandler)(HlrHandle h, HlrCustomMessageCallback handler, void* user_data);
typedef void (*HlrUnregisterCustomMessageHandler)(HlrHandle h, HlrCustomMessageCallback handler, void* user_data);

typedef void(*HlrDataMessageCallback)(const uint8_t* data, uint32_t size, void* user_data);

HLR_CPP_NS_END

#endif  // REMOTING_API_TYPES_H
