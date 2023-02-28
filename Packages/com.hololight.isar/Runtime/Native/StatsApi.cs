/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace HoloLight.Isar.Native
{
	public class StringValueAttribute : Attribute
	{
		public string StringValue { get; protected set; }

		public StringValueAttribute(string value)
		{
			this.StringValue = value;
		}
	}

	public enum RTCStatsType : uint
	{
		[StringValue("codec")]
		Codec = 0,
		[StringValue("inbound-rtp")]
		InboundRtp = 1,
		[StringValue("outbound-rtp")]
		OutboundRtp = 2,
		[StringValue("remote-inbound-rtp")]
		RemoteInboundRtp = 3,
		[StringValue("remote-outbound-rtp")]
		RemoteOutboundRtp = 4,
		[StringValue("media-source")]
		MediaSource = 5,
		[StringValue("csrc")]
		Csrc = 6,
		[StringValue("peer-connection")]
		PeerConnection = 7,
		[StringValue("data-channel")]
		DataChannel = 8,
		[StringValue("stream")]
		Stream = 9,
		[StringValue("track")]
		Track = 10,
		[StringValue("transceiver")]
		Transceiver = 11,
		[StringValue("sender")]
		Sender = 12,
		[StringValue("receiver")]
		Receiver = 13,
		[StringValue("transport")]
		Transport = 14,
		[StringValue("sctp-transport")]
		SctpTransport = 15,
		[StringValue("candidate-pair")]
		CandidatePair = 16,
		[StringValue("local-candidate")]
		LocalCandidate = 17,
		[StringValue("remote-candidate")]
		RemoteCandidate = 18,
		[StringValue("certificate")]
		Certificate = 19,
		[StringValue("ice-server")]
		IceServer = 20,
	}

	public enum StatsMemberType : uint
	{
		Bool,    // bool
		Int32,   // int32_t
		Uint32,  // uint32_t
		Int64,   // int64_t
		Uint64,  // uint64_t
		Double,  // double
		String,  // std::string

		SequenceBool,    // std::vector<bool>
		SequenceInt32,   // std::vector<int32_t>
		SequenceUint32,  // std::vector<uint32_t>
		SequenceInt64,   // std::vector<int64_t>
		SequenceUint64,  // std::vector<uint64_t>
		SequenceDouble,  // std::vector<double>
		SequenceString,  // std::vector<std::string>
	}

	public struct Stats
	{
		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_get_stats_list")]
		public static extern IntPtr GetStatsList(IntPtr report, out ulong length, ref IntPtr types);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_get_json")]
		public static extern IntPtr GetJson(IntPtr report);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_get_id")]
		public static extern IntPtr GetId(IntPtr stats);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_get_type")]
		public static extern RTCStatsType GetType(IntPtr stats);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_get_timestamp")]
		public static extern long GetTimestamp(IntPtr stats);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_get_members")]
		public static extern IntPtr GetMembers(IntPtr stats, out ulong length);


		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_get_member_name")]
		public static extern IntPtr GetMemberName(IntPtr member);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_get_member_type")]
		public static extern StatsMemberType GetMemberType(IntPtr member);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_is_member_defined")]
		[return: MarshalAs(UnmanagedType.U1)]
		public static extern bool IsMemberDefined(IntPtr member);


		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_bool")]
		[return: MarshalAs(UnmanagedType.U1)]
		public static extern bool MemberGetBool(IntPtr member);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_int")]
		public static extern int MemberGetInt(IntPtr member);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_uint")]
		public static extern uint MemberGetUnsignedInt(IntPtr member);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_long")]
		public static extern long MemberGetLong(IntPtr member);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_ulong")]
		public static extern ulong MemberGetUnsignedLong(IntPtr member);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_double")]
		public static extern double MemberGetDouble(IntPtr member);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_string")]
		public static extern IntPtr MemberGetString(IntPtr member);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_bool_array")]
		public static extern IntPtr MemberGetBoolArray(IntPtr member, out ulong length);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_int_array")]
		public static extern IntPtr MemberGetIntArray(IntPtr member, out ulong length);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_uint_array")]
		public static extern IntPtr MemberGetUnsignedIntArray(IntPtr member, out ulong length);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_long_array")]
		public static extern IntPtr MemberGetLongArray(IntPtr member, out ulong length);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_ulong_array")]
		public static extern IntPtr MemberGetUnsignedLongArray(IntPtr member, out ulong length);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_double_array")]
		public static extern IntPtr MemberGetDoubleArray(IntPtr member, out ulong length);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_stats_member_get_string_array")]
		public static extern IntPtr MemberGetStringArray(IntPtr member, out ulong length);

	}
}