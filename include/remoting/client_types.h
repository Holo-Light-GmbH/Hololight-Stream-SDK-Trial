/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

#ifndef REMOTING_CLIENT_TYPES_H
#define REMOTING_CLIENT_TYPES_H

#include "remoting/types.h"

HLR_CPP_NS_BEGIN

// typedef void (*HlrClQrCodeMessageReceivedCallback)(HlrQrCodeMessage* message, void* user_data);

// // TODO: this struct api sucks, we shouldn't expose that crap to the public api
// typedef struct HlrClMessageCallbacks {
//   HlrClQrCodeMessageReceivedCallback qr_code_message_received_cb;

//   void* user_data;
// } HlrClMessageCallbacks;

typedef void (*HlrClQrIsSupportedCallback)(void* user_data);
typedef void (*HlrClQrRequestAccessCallback)(void* user_data);
typedef void (*HlrClQrStartCallback)(void* user_data);
typedef void (*HlrClQrStopCallback)(void* user_data);
//typedef void (*HlrClQrGetListCallback)(void* user_data);
typedef void (*HlrClFrameCallback)(HlrGraphicsApiFrame frame, void* user_data);
typedef void (*HlrClAnchorAddCallback)(HlrAnchor const* anchorData, void* user_data);
typedef void (*HlrClAnchorDeleteCallback)(HlrAnchorId const* anchorId, void* user_data);
typedef void (*HlrClAnchorExportCallback)(void* user_data);
typedef void (*HlrClAnchorImportCallback)(const byte* anchor_data, size_t size, void* user_data);
typedef void (*HlrClAnchorCreateStoreCallback)(void* user_data);
typedef void (*HlrClAnchorDestroyStoreCallback)(void* user_data);
typedef void (*HlrClAnchorClearStoreCallback)(void* user_data);
typedef void (*HlrClAnchorPersistAnchorCallback)(const char* anchor_name, HlrAnchorId const* id, void* user_data);
typedef void (*HlrClAnchorUnpersistAnchorCallback)(const char* anchor_name, void* user_data);
typedef void (*HlrClAnchorEnumeratePersistedAnchorNamesCallback)(void* user_data);
typedef void (*HlrClAnchorCreateSpatialAnchorFromPersistedNameCallback)(const char* anchor_name, void* user_data);

typedef struct HlrClMessageCallbacks {
  HlrClQrIsSupportedCallback qr_is_supported;
  HlrClQrRequestAccessCallback qr_request_access;
  HlrClQrStartCallback qr_start;
  HlrClQrStopCallback qr_stop;
  //HlrClQrGetListCallback qr_get_list_cb;
  void* user_data;
} HlrClMessageCallbacks;

HLR_CPP_NS_END

#endif // REMOTING_CLIENT_TYPES_H
