/**
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */
#ifdef __cplusplus
#ifdef _MSC_VER
#pragma warning(disable: 26812) // unscoped enums
#endif // _MSC_VER
#endif // __cplusplus

#ifndef REMOTING_INPUT_TYPES_H
#define REMOTING_INPUT_TYPES_H

#include "remoting/internal/HLR_API.h"

#include <assert.h>
#include <stdbool.h>
#include <stdint.h>
#include <math.h> // NaN: nanf("")

// TODO(viktor): move out of public header
#include <string.h> // memcmp, memmove, memset
#define ISAR_EQUAL_MEMORY(_destination, _source, _length) (!memcmp((_destination), (_source), (_length)))
#define ISAR_MOVE_MEMORY(_destination, _source, _length) memmove((_destination), (_source), (_length))
#define ISAR_COPY_MEMORY(_destination, _source, _length) memcpy((_destination), (_source), (_length))
#define ISAR_FILL_MEMORY(_destination, _length, _fill) memset((_destination), (_fill), (_length))
#define ISAR_FILL_MEMORY_TYPE(_type, _fill) memset(&(_type), (_fill), sizeof(_type))
#define ISAR_ZERO_MEMORY(_destination, _length) memset((_destination), 0, (_length))
#define ISAR_ZERO_MEMORY_TYPE(_type) memset(&(_type), 0, sizeof(_type))

HLR_CPP_NS_BEGIN

// NOTE(viktor): we don't do any conversions and just forward the platform layout
// NOTE(viktor): the naming doesn't matter since we just reinterpret_cast anyway without accessing the members directly
typedef struct HlrMatrix4x4 {
  // Format: column-major - unity / hlsl shaders
  float m00, m10, m20, m30;  // 16 bytes // x_axis[x, y, z, w]
  float m01, m11, m21, m31;  // 16 bytes // y_axis[x, y, z, w]
  float m02, m12, m22, m32;  // 16 bytes // z_axis[x, y, z, w]
  float m03, m13, m23, m33;  // 16 bytes //    pos[x, y, z, w]

  // Format: row-major - directx
  // float m00, m01, m02, m03;
  // float m10, m11, m12, m13;
  // float m20, m21, m22, m23;
  // float m30, m31, m32, m33;
} HlrMatrix4x4;  // 64 bytes

typedef struct HlrVector2 {
  float x, y;
} HlrVector2;

typedef struct HlrVector3 {
  float x, y, z;  // 12 bytes
} HlrVector3;     // 12 bytes

typedef struct HlrVector4 {
  float x, y, z, w;  // 16 bytes
} HlrVector4;        // 16 bytes
typedef HlrVector4 HlrQuaternion;
// Input Data Types

// TODO: instead of using 4 matices, which is huge, use forward, up & position
// like winrt::Windows::UI::Input::Spatial::SpatialPointerPose (timestamp)
// + winrt::Windows::Perception::People::HeadPose
// or
// + winrt::Windows::UI::Input::Spatial::SpatialPointerInteractionSourcePose via
//   SpatialPointerPose.TryGetInteractionSourcePose(source)
//   to also get the orientation quaternion
typedef struct HlrXrPose {
  // Frames rendered with this pose should be pushed with this timestamp value
  int64_t timestamp;  // 8 bytes

  // View matrices
  HlrMatrix4x4 view_left;   //  64 bytes
  HlrMatrix4x4 view_right;  //  64 bytes

  // Projection Matrices
  HlrMatrix4x4 proj_left;   //  64 bytes
  HlrMatrix4x4 proj_right;  //  64 bytes
} HlrXrPose;                // 256 bytes

// Input Event Data Types

typedef enum HlrInputEventType {
  // InteractionManager
  HlrInputEventType_SOURCE_DETECTED = 0,
  HlrInputEventType_SOURCE_LOST,
  HlrInputEventType_SOURCE_PRESSED,
  HlrInputEventType_SOURCE_UPDATED,
  HlrInputEventType_SOURCE_RELEASED,

  // GestureRecognizer
  HlrInputEventType_SELECTED,
  HlrInputEventType_TAPPED,
  HlrInputEventType_HOLD_STARTED,
  HlrInputEventType_HOLD_COMPLETED,
  HlrInputEventType_HOLD_CANCELED,
  HlrInputEventType_MANIPULATION_STARTED,
  HlrInputEventType_MANIPULATION_UPDATED,
  HlrInputEventType_MANIPULATION_COMPLETED,
  HlrInputEventType_MANIPULATION_CANCELED,
  HlrInputEventType_NAVIGATION_STARTED,
  HlrInputEventType_NAVIGATION_UPDATED,
  HlrInputEventType_NAVIGATION_COMPLETED,
  HlrInputEventType_NAVIGATION_CANCELED,

  HlrInputEventType_COUNT,
  HlrInputEventType_MIN = 0,
  HlrInputEventType_MAX = HlrInputEventType_COUNT - 1,
  HlrInputEventType_UNKNOWN = 0xFFFFFFFF,  // force sizeof int32
} HlrInputEventType;
static_assert(sizeof(HlrInputEventType) == sizeof(uint32_t), "enums need to be 32 bits, because bindings expect them to be");

inline char const* HlrInputEventType_to_str(HlrInputEventType type) {
  switch (type) {
    // InteractionManager
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_SOURCE_DETECTED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_SOURCE_LOST);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_SOURCE_PRESSED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_SOURCE_UPDATED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_SOURCE_RELEASED);

    // GestureRecognizer
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_SELECTED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_TAPPED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_HOLD_STARTED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_HOLD_COMPLETED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_HOLD_CANCELED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_MANIPULATION_STARTED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_MANIPULATION_UPDATED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_MANIPULATION_COMPLETED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_MANIPULATION_CANCELED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_NAVIGATION_STARTED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_NAVIGATION_UPDATED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_NAVIGATION_COMPLETED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_NAVIGATION_CANCELED);

    HLR_STRINGIFY_ENUM_CASE(HlrInputEventType_COUNT);
    default:
      // assert(false && "Unknown input event type!!!");
      return HLR_STRINGIFY(HlrInputEventType_UNKNOWN);
      // TODO(viktor): actually return the stringified number?
      // return itoa((int)type);
  }
}

