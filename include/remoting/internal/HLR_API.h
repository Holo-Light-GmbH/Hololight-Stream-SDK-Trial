/**
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

#ifndef REMOTING_HLR_API_H
#define REMOTING_HLR_API_H

#pragma region // HLR_EXPORT

# ifdef __cplusplus
#   define HLR_EXTERN extern "C"
# else
#   define HLR_EXTERN
# endif  // __cplusplus

// HACK: because we dont have defines set up in build.gn at the moment
#define REMOTING_EXPORT

// This will be defined on all versions of Windows we care about (ARM/ARM64/x86/x64)
// https://docs.microsoft.com/en-us/cpp/preprocessor/predefined-macros?view=msvc-160#microsoft-specific-predefined-macros
#if defined(_WIN32)

# if defined(REMOTING_EXPORT)
#   define HLR_EXPORT HLR_EXTERN __declspec(dllexport)
# else
// Ref: https://docs.microsoft.com/en-us/cpp/build/importing-function-calls-using-declspec-dllimport?view=vs-2019
#   define HLR_EXPORT HLR_EXTERN __declspec(dllimport)
# endif

#elif defined(WEBRTC_ANDROID)
# if defined(REMOTING_EXPORT)
#   define HLR_EXPORT HLR_EXTERN __attribute__((visibility("default")))
# else
// https://clang.llvm.org/docs/LTOVisibility.html
// dllexport & dllimport both expose symbols so... ¯\_(ツ)_/¯
// TODO(viktor): research dllimport, i guess
#   define HLR_EXPORT HLR_EXTERN __attribute__((visibility("default")))
// #define HLR_EXPORT __attribute__((visibility("hidden")))
# endif
#endif // defined(WEBRTC_ANDROID)

#pragma endregion // HLR_EXPORT

#ifndef HLR_API
#define HLR_API(_return_value) HLR_EXPORT _return_value
#endif

#ifdef __cplusplus
#define HLR_CPP_NS_BEGIN namespace hlr {
#define HLR_CPP_NS_END }
#else
#define HLR_CPP_NS_BEGIN
#define HLR_CPP_NS_END
#endif

// TODO(viktor): create core.h/lang.h/preprocessor.h or sth
#ifndef IN
# define IN
#endif // IN

#ifndef OUT
# define OUT
#endif // OUT

#ifndef IN_OUT
# define IN_OUT
#endif // IN_OUT

//#define HLR_ARRAY_COUNT(array) (sizeof(array) / sizeof((array)[0]))

#ifndef HLR_STRINGIFY
# define HLR_STRINGIFY(value) #value
#endif // HLR_STRINGIFY

#ifndef HLR_STRINGIFY_DELAYED
# define HLR_STRINGIFY_DELAYED(value) HLR_STRINGIFY(value)
#endif // HLR_STRINGIFY

// unicode string
#ifndef HLR_WIDE
# define HLR_WIDE(value) L##value
#endif // HLR_WIDE

#ifndef HLR_WIDE_DELAYED
# define HLR_WIDE_DELAYED(value) HLR_WIDE(value)
#endif // HLR_WIDE_DELAYED

#ifndef HLR_STRINGIFY_W
//# define HLR_STRINGIFY_W(value) L#value // I think this is an msvc extension
//# define HLR_STRINGIFY_W(value) TEXT(#value) // winnt.h (windows only)
# define HLR_STRINGIFY_W(value) HLR_WIDE(#value)
#endif // HLR_STRINGIFY_W

#ifndef HLR_STRINGIFY_DELAYED_W
# define HLR_STRINGIFY_DELAYED_W(value) HLR_STRINGIFY_W(value)
#endif // HLR_STRINGIFY_DELAYED_W

#ifndef HLR_STRINGIFY_ENUM_CASE
#define HLR_STRINGIFY_ENUM_CASE(value) \
  case value:                     \
    return HLR_STRINGIFY(value)
#endif

#ifndef HLR_STRINGIFY_ENUM_CASE_W
#define HLR_STRINGIFY_ENUM_CASE_W(value) \
  case value:                       \
    return HLR_STRINGIFY_W(value)
#endif

#endif  // REMOTING_EXPORT_H
