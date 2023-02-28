/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

#ifndef REMOTING_SERVER_API_H
#define REMOTING_SERVER_API_H

#include "remoting/internal/graphics_api_config.h"

#include "remoting/server_types.h"

HLR_CPP_NS_BEGIN

/// signaling

// Creates a session description offer.
// Calls HlrSdpCreatedCallback if creation was successful.
typedef HlrError (*HlrSvCreateOffer)(HlrHandle h);
typedef HlrError (*HlrSvSetRemoteDescription)(HlrHandle h, const char* sdp_desc);
typedef HlrError (*HlrSvAddIceCandidate)(HlrHandle h, const char* sdp_mid, int32_t mline_index, const char* sdpized_candidate);

typedef struct HlrSvSignalingApi {
  HlrSvCreateOffer create_offer;
  HlrSvSetRemoteDescription set_remote_answer;
  HlrSvAddIceCandidate add_ice_candidate;
} HlrSvSignalingApi;

/// connection

typedef HlrError (*HlrSvReset)(HlrHandle h);
typedef HlrError (*HlrSvInitVideoTrack)(HlrHandle h, HlrGraphicsApiConfig gfx_config);
typedef HlrError (*HlrSvPushFrame)(HlrHandle h, HlrGraphicsApiFrame frame, int64_t bandwidth);
typedef HlrError (*HlrSvPushAudioData)(HlrHandle h, HlrAudioData data);
//typedef HlrError (*HlrSvPushFrame)(HlrHandle h, HlrGraphicsApiFrame frame);
typedef HlrError (*HlrSvSetAudioTrackEnabled)(HlrHandle h, int32_t enabled);
typedef void (*HlrSvRegisterMessageCallbacks)(HlrHandle h, HlrSvMessageCallbacks const* callbacks);
typedef HlrError (*HlrSvQrIsSupported)(HlrHandle h);
typedef HlrError (*HlrSvQrRequestAccess)(HlrHandle h);
typedef HlrError (*HlrSvQrStart)(HlrHandle h);
typedef HlrError (*HlrSvQrStop)(HlrHandle h);
// typedef HlrError(* HlrSvQrGetList)(HlrHandle h);

typedef void (*HlrSvRegisterConnectionStateHandler)(HlrHandle h, HlrConnectionStateChangedCallback cb, void* user_data);
typedef void (*HlrSvUnregisterConnectionStateHandler)(HlrHandle h, HlrConnectionStateChangedCallback cb);
typedef void (*HlrSvUnregisterConnectionStateHandler2)(HlrHandle h, HlrConnectionStateChangedCallback cb, void* user_data);

typedef void (*HlrSvRegisterViewPoseHandler)(HlrHandle h, HlrSvViewPoseReceivedCallback cb, void* user_data);
typedef void (*HlrSvUnregisterViewPoseHandler)(HlrHandle h, HlrSvViewPoseReceivedCallback cb);
typedef void (*HlrSvUnregisterViewPoseHandler2)(HlrHandle h, HlrSvViewPoseReceivedCallback cb, void* user_data);

typedef void (*HlrSvRegisterInputEventHandler)(HlrHandle h, HlrSvInputEventReceivedCallback cb, void* user_data);
typedef void (*HlrSvUnregisterInputEventHandler)(HlrHandle h, HlrSvInputEventReceivedCallback cb);
typedef void (*HlrSvUnregisterInputEventHandler2)(HlrHandle h, HlrSvInputEventReceivedCallback cb, void* user_data);

typedef void (*HlrSvRegisterSpatialInputHandler)(HlrHandle h, HlrSvSpatialInputReceivedCallback cb, void* user_data);
typedef void (*HlrSvUnregisterSpatialInputHandler)(HlrHandle h, HlrSvSpatialInputReceivedCallback cb);
typedef void (*HlrSvUnregisterSpatialInputHandler2)(HlrHandle h, HlrSvSpatialInputReceivedCallback cb, void* user_data);

typedef void (*HlrSvRegisterAudioDataHandler)(HlrHandle h, HlrSvAudioDataReceivedCallback cb, void* user_data);
typedef void (*HlrSvUnregisterAudioDataHandler)(HlrHandle h, HlrSvAudioDataReceivedCallback cb);
typedef void (*HlrSvUnregisterAudioDataHandler2)(HlrHandle h, HlrSvAudioDataReceivedCallback cb, void* user_data);

typedef void (*HlrSvRegisterStatsHandler)(HlrHandle h, HlrSvStatsCallback cb, void* user_data);
typedef void (*HlrSvUnregisterStatsHandler)(HlrHandle h, HlrSvStatsCallback cb, void* user_data);
typedef void (*HlrSvGetStats)(HlrHandle h);