typedef enum HlrInputType {
  // InteractionManager
  HlrInputType_SOURCE_DETECTED = 0,
  HlrInputType_SOURCE_LOST,
  HlrInputType_SOURCE_PRESSED,
  HlrInputType_SOURCE_UPDATED,
  HlrInputType_SOURCE_RELEASED,

  HlrInputType_COUNT,
  HlrInputType_MIN = 0,
  HlrInputType_MAX = HlrInputEventType_COUNT - 1,
  HlrInputType_UNKNOWN = 0xFFFFFFFF,  // force sizeof int32
} HlrInputType;
static_assert(sizeof(HlrInputType) == sizeof(uint32_t),
              "enums need to be 32 bits, because bindings expect them to be");

  inline char const* HlrSpatialInputType_to_str(HlrInputType type) {
  switch (type) {
    // InteractionManager
    HLR_STRINGIFY_ENUM_CASE(HlrInputType_SOURCE_DETECTED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputType_SOURCE_LOST);
    HLR_STRINGIFY_ENUM_CASE(HlrInputType_SOURCE_PRESSED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputType_SOURCE_UPDATED);
    HLR_STRINGIFY_ENUM_CASE(HlrInputType_SOURCE_RELEASED);

    HLR_STRINGIFY_ENUM_CASE(HlrInputType_COUNT);
    default:
      assert(false && "Unknown input event type!!!");
      return HLR_STRINGIFY(HlrInputType_UNKNOWN);
  }
}

#pragma region  // input data types

typedef enum HlrSpatialInteractionSourceKind {
  HlrSpatialInteractionSourceKind_OTHER = 0,
  HlrSpatialInteractionSourceKind_HAND = 1,
  HlrSpatialInteractionSourceKind_VOICE = 2,
  HlrSpatialInteractionSourceKind_CONTROLLER = 3,
} HlrSpatialInteractionSourceKind;
static_assert(sizeof(HlrSpatialInteractionSourceKind) == sizeof(int32_t),
              "enums need to be 32 bits, because bindings expect them to be");

typedef enum HlrSpatialInteractionSourceHandedness {
  HlrSpatialInteractionSourceHandedness_UNSPECIFIED = 0,
  HlrSpatialInteractionSourceHandedness_LEFT = 1,
  HlrSpatialInteractionSourceHandedness_RIGHT = 2,
} HlrSpatialInteractionSourceHandedness;
static_assert(sizeof(HlrSpatialInteractionSourceHandedness) == sizeof(int32_t),
              "enums need to be 32 bits, because bindings expect them to be");

typedef enum HlrSpatialInteractionSourceFlags {
  HlrSpatialInteractionSourceFlags_SUPPORTS_TOUCHPAD = 1 << 0,
  HlrSpatialInteractionSourceFlags_SUPPORTS_THUMBSTICK = 1 << 1,
  HlrSpatialInteractionSourceFlags_SUPPORTS_POINTING = 1 << 2,
  HlrSpatialInteractionSourceFlags_SUPPORTS_GRASP = 1 << 3,
  HlrSpatialInteractionSourceFlags_SUPPORTS_MENU = 1 << 4,
} HlrSpatialInteractionSourceFlags;
static_assert(sizeof(HlrSpatialInteractionSourceFlags) == sizeof(int32_t),
              "enums need to be 32 bits, because bindings expect them to be");

typedef enum HlrSpatialInteractionSourcePositionAccuracy {
  HlrInteractionSourcePositionAccuracy_HIGH = 0,
  HlrInteractionSourcePositionAccuracy_APPROXIMATE = 1,
} HlrSpatialInteractionSourcePositionAccuracy;
static_assert(sizeof(HlrSpatialInteractionSourcePositionAccuracy) ==
                  sizeof(int32_t),
              "enums need to be 32 bits, because bindings expect them to be");

typedef enum HlrSpatialInteractionSourcePoseFlags {
  HlrSpatialInteractionSourcePoseFlags_NONE = 0 << 0,
  HlrSpatialInteractionSourcePoseFlags_HAS_GRIP_POSITION = 1 << 0,
  HlrSpatialInteractionSourcePoseFlags_HAS_GRIP_ORIENTATION = 1 << 1,
  HlrSpatialInteractionSourcePoseFlags_HAS_POINTER_POSITION = 1 << 2,
  HlrSpatialInteractionSourcePoseFlags_HAS_POINTER_ORIENTATION = 1 << 3,
  HlrSpatialInteractionSourcePoseFlags_HAS_VELOCITY = 1 << 4,
  HlrSpatialInteractionSourcePoseFlags_HAS_ANGULAR_VELOCITY = 1 << 5,
} HlrSpatialInteractionSourcePoseFlags;
static_assert(sizeof(HlrSpatialInteractionSourcePoseFlags) == sizeof(int32_t),
              "enums need to be 32 bits, because bindings expect them to be");

#ifdef __cplusplus

inline HlrSpatialInteractionSourcePoseFlags& operator|= (
  HlrSpatialInteractionSourcePoseFlags& lhs, HlrSpatialInteractionSourcePoseFlags rhs)
{
  reinterpret_cast<uint32_t&>(lhs) |= static_cast<uint32_t>(rhs);
  return lhs;
}

#endif

typedef struct HlrSpatialInteractionSourcePose {
  HlrQuaternion gripOrientation;                                 // 16 bytes
  HlrQuaternion pointerOrientation;                              // 16 bytes
  HlrVector3 gripPosition;                                       // 12 bytes
  HlrVector3 pointerPosition;                                    // 12 bytes
  HlrVector3 velocity;                                           // 12 bytes
  HlrVector3 angularVelocity;                                    // 12 bytes
  HlrSpatialInteractionSourcePositionAccuracy positionAccuracy;  //  4 bytes
  HlrSpatialInteractionSourcePoseFlags flags;                    //  4 bytes
} HlrSpatialInteractionSourcePose;                               // 88 bytes

typedef struct HlrSpatialInteractionSourceProperties {
  double sourceLossRisk;                       //   8 bytes
  HlrVector3 sourceLossMitigationDirection;    //  12 bytes
  HlrSpatialInteractionSourcePose sourcePose;  //  88 bytes
  // char _padding[4];                         //   4 bytes
} HlrSpatialInteractionSourceProperties;  // 112 bytes

inline HlrSpatialInteractionSourceProperties HlrSpatialInteractionSourceProperties_default() {
  HlrSpatialInteractionSourceProperties result;
  result.sourceLossMitigationDirection.x = nanf("");
  return result;
}

typedef struct HlrSpatialInteractionSource {
  uint32_t id;                                       //  4 bytes
  HlrSpatialInteractionSourceKind kind;              //  4 bytes
  HlrSpatialInteractionSourceHandedness handedness;  //  4 bytes
  HlrSpatialInteractionSourceFlags flags;            //  4 bytes
} HlrSpatialInteractionSource;                       // 16 bytes

