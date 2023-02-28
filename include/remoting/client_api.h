/*
 *  Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

#ifndef REMOTING_CLIENT_API_H
#define REMOTING_CLIENT_API_H

#include "remoting/client_types.h"
#include "remoting/internal/graphics_api_config.h"

HLR_CPP_NS_BEGIN

/// signaling

typedef HlrError (*HlrClSetRemoteDescription)(HlrHandle h, const char* sdp_desc);
typedef HlrError (*HlrClAddIceCandidate)(HlrHandle h, const char* sdp_mid, int32_t mline_index, const char* sdpized_candidate);

typedef struct HlrClSignalingApi {
  HlrClSetRemoteDescription set_remote_offer;
  HlrClAddIceCandidate add_ice_candidate;
} HlrClSignalingApi;

/// connection
typedef HlrError (*HlrClInitCameraTrack)(HlrHandle h, HlrGraphicsApiConfig gfx_config);
typedef HlrError (*HlrClPullFrame)(HlrHandle h, OUT HlrGraphicsApiFrame* frame);
typedef HlrError (*HlrClPushViewPose)(HlrHandle h, HlrXrPose const* pose);
typedef HlrError (*HlrClPushInputEvent)(HlrHandle h, HlrInputEvent const* inputEvent);
typedef HlrError (*HlrClPushSpatialInput)(HlrHandle h, HlrSpatialInput const* spatialInput);
typedef void     (*HlrClRegisterMessageCallbacks)(HlrHandle h, HlrClMessageCallbacks const* callbacks);
typedef HlrError (*HlrClQrIsSupported)(HlrHandle h, bool isSupported);
typedef HlrError (*HlrClQrRequestAccess)(HlrHandle h, HlrQrAccessStatus status);
// typedef HlrError (*HlrClQrGetList)(HlrHandle h, HlrQrCodeList const* data);
typedef HlrError (*HlrClQrAdded)(HlrHandle h, HlrQrCode const* data);
typedef HlrError (*HlrClQrUpdated)(HlrHandle h, HlrQrCode const* data);
typedef HlrError (*HlrClQrRemoved)(HlrHandle h, HlrQrCode const* data);
typedef HlrError (*HlrClQrEnumerationCompleted)(HlrHandle h);

typedef HlrError (*HlrClRegisterAnchorAddCallback)(HlrHandle h, HlrClAnchorAddCallback cb, void* user_data);
typedef HlrError (*HlrClRegisterAnchorDeleteCallback)(HlrHandle h, HlrClAnchorDeleteCallback cb, void* user_data);
typedef HlrError (*HlrClRegisterAnchorExportCallback)(HlrHandle h, HlrClAnchorExportCallback cb, void* user_data);
typedef HlrError (*HlrClRegisterAnchorImportCallback)(HlrHandle h, HlrClAnchorImportCallback cb, void* user_data);
typedef HlrError (*HlrClRegisterAnchorCreateStoreCallback)(HlrHandle h, HlrClAnchorCreateStoreCallback cb, void* user_data);
typedef HlrError (*HlrClRegisterAnchorDestroyStoreCallback)(HlrHandle h, HlrClAnchorDestroyStoreCallback cb, void* user_data);
typedef HlrError (*HlrClRegisterAnchorClearStoreCallback)(HlrHandle h, HlrClAnchorClearStoreCallback cb, void* user_data);
typedef HlrError (*HlrClRegisterAnchorPersistCallback)(HlrHandle h, HlrClAnchorPersistAnchorCallback cb, void* user_data);
typedef HlrError (*HlrClRegisterAnchorUnpersistCallback)(HlrHandle h, HlrClAnchorUnpersistAnchorCallback cb, void* user_data);
typedef HlrError (*HlrClRegisterAnchorEnumeratePersistedAnchorNamesCallback)(HlrHandle h, HlrClAnchorEnumeratePersistedAnchorNamesCallback cb, void* user_data);
typedef HlrError (*HlrClRegisterAnchorCreateSpatialAnchorFromPersistedNameCallback)(HlrHandle h, HlrClAnchorCreateSpatialAnchorFromPersistedNameCallback cb, void* user_data);
typedef HlrError (*HlrClUnregisterAnchorAddCallback)(void);
typedef HlrError (*HlrClUnregisterAnchorDeleteCallback)(void);
typedef HlrError (*HlrClUnregisterAnchorExportCallback)(void);
typedef HlrError (*HlrClUnregisterAnchorImportCallback)(void);
typedef HlrError (*HlrClUnregisterAnchorCreateStoreCallback)(void);
typedef HlrError (*HlrClUnregisterAnchorDestroyStoreCallback)(void);
typedef HlrError (*HlrClUnregisterAnchorClearStoreCallback)(void);
typedef HlrError (*HlrClUnregisterAnchorPersistCallback)(void);
typedef HlrError (*HlrClUnregisterAnchorUnpersistCallback)(void);
typedef HlrError (*HlrClUnregisterAnchorEnumeratePersistedAnchorNamesCallback)(void);
typedef HlrError (*HlrClUnregisterAnchorCreateSpatialAnchorFromPersistedNameCallback)(void);

typedef HlrError (*HlrClAnchorAdded)(HlrHandle h, const HlrAnchor* data);
typedef HlrError (*HlrClAnchorUpdated)(HlrHandle h, const HlrAnchor* data, size_t num_anchors );
typedef HlrError (*HlrClAnchorDeleted)(HlrHandle h, const HlrAnchorId* id);
typedef HlrError (*HlrClAnchorExported)(HlrHandle h, const byte* data, size_t size);
typedef HlrError (*HlrClAnchorImported)(HlrHandle h, const HlrAnchor* anchors, size_t num_anchors);
typedef HlrError (*HlrClAnchorCreateStoreResponse)(HlrHandle h, bool is_succeeded);
typedef HlrError (*HlrClAnchorDestroyStoreResponse)(HlrHandle h, bool is_succeeded);
typedef HlrError (*HlrClAnchorClearStoreResponse)(HlrHandle h, bool is_succeeded);
typedef HlrError (*HlrClAnchorPersisted)(HlrHandle h, const HlrAnchorId* id);
typedef HlrError (*HlrClAnchorUnpersisted)(HlrHandle h, bool is_succeeded);
typedef HlrError (*HlrClAnchorPersistedNamesEnumerated)(HlrHandle h, const char** names, size_t num_names);
typedef HlrError (*HlrClAnchorCreatedFromPersistedName)(HlrHandle h, const HlrAnchor* anchor);

typedef void     (*HlrClEnableMicrophoneCapture)(bool enable);
typedef void     (*HlrClRegisterFrameHandler)(HlrHandle h, HlrClFrameCallback cb, void* user_data);
typedef void     (*HlrClUnregisterFrameHandler)(HlrHandle h);
typedef HlrError (*HlrClPushCameraFrame)(HlrHandle h, HlrGraphicsApiFrame frame, const uint8_t* frame_extension, uint32_t frame_extension_size);

typedef struct HlrClConnectionApi {
  HlrInit init;
  HlrClose close;
  HlrClInitCameraTrack init_camera_track;
  HlrClPushCameraFrame push_camera_frame;
  HlrClPullFrame pull_frame;
  HlrClPushViewPose push_view_pose;
  HlrClPushInputEvent push_input;
  HlrClPushSpatialInput push_spatial_input;
  HlrPushCustomMessage push_custom_message;
  HlrRegisterCustomMessageHandler register_custom_message_handler;
  HlrUnregisterCustomMessageHandler unregister_custom_message_handler;
  HlrClRegisterMessageCallbacks register_message_callbacks;
  HlrProcessMessages process_messages;
  HlrClQrIsSupported qr_is_supported;
  HlrClQrRequestAccess qr_request_access;
  // HlrClQrGetList qr_get_list;
  HlrClQrAdded qr_added;
  HlrClQrUpdated qr_updated;
  HlrClQrRemoved qr_removed;
  HlrClQrEnumerationCompleted qr_enumeration_completed;
  HlrClEnableMicrophoneCapture enable_microphone_capture;
  HlrClRegisterFrameHandler register_frame_handler;
  HlrClUnregisterFrameHandler unregister_frame_handler;
  //Anchors
  HlrClRegisterAnchorAddCallback register_anchor_add_callback;
  HlrClRegisterAnchorDeleteCallback register_anchor_delete_callback;
  HlrClRegisterAnchorExportCallback register_anchor_export_callback;
  HlrClRegisterAnchorImportCallback register_anchor_import_callback;
  HlrClRegisterAnchorCreateStoreCallback register_anchor_store_create_callback;
  HlrClRegisterAnchorDestroyStoreCallback register_anchor_store_destroy_callback;
  HlrClRegisterAnchorClearStoreCallback register_anchor_store_clear_callback;
  HlrClRegisterAnchorPersistCallback register_anchor_persist_callback;
  HlrClRegisterAnchorUnpersistCallback register_anchor_unpersist_callback;
  HlrClRegisterAnchorEnumeratePersistedAnchorNamesCallback register_anchor_enumerate_pers_names_callback;
  HlrClRegisterAnchorCreateSpatialAnchorFromPersistedNameCallback register_anchor_create_from_pers_name_callback;

  HlrClUnregisterAnchorAddCallback  unregister_anchor_add_callback;
  HlrClUnregisterAnchorDeleteCallback  unregister_anchor_delete_callback;
  HlrClUnregisterAnchorExportCallback  unregister_anchor_export_callback;
  HlrClUnregisterAnchorImportCallback  unregister_anchor_import_callback;
  HlrClUnregisterAnchorCreateStoreCallback unregister_anchor_store_create_callback;
  HlrClUnregisterAnchorDestroyStoreCallback unregister_anchor_store_destroy_callback;
  HlrClUnregisterAnchorClearStoreCallback unregister_anchor_store_clear_callback;
  HlrClUnregisterAnchorPersistCallback unregister_anchor_persist_callback;
  HlrClUnregisterAnchorUnpersistCallback unregister_anchor_unpersist_callback;
  HlrClUnregisterAnchorEnumeratePersistedAnchorNamesCallback unregister_anchor_enumerate_pers_names_callback;
  HlrClUnregisterAnchorCreateSpatialAnchorFromPersistedNameCallback unregister_anchor_create_from_pers_name_callback;

  HlrClAnchorAdded anchor_added;
  HlrClAnchorUpdated anchor_updated;
  HlrClAnchorDeleted anchor_deleted;
  HlrClAnchorExported anchor_exported;
  HlrClAnchorImported anchor_imported;
  HlrClAnchorCreateStoreResponse anchor_create_store_response;
  HlrClAnchorDestroyStoreResponse anchor_destroy_store_response;
  HlrClAnchorClearStoreResponse anchor_clear_store_response;
  HlrClAnchorPersisted anchor_persisted;
  HlrClAnchorUnpersisted anchor_unpersisted;
  HlrClAnchorPersistedNamesEnumerated anchor_persisted_names_enumerated;
  HlrClAnchorCreatedFromPersistedName anchor_created_from_persisted_name;

} HlrClConnectionApi;

// Client API entrypoint
// Contains function pointers to version-specific implementations.
typedef struct HlrClApi {
  HlrVersion protocol_version;
  HlrClSignalingApi signaling;
  HlrClConnectionApi connection;
} HlrClApi;

/// foreign function interface

// Creates a client API with a version-specific vtable.
// @param protocol_version protocol version retrieved from the server
// @param api remoting client API
HLR_API(HlrError)
hlr_cl_create_remoting_api(HlrVersion protocol_version, HlrClApi* api);

HLR_CPP_NS_END

#endif  // REMOTING_CLIENT_API_H
