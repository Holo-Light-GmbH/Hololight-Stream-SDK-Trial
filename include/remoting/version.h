/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

#ifndef REMOTING_VERSION_H
#define REMOTING_VERSION_H

// Create packed unsigned int with semantic version
#define HLR_MAKE_PACKED_VERSION(major, minor, patch) \
    (((major) << 22) | ((minor) << 12) | (patch))

// This is the current version of the library
// change this value to increase the version
#define HLR_LATEST_VERSION HLR_MAKE_PACKED_VERSION(2, 6, 0)

#define HLR_NEXT_MAJOR_VERSION HLR_LATEST_VERSION + HLR_MAKE_PACKED_VERSION(1, 0, 0)
#define HLR_NEXT_MINOR_VERSION HLR_LATEST_VERSION + HLR_MAKE_PACKED_VERSION(0, 1, 0)
#define HLR_NEXT_PATCH_VERSION HLR_LATEST_VERSION + HLR_MAKE_PACKED_VERSION(0, 0, 1)

#define HLR_INVALID_VERSION 0

// major 10 bits, minor 10 bits, patch 12 bits
#define HLR_GET_VERSION_MAJOR(version) ((version) >> 22)
#define HLR_GET_VERSION_MINOR(version) (((version) >> 12) & 0x3ff)
#define HLR_GET_VERSION_PATCH(version) ((version) & 0xfff)

HLR_CPP_NS_BEGIN

// Type for representing api versions.
// The api version is represented as packed integer value.
// major 10 bits, minor 10 bits, patch 12 bits
typedef unsigned int HlrVersion;

HLR_CPP_NS_END

#endif  // REMOTING_VERSION_H