typedef struct HlrPerceptionTimestamp {
  // long long predictionAmount;
  // long long targetTime;
  int64_t systemRelativeTargetTime;  //  8 bytes
} HlrPerceptionTimestamp;            //  8 bytes

typedef struct HlrPose {
  HlrVector3 position;        // 12 bytes
  HlrQuaternion orientation;  // 16 bytes
} HlrPose;                    // 28 bytes

// TODO(viktor): convert forward & up to a quaternion and use HlrPose
typedef struct HlrHeadPose {
  HlrVector3 position;          // 12 bytes
  HlrVector3 forwardDirection;  // 12 bytes
  HlrVector3 upDirection;       // 12 bytes
} HlrHeadPose;                  // 36 bytes

typedef enum HlrHandJointKind /*: int32_t*/ {
  HlrHandJointKind_PALM = 0,
  HlrHandJointKind_WRIST = 1,
  HlrHandJointKind_THUMB_METACARPAL = 2,
  HlrHandJointKind_THUMB_PROXIMAL = 3,
  HlrHandJointKind_THUMB_DISTAL = 4,
  HlrHandJointKind_THUMB_TIP = 5,
  HlrHandJointKind_INDEX_METACARPAL = 6,
  HlrHandJointKind_INDEX_PROXIMAL = 7,
  HlrHandJointKind_INDEX_INTERMEDIATE = 8,
  HlrHandJointKind_INDEX_DISTAL = 9,
  HlrHandJointKind_INDEX_TIP = 10,
  HlrHandJointKind_MIDDLE_METACARPAL = 11,
  HlrHandJointKind_MIDDLE_PROXIMAL = 12,
  HlrHandJointKind_MIDDLE_INTERMEDIATE = 13,
  HlrHandJointKind_MIDDLE_DISTAL = 14,
  HlrHandJointKind_MIDDLE_TIP = 15,
  HlrHandJointKind_RING_METACARPAL = 16,
  HlrHandJointKind_RING_PROXIMAL = 17,
  HlrHandJointKind_RING_INTERMEDIATE = 18,
  HlrHandJointKind_RING_DISTAL = 19,
  HlrHandJointKind_RING_TIP = 20,
  HlrHandJointKind_LITTLE_METACARPAL = 21,
  HlrHandJointKind_LITTLE_PROXIMAL = 22,
  HlrHandJointKind_LITTLE_INTERMEDIATE = 23,
  HlrHandJointKind_LITTLE_DISTAL = 24,
  HlrHandJointKind_LITTLE_TIP = 25,
  HlrHandJointKind_COUNT
} HlrHandJointKind;

typedef enum HlrXRControllerFeatureKind
{
  // Hands
  HlrXRControllerFeatureKind_HAND_PALM = 0,
  HlrXRControllerFeatureKind_HAND_WRIST = 1,
  HlrXRControllerFeatureKind_HAND_THUMB_METACARPAL = 2,
  HlrXRControllerFeatureKind_HAND_THUMB_PROXIMAL = 3,
  HlrXRControllerFeatureKind_HAND_THUMB_DISTAL = 4,
  HlrXRControllerFeatureKind_HAND_THUMB_TIP = 5,
  HlrXRControllerFeatureKind_HAND_INDEX_METACARPAL = 6,
  HlrXRControllerFeatureKind_HAND_INDEX_PROXIMAL = 7,
  HlrXRControllerFeatureKind_HAND_INDEX_INTERMEDIATE = 8,
  HlrXRControllerFeatureKind_HAND_INDEX_DISTAL = 9,
  HlrXRControllerFeatureKind_HAND_INDEX_TIP = 10,
  HlrXRControllerFeatureKind_HAND_MIDDLE_METACARPAL = 11,
  HlrXRControllerFeatureKind_HAND_MIDDLE_PROXIMAL = 12,
  HlrXRControllerFeatureKind_HAND_MIDDLE_INTERMEDIATE = 13,
  HlrXRControllerFeatureKind_HAND_MIDDLE_DISTAL = 14,
  HlrXRControllerFeatureKind_HAND_MIDDLE_TIP = 15,
  HlrXRControllerFeatureKind_HAND_RING_METACARPAL = 16,
  HlrXRControllerFeatureKind_HAND_RING_PROXIMAL = 17,
  HlrXRControllerFeatureKind_HAND_RING_INTERMEDIATE = 18,
  HlrXRControllerFeatureKind_HAND_RING_DISTAL = 19,
  HlrXRControllerFeatureKind_HAND_RING_TIP = 20,
  HlrXRControllerFeatureKind_HAND_LITTLE_METACARPAL = 21,
  HlrXRControllerFeatureKind_HAND_LITTLE_PROXIMAL = 22,
  HlrXRControllerFeatureKind_HAND_LITTLE_INTERMEDIATE = 23,
  HlrXRControllerFeatureKind_HAND_LITTLE_DISTAL = 24,
  HlrXRControllerFeatureKind_HAND_LITTLE_TIP = 25,

  // Buttons
  HlrXRControllerFeatureKind_BUTTON_HOME = 26,
  HlrXRControllerFeatureKind_BUTTON_MENU = 27,
  HlrXRControllerFeatureKind_BUTTON_SETTINGS = 28,
  HlrXRControllerFeatureKind_BUTTON_A = 29,
  HlrXRControllerFeatureKind_BUTTON_B = 30,
  HlrXRControllerFeatureKind_BUTTON_X = 31,
  HlrXRControllerFeatureKind_BUTTON_Y = 32,
  HlrXRControllerFeatureKind_BUTTON_PRIMARY_BUMPER = 33,
  HlrXRControllerFeatureKind_BUTTON_SECONDARY_BUMPER = 34,
  HlrXRControllerFeatureKind_BUTTON_PRIMARY_ANALOG_STICK_PRESS = 35,
  HlrXRControllerFeatureKind_BUTTON_SECONDARY_ANALOG_STICK_PRESS = 36,
  HlrXRControllerFeatureKind_BUTTON_PRIMARY_THUMB_REST = 37,
  HlrXRControllerFeatureKind_BUTTON_SECONDARY_THUMB_REST = 38,
  HlrXRControllerFeatureKind_BUTTON_PRIMARY_TRIGGER_PRESS = 39,
  HlrXRControllerFeatureKind_BUTTON_SECONDARY_TRIGGER_PRESS = 40,
  HlrXRControllerFeatureKind_BUTTON_PRIMARY_SQUEEZE_PRESS = 41,
  HlrXRControllerFeatureKind_BUTTON_SECONDARY_SQUEEZE_PRESS = 42,
  // Axis1D
  HlrXRControllerFeatureKind_AXIS1D_PRIMARY_TRIGGER = 43,
  HlrXRControllerFeatureKind_AXIS1D_SECONDARY_TRIGGER = 44,
  HlrXRControllerFeatureKind_AXIS1D_PRIMARY_SQUEEZE = 45,
  HlrXRControllerFeatureKind_AXIS1D_SECONDARY_SQUEEZE = 46,

  // Axis2D
  HlrXRControllerFeatureKind_AXIS2D_PRIMARY_ANALOG_STICK = 47,
  HlrXRControllerFeatureKind_AXIS2D_SECONDARY_ANALOG_STICK = 48,

} HlrXRControllerFeatureKind;

