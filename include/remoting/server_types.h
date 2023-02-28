/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

#ifndef REMOTING_SERVER_TYPES_H
#define REMOTING_SERVER_TYPES_H

#include "remoting/types.h"

HLR_CPP_NS_BEGIN

typedef struct HlrAudioData {
  const void* data;
  int32_t bits_per_sample;
  int32_t sample_rate;
  size_t number_of_channels;
  size_t samples_per_channel;
} HlrAudioData;

typedef void(*HlrSvViewPoseReceivedCallback)(HlrXrPose const* pose, void* user_data);
typedef void(*HlrSvInputEventReceivedCallback)(HlrInputEvent const* input_event, void* user_data);
typedef void(*HlrSvSpatialInputReceivedCallback)(HlrSpatialInput const* spatial_input, void* user_data);
typedef void(*HlrSvAudioDataReceivedCallback)(HlrAudioData const* audio_data, void* user_data);
// typedef void ( *HlrSvQrMessageReceivedCallback)(HlrQrMessage const* message, void* user_data);
typedef void(*HlrSvQrIsSupportedCallback)(HlrQrIsSupported const* data, void* user_data);
typedef void(*HlrSvQrRequestAccessCallback)(HlrQrRequestAccess const* data, void* user_data);
typedef void(*HlrSvQrAddedCallback)(HlrQrAdded const* data, void* user_data);
typedef void(*HlrSvQrUpdatedCallback)(HlrQrUpdated const* data, void* user_data);
typedef void(*HlrSvQrRemovedCallback)(HlrQrRemoved const* data, void* user_data);
typedef void(*HlrSvQrEnumerationCompletedCallback)(void* user_data);

typedef void(*HlrSvStatsCallback)(const void* stats_data, void* user_data);

typedef void (*HlrSvAnchorAddCallback)(HlrAnchor const* anchorData, void* user_data);
typedef void (*HlrSvAnchorDeleteCallback)(HlrAnchorId const* anchorId, void* user_data);
typedef void (*HlrSvAnchorExportCallback)(const char* data, size_t num_bytes, void* user_data);
typedef void (*HlrSvAnchorImportCallback)(HlrAnchor const* anchorData, size_t num_anchors, void* user_data);
typedef void (*HlrSvAnchorUpdateCallback)(HlrAnchor const* anchorData, size_t num_anchors, void* user_data);
typedef void (*HlrSvAnchorCreateStoreCallback)(bool is_succeded, void* user_data);
typedef void (*HlrSvAnchorDestroyStoreCallback)(bool is_succeded, void* user_data);
typedef void (*HlrSvAnchorClearStoreCallback)(bool is_succeded, void* user_data);
typedef void (*HlrSvAnchorEnumeratePersistedAnchorNamesCallback)(size_t num_anchors, const char** names, void* user_data);
typedef void (*HlrSvAnchorCreateSpatialAnchorFromPersistedNameCallback)(HlrAnchor const* anchorData, void* user_data);
typedef void (*HlrSvAnchorPersistSpatialAnchorCallback)(HlrAnchorId const* anchorId, void* user_data);
typedef void (*HlrSvAnchorUnpersistSpatialAnchorCallback)(bool is_succeded, void* user_data);

typedef void (*HlrSvRegisterAnchorAddCallback)(HlrHandle h, HlrSvAnchorAddCallback cb, void* user_data);
typedef void (*HlrSvRegisterAnchorDeleteCallback)(HlrHandle h, HlrSvAnchorDeleteCallback cb, void* user_data);
typedef void (*HlrSvRegisterAnchorExportCallback)(HlrHandle h, HlrSvAnchorExportCallback cb, void* user_data);
typedef void (*HlrSvRegisterAnchorImportCallback)(HlrHandle h, HlrSvAnchorImportCallback cb, void* user_data);
typedef void (*HlrSvRegisterAnchorUpdateCallback)(HlrHandle h, HlrSvAnchorUpdateCallback cb, void* user_data);
typedef void (*HlrSvRegisterAnchorCreateStoreCallback)(HlrHandle h, HlrSvAnchorCreateStoreCallback cb, void* user_data);
typedef void (*HlrSvRegisterAnchorDestroyStoreCallback)(HlrHandle h, HlrSvAnchorDestroyStoreCallback cb, void* user_data);
typedef void (*HlrSvRegisterAnchorClearStoreCallback)(HlrHandle h, HlrSvAnchorClearStoreCallback cb, void* user_data);
typedef void (*HlrSvRegisterAnchorPersistCallback)(HlrHandle h, HlrSvAnchorPersistSpatialAnchorCallback cb, void* user_data);
typedef void (*HlrSvRegisterAnchorUnpersistSpatialAnchorCallback)(HlrHandle h, HlrSvAnchorUnpersistSpatialAnchorCallback cb, void* user_data);
typedef void (*HlrSvRegisterAnchorEnumeratePersistedAnchorNamesCallback)(HlrHandle h, HlrSvAnchorEnumeratePersistedAnchorNamesCallback cb, void* user_data);
typedef void (*HlrSvRegisterAnchorCreateSpatialAnchorFromPersistedNameCallback)(HlrHandle h, HlrSvAnchorCreateSpatialAnchorFromPersistedNameCallback cb, void* user_data);

typedef void (*HlrSvUnregisterAnchorAddCallback)(void);
typedef void (*HlrSvUnregisterAnchorDeleteCallback)(void);
typedef void (*HlrSvUnregisterAnchorExportCallback)(void);
typedef void (*HlrSvUnregisterAnchorImportCallback)(void);
typedef void (*HlrSvUnregisterAnchorUpdateCallback)(void);
typedef void (*HlrSvUnregisterAnchorCreateStoreCallback)(void);
typedef void (*HlrSvUnregisterAnchorDestroyStoreCallback)(void);
typedef void (*HlrSvUnregisterAnchorClearStoreCallback)(void);
typedef void (*HlrSvUnregisterAnchorPersistCallback)(void);
typedef void (*HlrSvUnregisterAnchorEnumeratePersistedAnchorNamesCallback)(void);
typedef void (*HlrSvUnregisterAnchorCreateSpatialAnchorFromPersistedNameCallback)(void);
typedef void (*HlrSvUnregisterAnchorUnpersistSpatialAnchorCallback)(void);

typedef struct HlrSvMessageCallbacks {
  HlrSvViewPoseReceivedCallback view_pose_received_cb;
  HlrSvInputEventReceivedCallback input_event_received_cb;
  // HlrSvQrMessageReceivedCallback qr_message_received_cb;
  HlrSvQrIsSupportedCallback qr_is_supported;
  HlrSvQrRequestAccessCallback qr_request_access;
  HlrSvQrAddedCallback qr_added;
  HlrSvQrUpdatedCallback qr_updated;
  HlrSvQrRemovedCallback qr_removed;
  HlrSvQrEnumerationCompletedCallback qr_enumeration_completed;
  void* user_data;
} HlrSvMessageCallbacks;

HLR_CPP_NS_END

#endif  // REMOTING_SERVER_TYPES_H