// Peer connection specific parts of server api.
// Groups webrtc connection related methods together.
// TODO: not really, currently contains everything that depends on a valid connection + init, close & reset
typedef struct HlrSvConnectionApi {
  HlrInit init;
  HlrInit2 init2;
  HlrClose close;
  HlrSvReset reset;
  HlrSvInitVideoTrack init_video_track;
  HlrSvPushFrame push_frame;
  HlrSvPushAudioData push_audio_data;
  HlrPushCustomMessage push_custom_message;
  HlrSvSetAudioTrackEnabled set_audio_track_enabled;

  HlrSvRegisterConnectionStateHandler register_connection_state_handler;
  HlrSvUnregisterConnectionStateHandler unregister_connection_state_handler;

  HlrSvRegisterViewPoseHandler register_view_pose_handler;
  HlrSvUnregisterViewPoseHandler unregister_view_pose_handler;

  HlrSvRegisterInputEventHandler register_input_event_handler;
  HlrSvUnregisterInputEventHandler unregister_input_event_handler;

  HlrSvRegisterSpatialInputHandler register_spatial_input_handler;
  HlrSvUnregisterSpatialInputHandler unregister_spatial_input_handler;

  HlrRegisterCustomMessageHandler register_custom_message_handler;
  HlrUnregisterCustomMessageHandler unregister_custom_message_handler;

  HlrSvRegisterAudioDataHandler register_audio_data_handler;
  HlrSvUnregisterAudioDataHandler unregister_audio_data_handler;

  // KL: maybe we could put all these inside a struct QrApi
  HlrSvRegisterMessageCallbacks register_message_callbacks;
  HlrProcessMessages process_messages;
  HlrSvQrIsSupported qr_is_supported;
  HlrSvQrRequestAccess qr_request_access;
  HlrSvQrStart qr_start;
  HlrSvQrStop qr_stop;
  // HlrSvQrGetList qr_get_list;

  HlrSvUnregisterConnectionStateHandler2 unregister_connection_state_handler2;
  HlrSvUnregisterViewPoseHandler2 unregister_view_pose_handler2;
  HlrSvUnregisterInputEventHandler2 unregister_input_event_handler2;
  HlrSvUnregisterSpatialInputHandler2 unregister_spatial_input_handler2;
  HlrSvUnregisterAudioDataHandler2 unregister_audio_data_handler2;

  HlrSvRegisterStatsHandler register_stats_handler;
  HlrSvUnregisterStatsHandler unregister_stats_handler;
  HlrSvGetStats get_stats;

  // Anchor API
  HlrSvRegisterAnchorAddCallback register_anchor_add_callback;
  HlrSvRegisterAnchorDeleteCallback register_anchor_delete_callback;
  HlrSvRegisterAnchorUpdateCallback register_anchor_update_callback;
  HlrSvRegisterAnchorImportCallback register_anchor_import_callback;
  HlrSvRegisterAnchorExportCallback register_anchor_export_callback;
  HlrSvRegisterAnchorCreateStoreCallback  register_anchor_store_create_callback;
  HlrSvRegisterAnchorDestroyStoreCallback register_anchor_store_destroy_callback;
  HlrSvRegisterAnchorClearStoreCallback   register_anchor_store_clear_callback;
  HlrSvRegisterAnchorPersistCallback      register_anchor_persist_callback;
  HlrSvRegisterAnchorEnumeratePersistedAnchorNamesCallback register_anchor_enumerate_persisted_callback;
  HlrSvRegisterAnchorCreateSpatialAnchorFromPersistedNameCallback register_anchor_create_spatial_from_name_callback;
  HlrSvRegisterAnchorUnpersistSpatialAnchorCallback register_anchor_unpersist_spatial_anchor_callback;

  HlrSvUnregisterAnchorAddCallback unregister_anchor_add_callback;
  HlrSvUnregisterAnchorDeleteCallback unregister_anchor_delete_callback;
  HlrSvUnregisterAnchorUpdateCallback unregister_anchor_update_callback;
  HlrSvUnregisterAnchorImportCallback unregister_anchor_import_callback;
  HlrSvUnregisterAnchorExportCallback unregister_anchor_export_callback;
  HlrSvUnregisterAnchorCreateStoreCallback unregister_anchor_store_create_callback;
  HlrSvUnregisterAnchorDestroyStoreCallback unregister_anchor_store_destroy_callback;
  HlrSvUnregisterAnchorClearStoreCallback   unregister_anchor_store_clear_callback;
  HlrSvUnregisterAnchorPersistCallback      unregister_anchor_persist_callback;
  HlrSvUnregisterAnchorEnumeratePersistedAnchorNamesCallback unregister_anchor_enumerate_persisted_callback;
  HlrSvUnregisterAnchorCreateSpatialAnchorFromPersistedNameCallback unregister_anchor_create_spatial_from_name_callback;
  HlrSvUnregisterAnchorUnpersistSpatialAnchorCallback unregister_anchor_unpersist_spatial_anchor_callback;

} HlrSvConnectionApi;

// Server API entrypoint.
// Contains function pointers to version-specific implementations.
typedef struct HlrSvApi {
  HlrVersion protocol_version;
  HlrSvSignalingApi signaling;
  HlrSvConnectionApi connection;
} HlrSvApi;

/// foreign function interface

// Creates a server API with a version-specific vtable.
HLR_API(HlrError)
hlr_sv_create_remoting_api(HlrSvApi* api);

HLR_API(HlrError)
isar_try_acquire_camera_image_buffer(unsigned char *pData, long lDataLen);

HLR_API(HlrError)
isar_try_acquire_latest_camera_image(HlrTextureFormat* format, int32_t* width, int32_t* height, int64_t* timestamp);

HLR_CPP_NS_END

#endif  // REMOTING_SERVER_API_H