typedef enum HlrXRControllerType
{
  HlrXRControllerType_HoloLens_Hands = 0,
  HlrXRControllerType_Oculus_Quest_2_Hands = 1,
  HlrXRControllerType_Oculus_Quest_2_Controller = 2,
  // Extend for more controller types (Vive, SteamVR, WMR Controller, whatever...)
} HlrXRControllerType;

typedef enum HlrJointPoseAccuracy /*: int32_t*/ {
  HlrJointPoseAccuracy_HIGH = 0,
  HlrJointPoseAccuracy_APPROXIMATE = 1,
} HlrJointPoseAccuracy;

// TODO(viktor): rename members to small letters
typedef struct HlrJointPose {
  HlrQuaternion Orientation;      // 16 bytes
  HlrVector3 Position;            // 12 bytes
  float Radius;                   //  4 bytes
  // TODO: encode Accuracy (0/1) into radius sign bit (can't have negative radius)
  HlrJointPoseAccuracy Accuracy;  //  4 bytes
} HlrJointPose;                   // 36 bytes

// TODO: save 104 bytes: pack Accuracy into Radius
typedef struct HlrHandPose {
  HlrJointPose jointPoses[HlrHandJointKind_COUNT];  // 936 bytes
} HlrHandPose;                                      // 1040 bytes

// padding needed to align the size to 8 bytes (64 bits)
typedef struct HlrSpatialPointerPose {
  HlrPerceptionTimestamp timestamp;  //   8 bytes
  HlrHeadPose head;                  //  36 bytes
  HlrHandPose hand;                  // 200 bytes
  // uint32_t _padding;              //   4 bytes
} HlrSpatialPointerPose;  // 248 bytes

typedef enum HlrSpatialInteractionSourceStateFlags {
  HlrInteractionSourceStateFlags_NONE = 0 << 0,
  HlrInteractionSourceStateFlags_GRASPED = 1 << 0,
  HlrInteractionSourceStateFlags_ANY_PRESSED = 1 << 1,
  HlrInteractionSourceStateFlags_TOUCHPAD_PRESSED = 1 << 2,
  HlrInteractionSourceStateFlags_THUMBSTICK_PRESSED = 1 << 3,
  HlrInteractionSourceStateFlags_SELECT_PRESSED = 1 << 4,
  HlrInteractionSourceStateFlags_MENU_PRESSED = 1 << 5,
  HlrInteractionSourceStateFlags_TOUCHPAD_TOUCHED = 1 << 6,
} HlrSpatialInteractionSourceStateFlags;
static_assert(sizeof(HlrSpatialInteractionSourceStateFlags) == sizeof(uint32_t),
              "enums need to be 32 bits, because bindings expect them to be");

typedef struct HlrSpatialInteractionSourceState {
  HlrSpatialInteractionSourceProperties properties;  //  112 bytes
  HlrSpatialInteractionSource source;                //   16 bytes
  HlrHeadPose headPose;                              //   36 bytes
  HlrHandPose handPose;                              // 1040 bytes
  // HlrVector2 thumbstickPosition;                  //    8 bytes
  // HlrVector2 touchpadPosition;                    //    8 bytes
  float selectPressedAmount;                         //    4 bytes
  HlrSpatialInteractionSourceStateFlags flags;       //    4 bytes
  // TODO: since we have padding here
  // TODO: we could make selectPressedAmount a double afterall (like the original type)
  // char _padding[4];                              //     4 bytes
} HlrSpatialInteractionSourceState;  // 1216 bytes

#pragma region HlrInputEventData types

typedef struct HlrInputEventDataSpatialInteractionSourceDetected {
  // HlrSpatialInteractionSource interactionSource;  // 48 bytes
  HlrSpatialInteractionSourceState interactionSourceState;  // 176 bytes
} HlrInputEventDataSpatialInteractionSourceDetected;        // 176 bytes

typedef struct HlrInputEventDataSpatialInteractionSourceLost {
  HlrSpatialInteractionSourceState interactionSourceState;  // 176 bytes
} HlrInputEventDataSpatialInteractionSourceLost;            // 16 bytes

typedef struct HlrInputEventDataSpatialInteractionSourcePressed {
  HlrSpatialInteractionSourceState interactionSourceState;  // 176 bytes
} HlrInputEventDataSpatialInteractionSourcePressed;         // 16 bytes

typedef struct HlrInputEventDataSpatialInteractionSourceUpdated {
  HlrSpatialInteractionSourceState interactionSourceState;  // 176 bytes
} HlrInputEventDataSpatialInteractionSourceUpdated;         // 16 bytes

typedef struct HlrInputEventDataSpatialInteractionSourceReleased {
  HlrSpatialInteractionSourceState interactionSourceState;  // 176 bytes
} HlrInputEventDataSpatialInteractionSourceReleased;        // 16 bytes

// padding to align the size to 8 bytes (64 bits)
typedef struct HlrInputEventDataTapped {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
  uint32_t tapCount;                           //  4 bytes
  // uint32_t _padding;                           //  4 bytes
} HlrInputEventDataTapped;  // 56 bytes

typedef struct HlrInputEventDataHoldStarted {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
} HlrInputEventDataHoldStarted;                // 56 bytes

typedef struct HlrInputEventDataHoldCompleted {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
} HlrInputEventDataHoldCompleted;              //  4 bytes

typedef struct HlrInputEventDataHoldCanceled {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
} HlrInputEventDataHoldCanceled;               //  4 bytes

typedef struct HlrInputEventDataManipulationStarted {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
} HlrInputEventDataManipulationStarted;        // 52 bytes

typedef struct HlrSpatialManipulationDelta {
  HlrVector3 translation;       // 12 bytes
} HlrSpatialManipulationDelta;  // 12 bytes

typedef struct HlrInputEventDataManipulationUpdated {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
  HlrSpatialManipulationDelta delta;           // 12 bytes
} HlrInputEventDataManipulationUpdated;        // 16 bytes

