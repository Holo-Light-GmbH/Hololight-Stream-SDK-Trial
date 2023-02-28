#ifndef REMOTING_STATS_API_H
#define REMOTING_STATS_API_H

#include "remoting/internal/HLR_API.h"

#include <stdint.h>

HLR_CPP_NS_BEGIN

HLR_API(const void**)
hlr_stats_get_stats_list(const void* report, size_t* length, uint32_t** types);

HLR_API(const char*)
hlr_stats_get_json(const void* stats);

HLR_API(const char*)
hlr_stats_get_id(const void* stats);

HLR_API(uint32_t)
hlr_stats_get_type(const void* stats);

HLR_API(int64_t)
hlr_stats_get_timestamp(const void* stats);

HLR_API(void*)
hlr_stats_get_members(const void* stats, size_t* length);

HLR_API(const char*)
hlr_stats_get_member_name(const void* member);

HLR_API(uint32_t)
hlr_stats_get_member_type(const void* member);

HLR_API(bool)
hlr_stats_is_member_defined(const void* member);

HLR_API(bool)
hlr_stats_member_get_bool(const void* member);

HLR_API(int32_t)
hlr_stats_member_get_int(const void* member);

HLR_API(uint32_t)
hlr_stats_member_get_uint(const void* member);

HLR_API(int64_t)
hlr_stats_member_get_long(const void* member);

HLR_API(uint64_t)
hlr_stats_member_get_ulong(const void* member);

HLR_API(double)
hlr_stats_member_get_double(const void* member);

HLR_API(const char *)
hlr_stats_member_get_string(const void* member);

HLR_API(bool*)
hlr_stats_member_get_bool_array(const void* member, size_t* length);

HLR_API(int32_t*)
hlr_stats_member_get_int_array(const void* member, size_t* length);

HLR_API(uint32_t*)
hlr_stats_member_get_uint_array(const void* member, size_t* length);

HLR_API(int64_t*)
hlr_stats_member_get_long_array(const void* member, size_t* length);

HLR_API(uint64_t*)
hlr_stats_member_get_ulong_array(const void* member, size_t* length);

HLR_API(double*)
hlr_stats_member_get_double_array(const void* member, size_t* length);

HLR_API(const char**)
hlr_stats_member_get_string_array(const void* member, size_t* length);

HLR_CPP_NS_END

#endif  //REMOTING_STATS_API_H
