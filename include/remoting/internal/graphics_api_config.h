/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

#ifndef GRAPHICS_API_CONFIG_H
#define GRAPHICS_API_CONFIG_H

#include <stdint.h>

#ifdef _WIN32
#include <d3d11.h>
#endif

#include "HLR_API.h"

#include "remoting/input_types.h"

// Cross-platform Graphics API structs
// -----------------------------------------------------------------------------

HLR_CPP_NS_BEGIN

typedef struct HlrGraphicsApiFrame {
  // Server should set this equal to the timestamp of the
  // XrPose that this frame was rendered with
  int64_t timestamp;

#if defined(_WIN32)
  ID3D11Texture2D* d3d_frame;
  ID3D11Texture2D* d3d_depth_frame;
  uint32_t subresource_index;
#endif
  HlrXrPose pose;
} HlrGraphicsApiFrame;

typedef struct HlrGraphicsApiConfig {
  // in the future (when there's more than a single graphics api) we could
  // make this a union of api-specific config structs.
#if defined(_WIN32)
  ID3D11Device* d3d_device;
  ID3D11DeviceContext* d3d_context;
  D3D11_TEXTURE2D_DESC* render_target_desc;
  D3D11_TEXTURE2D_DESC* depth_render_target_desc;
#endif
} HlrGraphicsApiConfig;

typedef enum HlrTextureFormat{
  HlrTextureFormat_RGBA32,

  HlrTextureFormat_Count,
  HlrTextureFormat_MAX = 0xFFFFFFFF,
} HlrTextureFormat;

HLR_CPP_NS_END

#endif  // GRAPHICS_API_CONFIG_H