typedef struct HlrInputEventDataManipulationCompleted {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
  HlrSpatialManipulationDelta delta;           // 12 bytes
} HlrInputEventDataManipulationCompleted;      // 16 bytes

typedef struct HlrInputEventDataManipulationCanceled {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
} HlrInputEventDataManipulationCanceled;       //  4 bytes

typedef enum HlrNavigationFlags {
  HlrNavigationFlags_NONE = 0,
  HlrNavigationFlags_IS_ON_RAILS = 1,
  HlrNavigationFlags_IS_NAV_X = 2,
  HlrNavigationFlags_IS_NAV_Y = 4,
  HlrNavigationFlags_IS_NAV_Z = 8,
} HlrNavigationFlags;

typedef struct HlrInputEventDataNavigationStarted {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
  uint32_t railsFlags;                         // of type HlrNavigationFlags
} HlrInputEventDataNavigationStarted;          // 52 bytes

typedef struct HlrInputEventDataNavigationUpdated {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
  uint32_t railsFlags;                         // of type HlrNavigationFlags
  HlrVector3 normalizedOffset;                 // 12 bytes
} HlrInputEventDataNavigationUpdated;          // 16 bytes

typedef struct HlrInputEventDataNavigationCompleted {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
  uint32_t railsFlags;                         // of type HlrNavigationFlags
  HlrVector3 normalizedOffset;                 // 12 bytes
} HlrInputEventDataNavigationCompleted;        // 16 bytes

typedef struct HlrInputEventDataNavigationCanceled {
  HlrSpatialInteractionSource source;          //  4 bytes
  HlrSpatialInteractionSourcePose sourcePose;  // 48 bytes
  HlrHeadPose headPose;                        // 48 bytes
  uint32_t railsFlags;                         // of type HlrNavigationFlags
} HlrInputEventDataNavigationCanceled;         //  4 bytes

#pragma endregion  // HlrInputEventData types

#pragma endregion  // input data types

typedef union HlrInputEventData {
  /// InteractionManager events
  HlrInputEventDataSpatialInteractionSourceDetected sourceDetected; // 176 bytes
  HlrInputEventDataSpatialInteractionSourceLost sourceLost;         //  16 bytes
  HlrInputEventDataSpatialInteractionSourcePressed sourcePressed;   //  16 bytes
  HlrInputEventDataSpatialInteractionSourceUpdated sourceUpdated;   //  16 bytes
  HlrInputEventDataSpatialInteractionSourceReleased sourceReleased; //  16 bytes

  /// GestureRecognizer events
  // HlrInputData_Selected selected;
  HlrInputEventDataTapped tapped;  //  56 bytes
  // Hold
  HlrInputEventDataHoldStarted holdStarted;      //  44 bytes
  HlrInputEventDataHoldCompleted holdCompleted;  //   4 bytes
  HlrInputEventDataHoldCanceled holdCanceled;    //   4 bytes
  // Manipulation
  HlrInputEventDataManipulationStarted manipulationStarted;      //   4 bytes
  HlrInputEventDataManipulationUpdated manipulationUpdated;      //   4 bytes
  HlrInputEventDataManipulationCompleted manipulationCompleted;  //   4 bytes
  HlrInputEventDataManipulationCanceled manipulationCanceled;    //   4 bytes
  // Navigation
  HlrInputEventDataNavigationStarted navigationStarted;      //   4 bytes
  HlrInputEventDataNavigationUpdated navigationUpdated;      //   4 bytes
  HlrInputEventDataNavigationCompleted navigationCompleted;  //   4 bytes
  HlrInputEventDataNavigationCanceled navigationCanceled;    //   4 bytes
} HlrInputEventData;                                         // 176 bytes

typedef struct HlrInputEvent {
  HlrInputEventType type;  //   4 bytes
  // char _padding[4];    //   4 bytes
  HlrInputEventData data;  // 176 bytes
} HlrInputEvent;           // 184 bytes
// TODO: these static_asserts are only here because we create the bindings by hand
// and need a reminder to update the bindings accordingly
// but be aware that the size alone doesn't necessarily reflect layout changes
// and this does not ensure compatibility between bindings changes
static_assert(
  sizeof(HlrInputEvent) == 1120,
  "the data layout of " HLR_STRINGIFY(HlrInputEvent)
  " was altered which changed it's size, "
  // HLR_STRINGIFY_DELAYED(sizeof(HlrInputEvent)) " "
  "this could affect foreign function interfaces!");

#pragma region  // HlrInputEvent constructors/factories

// TODO(viktor): get rid of these factory functions
inline HlrInputEvent HlrInputEvent_create() {
  HlrInputEvent input;
#ifndef NDEBUG
  // input._padding = 0xDEADBEEF;
  // input._padding = 0x1BADCAFE;
  // input._padding = 0xCDCDCDCD; // visual studio
  ISAR_FILL_MEMORY_TYPE(input, 0xCDCDCDCD);
  // memset(&(input), 0xCDCDCDCD, sizeof(input));
#else
  ISAR_ZERO_MEMORY_TYPE(input);
  // memset(&(input), 0, sizeof(input));
#endif
  return input;
}

typedef struct HlrButton{
  uint32_t identifier;
  bool value;
}HlrButton;

typedef struct HlrAxis1D{
  uint32_t identifier;
  float value;
}HlrAxis1D;

typedef struct HlrAxis2D{
  uint32_t identifier;
  HlrVector2 value;
}HlrAxis2D;

typedef enum HlrButtonType{
  HlrButtonKind_home = 0,
  HlrButtonKind_menu = 1,
  HlrButtonKind_settings = 2,
  HlrButtonKind_a = 3,
  HlrButtonKind_b = 4,
  HlrButtonKind_x = 5,
  HlrButtonKind_y = 6,
  HlrButtonKind_bumperLeft = 7,
  HlrButtonKind_bumperRight = 8,
  HlrButtonKind_leftStickPress = 9,
  HlrButtonKind_rightStickPress = 10,
  HlrAxis2DKind_primaryThumbRest = 11,
  HlrAxis2DKind_secondaryThumbRest = 12,
  HlrButtonKind_COUNT
} HlrButtonType;

typedef enum HlrAxis1DType{
  HlrAxis1DKind_primaryTrigger = 0,
  HlrAxis1DKind_secondaryTrigger = 1,
  HlrAxis1DKind_primarySqueeze = 2,
  HlrAxis1DKind_secondarySqueeze = 3,
  HlrAxis1DKind_COUNT
} HlrAxis1DType;

typedef enum HlrAxis2DType
{
  HlrAxis2DKind_primaryStick = 0,
  HlrAxis2DKind_secondaryStick = 1,
  HlrAxis2DKind_primaryControlPad = 2,
  HlrAxis2DKind_secondaryControlPad = 3,
  HlrAxis2DKind_COUNT
} HlrAxis2DType;

typedef struct HlrControllerData{
  uint32_t controllerIdentifier;
  HlrSpatialInteractionSourceHandedness handedness;
  HlrHeadPose headPose;
  HlrPose controllerPose;
  HlrPose pointerPose;

  HlrHandPose handData;
  HlrButton* buttons;
  uint32_t buttonsLength;
  HlrAxis1D* axis1D;
  uint32_t axis1DLength;
  HlrAxis2D* axis2D;
  uint32_t axis2DLength;

  // Variable sized data...
  //HlrButton buttons[HlrAxis2DKind_COUNT]; // home, menu, settings, x,y,a,b, bumper left, bumper right, left stick pressed, right stick pressed, = length 12
  //HlrAxis1D axis1D[HlrAxis2DKind_COUNT]; // trigger left, trigger right, squeeze left, squeeze right = length 4
  //HlrAxis2D axis2D[HlrAxis2DKind_COUNT]; // primary stick, secondary stick, Touch.PrimaryThumbRest, Touch.SecondaryThumbRest = length 4
  // Different input variants
  //handJoints:HandJoints; // contains all the joint data

} HlrControllerData;

typedef struct HlrInteractionSourceState {
  HlrControllerData controllerData; // TODO: inline the content of this
} HlrInteractionSourceState;  // 1216 bytes

typedef struct HlrSpatialInputDataInteractionSourceDetected {
  // HlrSpatialInteractionSource interactionSource;  // 48 bytes
  HlrInteractionSourceState interactionSourceState;  // 176 bytes
} HlrSpatialInputDataInteractionSourceDetected;        // 176 bytes

typedef struct HlrSpatialInputDataInteractionSourceLost {
  HlrInteractionSourceState interactionSourceState;  // 176 bytes
} HlrSpatialInputDataInteractionSourceLost;            // 16 bytes

typedef struct HlrSpatialInputDataInteractionSourcePressed {
  HlrInteractionSourceState interactionSourceState;  // 176 bytes
} HlrSpatialInputDataInteractionSourcePressed;         // 16 bytes

typedef struct HlrSpatialInputDataInteractionSourceUpdated {
  HlrInteractionSourceState interactionSourceState;  // 176 bytes
} HlrSpatialInputDataInteractionSourceUpdated;         // 16 bytes

typedef struct HlrSpatialInputDataInteractionSourceReleased {
  HlrInteractionSourceState interactionSourceState;  // 176 bytes
} HlrSpatialInputDataInteractionSourceReleased;        // 16 bytes

typedef union HlrSpatialInputData {
  /// InteractionManager events
  HlrSpatialInputDataInteractionSourceDetected sourceDetected; // 176 bytes
  HlrSpatialInputDataInteractionSourceLost sourceLost;         //  16 bytes
  HlrSpatialInputDataInteractionSourcePressed sourcePressed;   //  16 bytes
  HlrSpatialInputDataInteractionSourceUpdated sourceUpdated;   //  16 bytes
  HlrSpatialInputDataInteractionSourceReleased sourceReleased;

} HlrSpatialInputData;

typedef struct HlrSpatialInput {
  HlrInputType type;  //   4 bytes
  // char _padding[4];    //   4 bytes
  HlrSpatialInputData data;  // 176 bytes
} HlrSpatialInput;


inline HlrSpatialInput HlrSpatialInput_create() {
  HlrSpatialInput spatialInput;
#ifndef NDEBUG
  // spatialInput._padding = 0xDEADBEEF;
  // spatialInput._padding = 0x1BADCAFE;
  // spatialInput._padding = 0xCDCDCDCD; // visual studio
  ISAR_FILL_MEMORY_TYPE(spatialInput, 0xCDCDCDCD);
  // memset(&(spatialInput), 0xCDCDCDCD, sizeof(spatialInput));
#else
  ISAR_ZERO_MEMORY_TYPE(spatialInput);
  // memset(&(spatialInput), 0, sizeof(spatialInput));
#endif

  return spatialInput;
}

#pragma region  // InteractionManager events

inline HlrInputEvent HlrInputEvent_createSourceDetected(
    HlrSpatialInteractionSourceState interactionSourceState) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_SOURCE_DETECTED;
  result.data.sourceDetected.interactionSourceState = interactionSourceState;
  return result;
}

inline HlrInputEvent HlrInputEvent_createSourceLost(
    HlrSpatialInteractionSourceState interactionSourceState) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_SOURCE_LOST;
  result.data.sourceDetected.interactionSourceState = interactionSourceState;
  return result;
}

inline HlrInputEvent HlrInputEvent_createSourcePressed(
    HlrSpatialInteractionSourceState interactionSourceState) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_SOURCE_PRESSED;
  result.data.sourceDetected.interactionSourceState = interactionSourceState;
  return result;
}

inline HlrInputEvent HlrInputEvent_createSourceUpdated(
    HlrSpatialInteractionSourceState interactionSourceState) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_SOURCE_UPDATED;
  result.data.sourceDetected.interactionSourceState = interactionSourceState;
  return result;
}

inline HlrInputEvent HlrInputEvent_createSourceReleased(
    HlrSpatialInteractionSourceState interactionSourceState) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_SOURCE_RELEASED;
  result.data.sourceDetected.interactionSourceState = interactionSourceState;
  return result;
}

inline HlrSpatialInput HlrSpatialInput_createSourceDetected(
    HlrInteractionSourceState interactionSourceState) {
  HlrSpatialInput result = HlrSpatialInput_create();

  result.type = HlrInputType_SOURCE_DETECTED;
  result.data.sourceDetected.interactionSourceState = interactionSourceState;
  return result;
}

inline HlrSpatialInput HlrSpatialInput_createSourceLost(
    HlrInteractionSourceState interactionSourceState) {
  HlrSpatialInput result = HlrSpatialInput_create();

  result.type = HlrInputType_SOURCE_LOST;
  result.data.sourceDetected.interactionSourceState = interactionSourceState;
  return result;
}

inline HlrSpatialInput HlrSpatialInput_createSourcePressed(
    HlrInteractionSourceState interactionSourceState) {
  HlrSpatialInput result = HlrSpatialInput_create();

  result.type = HlrInputType_SOURCE_PRESSED;
  result.data.sourceDetected.interactionSourceState = interactionSourceState;
  return result;
}

inline HlrSpatialInput HlrSpatialInput_createSourceUpdated(
    HlrInteractionSourceState interactionSourceState) {
  HlrSpatialInput result = HlrSpatialInput_create();

  result.type = HlrInputType_SOURCE_UPDATED;
  result.data.sourceDetected.interactionSourceState = interactionSourceState;
  return result;
}

inline HlrSpatialInput HlrSpatialInput_createSourceReleased(
    HlrInteractionSourceState interactionSourceState) {
  HlrSpatialInput result = HlrSpatialInput_create();

  result.type = HlrInputType_SOURCE_RELEASED;
  result.data.sourceDetected.interactionSourceState = interactionSourceState;
  return result;
}

#pragma endregion  // InteractionManager events

#pragma region  // GestureRecognizer events

inline HlrInputEvent HlrInputEvent_createSelected() {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_SELECTED;
  return result;
}

inline HlrInputEvent
HlrInputEvent_createTapped(HlrSpatialInteractionSource source,
                           HlrSpatialInteractionSourcePose sourcePose,
                           HlrHeadPose headPose,
                           uint32_t tapCount) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_TAPPED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  result.data.tapped.tapCount = tapCount;
  return result;
}

inline HlrInputEvent
HlrInputEvent_createHoldStarted(HlrSpatialInteractionSource source,
                                HlrSpatialInteractionSourcePose sourcePose,
                                HlrHeadPose headPose) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_HOLD_STARTED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  return result;
}

inline HlrInputEvent
HlrInputEvent_createHoldCompleted(HlrSpatialInteractionSource source,
                                  HlrSpatialInteractionSourcePose sourcePose,
                                  HlrHeadPose headPose) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_HOLD_COMPLETED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  return result;
}

inline HlrInputEvent
HlrInputEvent_createHoldCanceled(HlrSpatialInteractionSource source,
                                 HlrSpatialInteractionSourcePose sourcePose,
                                 HlrHeadPose headPose) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_HOLD_CANCELED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  return result;
}

inline HlrInputEvent HlrInputEvent_createManipulationStarted(
    HlrSpatialInteractionSource source,
    HlrSpatialInteractionSourcePose sourcePose,
    HlrHeadPose headPose) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_MANIPULATION_STARTED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  return result;
}

inline HlrInputEvent HlrInputEvent_createManipulationUpdated(
    HlrSpatialInteractionSource source,
    HlrSpatialInteractionSourcePose sourcePose,
    HlrHeadPose headPose,
    HlrSpatialManipulationDelta delta) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_MANIPULATION_UPDATED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  result.data.manipulationUpdated.delta = delta;
  return result;
}

inline HlrInputEvent HlrInputEvent_createManipulationCompleted(
    HlrSpatialInteractionSource source,
    HlrSpatialInteractionSourcePose sourcePose,
    HlrHeadPose headPose,
    HlrSpatialManipulationDelta delta) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_MANIPULATION_COMPLETED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  result.data.manipulationCompleted.delta = delta;
  return result;
}

inline HlrInputEvent HlrInputEvent_createManipulationCanceled(
    HlrSpatialInteractionSource source,
    HlrSpatialInteractionSourcePose sourcePose,
    HlrHeadPose headPose) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_MANIPULATION_CANCELED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  return result;
}

inline HlrInputEvent HlrInputEvent_createNavigationStarted(
    HlrSpatialInteractionSource source,
    HlrSpatialInteractionSourcePose sourcePose,
    HlrHeadPose headPose,
    uint32_t railsFlags) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_NAVIGATION_STARTED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  result.data.navigationStarted.railsFlags = railsFlags;
  return result;
}

inline HlrInputEvent HlrInputEvent_createNavigationUpdated(
    HlrSpatialInteractionSource source,
    HlrSpatialInteractionSourcePose sourcePose,
    HlrHeadPose headPose,
    uint32_t railsFlags,
    HlrVector3 normalizedOffset) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_NAVIGATION_UPDATED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  result.data.navigationUpdated.railsFlags = railsFlags;
  result.data.navigationUpdated.normalizedOffset = normalizedOffset;
  return result;
}

inline HlrInputEvent HlrInputEvent_createNavigationCompleted(
    HlrSpatialInteractionSource source,
    HlrSpatialInteractionSourcePose sourcePose,
    HlrHeadPose headPose,
    uint32_t railsFlags,
    HlrVector3 normalizedOffset) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_NAVIGATION_COMPLETED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  result.data.navigationCompleted.railsFlags = railsFlags;
  result.data.navigationCompleted.normalizedOffset = normalizedOffset;
  return result;
}

inline HlrInputEvent HlrInputEvent_createNavigationCanceled(
    HlrSpatialInteractionSource source,
    HlrSpatialInteractionSourcePose sourcePose,
    HlrHeadPose headPose,
    uint32_t railsFlags) {
  HlrInputEvent result = HlrInputEvent_create();

  result.type = HlrInputEventType_NAVIGATION_CANCELED;
  result.data.tapped.source = source;
  result.data.tapped.sourcePose = sourcePose;
  result.data.tapped.headPose = headPose;
  result.data.navigationCanceled.railsFlags = railsFlags;
  return result;
}

#pragma endregion  // GestureRecognizer events

#pragma endregion  // HlrInputEvent constructors/factories

#pragma region // HlrAnchorMessage

typedef enum HlrAnchorTrackingState
{
	NONE = 0,
	LIMITED,
	TRACKING

} HlrAnchorTrackingState;

typedef struct HlrAnchorId{
  uint64_t sub_id_first;
  uint64_t sub_id_second;
} HlrAnchorId;

typedef struct  HlrAnchor{
    HlrAnchorId id;
    HlrPose pose;
    HlrAnchorTrackingState tracking_state;
} HlrAnchor;

#pragma endregion // HlrAnchorMessage


#pragma region // HlrQrMessage

typedef enum HlrQrAccessStatus /*: int32_t*/ {
  HlrQrAccessStatus_DeniedBySystem = 0,
  HlrQrAccessStatus_NotDeclaredByApp = 1,
  HlrQrAccessStatus_DeniedByUser = 2,
  HlrQrAccessStatus_UserPromptRequired = 3,
  HlrQrAccessStatus_Allowed = 4,
  HlrQrAccessStatus__INT32 = 0xFFFFFFFF,
} HlrQrAccessStatus;

typedef struct HlrGuid {
  uint32_t data1;
  uint16_t data2;
  uint16_t data3;
  uint8_t  data4[8];
} HlrGuid;

// ref: winrt::Microsoft::MixedReality::QR::QRVersion
typedef enum HlrQrVersion /*: int32_t*/ {
  HlrQrVersion_INVALID = 0,
  HlrQrVersion_QR1 = 1,
  HlrQrVersion_QR2 = 2,
  HlrQrVersion_QR3 = 3,
  HlrQrVersion_QR4 = 4,
  HlrQrVersion_QR5 = 5,
  HlrQrVersion_QR6 = 6,
  HlrQrVersion_QR7 = 7,
  HlrQrVersion_QR8 = 8,
  HlrQrVersion_QR9 = 9,
  HlrQrVersion_QR10 = 10,
  HlrQrVersion_QR11 = 11,
  HlrQrVersion_QR12 = 12,
  HlrQrVersion_QR13 = 13,
  HlrQrVersion_QR14 = 14,
  HlrQrVersion_QR15 = 15,
  HlrQrVersion_QR16 = 16,
  HlrQrVersion_QR17 = 17,
  HlrQrVersion_QR18 = 18,
  HlrQrVersion_QR19 = 19,
  HlrQrVersion_QR20 = 20,
  HlrQrVersion_QR21 = 21,
  HlrQrVersion_QR22 = 22,
  HlrQrVersion_QR23 = 23,
  HlrQrVersion_QR24 = 24,
  HlrQrVersion_QR25 = 25,
  HlrQrVersion_QR26 = 26,
  HlrQrVersion_QR27 = 27,
  HlrQrVersion_QR28 = 28,
  HlrQrVersion_QR29 = 29,
  HlrQrVersion_QR30 = 30,
  HlrQrVersion_QR31 = 31,
  HlrQrVersion_QR32 = 32,
  HlrQrVersion_QR33 = 33,
  HlrQrVersion_QR34 = 34,
  HlrQrVersion_QR35 = 35,
  HlrQrVersion_QR36 = 36,
  HlrQrVersion_QR37 = 37,
  HlrQrVersion_QR38 = 38,
  HlrQrVersion_QR39 = 39,
  HlrQrVersion_QR40 = 40,
  HlrQrVersion_MICRO_QRM1 = 41,
  HlrQrVersion_MICRO_QRM2 = 42,
  HlrQrVersion_MICRO_QRM3 = 43,
  HlrQrVersion_MICRO_QRM4 = 44,
  HlrQrVersion__INT32 = 0xFFFFFFFF // TODO(viktor): instead of this we could do union HlrQrVersion { enum Enum {}; int32_t _padding;}
} HLrQrVersion;

typedef struct HlrQrCode {
  HlrGuid id;
  //HlrGuid spatialGraphNodeId;
  int64_t timestamp;
  int64_t sysTimestamp;
  float physicalSideLength;
  HLrQrVersion version;
  uint32_t dataSize;
  uint8_t* data;
  HlrPose pose;
} HlrQrCode;

inline HlrQrCode HlrQrCode_default(void) {
  HlrQrCode result;
  ISAR_ZERO_MEMORY_TYPE(result);
  result.pose.position.x = nanf("");
  return result;
}

// typedef struct HlrQrCodeList {
//   uint32_t size;
//   HlrQrCode* data;
// } HlrQrCodeList;

// typedef enum HlrQrMessageType HlrQrMessageType;
typedef enum HlrQrMessageType {
  HlrQrMessageType_IS_SUPPORTED = 0,
  HlrQrMessageType_REQUEST_ACCESS,
  HlrQrMessageType_START,
  HlrQrMessageType_STOP,
  // HlrQrMessageType_GET_LIST,
  HlrQrMessageType_ADDED,
  HlrQrMessageType_UPDATED,
  HlrQrMessageType_REMOVED,
  HlrQrMessageType_ENUMERATION_COMPLETED,

  HlrQrMessageType_COUNT,
  HlrQrMessageType_MIN = 0,
  // HlrQrMessageType_MIN = HlrQrMessageType_NONE + 1,
  HlrQrMessageType_MAX = HlrInputEventType_COUNT - 1,
  HlrQrMessageType__INT32 = 0xFFFFFFFF,  // force sizeof int32
} HlrQrMessageType;

typedef struct HlrQrIsSupported {
  bool is_supported;
} HlrQrIsSupported;

typedef struct HlrQrRequestAccess {
  HlrQrAccessStatus status;
} HlrQrRequestAccess;

// typedef struct HlrQrStart {} HlrQrStart;
// typedef struct HlrQrStop {} HlrQrStop;
// typedef struct HlrQrGetList {
//   HlrQrCodeList list;
// } HlrQrGetList;

struct HlrQrEventData {
  HlrQrCode code;
};
typedef struct HlrQrEventData HlrQrAdded;
typedef struct HlrQrEventData HlrQrUpdated;
typedef struct HlrQrEventData HlrQrRemoved;

#pragma endregion // HlrQrMessage

#pragma region // TODO: INTERNAL: move out of public header

typedef union HlrQrMessageData {
  HlrQrIsSupported is_supported;
  HlrQrRequestAccess request_access;
  // HlrQrMessageData_Start start;
  // HlrQrMessageData_Stop stop;
  // HlrQrMessageData_GetList get_list;
  HlrQrAdded added;
  HlrQrUpdated updated;
  HlrQrRemoved removed;
} HlrQrMessageData;

typedef struct HlrQrMessage {
  HlrQrMessageType type;
  HlrQrMessageData data;
  // HlrMessageData_QrType type;
  // HlrMessageData_QrData data;
} HlrQrMessage;

#pragma region // SpatialInput



#pragma endregion // SpatialInput

typedef enum HlrMessageType {
  HlrMessageType_NONE = 0,
  HlrMessageType_XrStereoPose = 1,
  HlrMessageType_InputEvent = 2,
  HlrMessageType_QrMessage = 3,
  HlrMessageType_SpatialInput = 4,
  HlrMessageType_COUNT,
  HlrMessageType_MIN = HlrMessageType_NONE,
  HlrMessageType_MAX = HlrMessageType_COUNT - 1,
  HlrMessageType__INT32 = 0xFFFFFFFF,  // force sizeof int32
} HlrMessageType;

typedef union HlrMessageData {
  HlrXrPose pose;
  HlrInputEvent input;
  HlrQrMessage qr;
  HlrSpatialInput spatialInput;
  // HlrMessageData_Pose pose;
  // HlrMessageData_Input input;
  // HlrMessageData_Qr qr;
} HlrMessageData;

typedef struct HlrMessage {
  HlrMessageType type;
  HlrMessageData data;
} HlrMessage;

#pragma endregion // TODO: INTERNAL: move out of public header

HLR_CPP_NS_END

#endif  // REMOTING_INPUT_TYPES_H
