using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace HoloLight.Isar
{
	public static class Fallbacks
	{
		public static T GetFallbackValue<T>(string name)
		{
			var value = fallbackValues[name];

			Debug.LogWarning("Config property '" + name + "' could not be read. Defaulting to '" + value + "'.");
			return (T)value;
		}

		public static readonly Role Role = Role.Server;
		public static readonly VideoSource VideoSource = VideoSource.D3D11;
		public static readonly Encoder Encoder = Encoder.H264_UWP;
		public static readonly Decoder Decoder = Decoder.H264_UWP;
		public static readonly Dictionary<string, string> IceServer = new Dictionary<string, string>()
		{
			{ "url", "stun:stun.l.google.com:19302" },
			{ "username", "" },
			{ "credential", "" }
		};
		public static readonly List<Dictionary<string, string>> IceServers = new List<Dictionary<string, string>>()
		{
			IceServer
		};
		public static readonly string DnssdName = "";
		public static readonly string[] DiagnosticOptions = { };
		public static readonly AudioDevice AudioDevice = AudioDevice.Default;
		public static readonly SignalingConfig SignalingConfig = new SignalingConfig() { ip = "0.0.0.0", port = 9999 };
		public static readonly RenderConfig RenderConfig = new RenderConfig() { name = "HoloLens2", width = 1440, height = 936, numViews = 2, bandwidth = 36000, framerate = 60 };

		static readonly Dictionary<string, object> fallbackValues = new Dictionary<string, object>()
		{
			{ "role", Role },
			{ "video-source", VideoSource },
			{ "encoder", Encoder },
			{ "decoder", Decoder },
			{ "ice-servers", IceServers },
			{ "url", IceServer["url"] },
			{ "username", IceServer["username"] },
			{ "credential", IceServer["credential"] },
			{ "dnssd-name", DnssdName },
			{ "diagnostic-options", DiagnosticOptions },
			{ "audio-device", AudioDevice },
			{ "signaling", SignalingConfig },
			{ "ip", SignalingConfig.ip },
			{ "port", SignalingConfig.port },
			{ "render-config", RenderConfig },
			{ "name", RenderConfig.name },
			{ "width", RenderConfig.width },
			{ "height", RenderConfig.height },
			{ "numViews", RenderConfig.numViews },
			{ "bandwidth", RenderConfig.bandwidth },
			{ "framerate", RenderConfig.framerate }
		};
	}

	/// <summary>
	/// Serialisable class for unpacking the current resolution of the camera
	/// </summary>
	[System.Serializable]
	public class IsarRemotingConfig
	{
		public Role role;
		[JsonProperty("video-source")]
		public VideoSource videoSource;
		public Encoder encoder;
		public Decoder decoder;
		[JsonProperty("ice-servers")]
		public List<Dictionary<string, string>> iceServers;
		[JsonProperty("dnssd-name")]
		public string dnssdName;
		[JsonProperty("diagnostic-options")]
		public string[] diagnosticOptions;
		[JsonProperty("audio-device")]
		public AudioDevice audioDevice;
		[JsonProperty("signaling")]
		public SignalingConfig signalingConfig;
		[JsonProperty("render-config")]
		public RenderConfig renderConfig;

		public static IsarRemotingConfig CreateFromJSON(string jsonString)
		{
			IsarRemotingConfig cfg = new IsarRemotingConfig();
			JObject parsedObject;
			try
			{
				parsedObject = JObject.Parse(jsonString);

				//Parse enums
				cfg.role = ParseEnum<Role>("role", (string)parsedObject["role"]);
				cfg.videoSource = ParseEnum<VideoSource>("video-source", (string)parsedObject["video-source"]);
				cfg.encoder = ParseEnum<Encoder>("encoder", (string)parsedObject["encoder"]);
				cfg.decoder = ParseEnum<Decoder>("decoder", (string)parsedObject["decoder"]);
				cfg.audioDevice = ParseEnum<AudioDevice>("audio-device", (string)parsedObject["audio-device"]);

				//Initialize iceservers list and populate
				JArray iceServersArr = (JArray)parsedObject["ice-servers"];
				cfg.iceServers = iceServersArr.Select(iceServerJSON => {
					Dictionary<string, string> iceServerEntry = new Dictionary<string, string>();

					iceServerEntry.Add("url", (string)iceServerJSON["url"] ?? Fallbacks.GetFallbackValue<string>("url"));
					iceServerEntry.Add("username", (string)iceServerJSON["username"] ?? Fallbacks.GetFallbackValue<string>("username"));
					iceServerEntry.Add("credential", (string)iceServerJSON["credential"] ?? Fallbacks.GetFallbackValue<string>("credential"));

					return iceServerEntry;
				}).ToList();

				JObject signalingConfigObj = (JObject)parsedObject["signaling"];
				cfg.signalingConfig = new SignalingConfig();
				cfg.signalingConfig.ip = (string)signalingConfigObj["ip"] ?? Fallbacks.GetFallbackValue<string>("ip");
				cfg.signalingConfig.port = signalingConfigObj["port"] != null ? (int)signalingConfigObj["port"] : Fallbacks.GetFallbackValue<int>("port");
				cfg.signalingConfig.CheckValues();

				cfg.dnssdName = (string)parsedObject["dnssd-name"];
				cfg.diagnosticOptions = ((JArray)parsedObject["diagnostic-options"]).Select(elem => (string)elem).ToArray();

				//Parse render config
				JObject renderConfigObj = (JObject)parsedObject["render-config"];
				cfg.renderConfig = new RenderConfig();
				cfg.renderConfig.name = (string)renderConfigObj["name"] ?? Fallbacks.GetFallbackValue<string>("name");
				cfg.renderConfig.width = renderConfigObj["width"] != null ? (int)renderConfigObj["width"] : Fallbacks.GetFallbackValue<int>("width");
				cfg.renderConfig.height = renderConfigObj["height"] != null ? (int)renderConfigObj["height"] : Fallbacks.GetFallbackValue<int>("height");
				cfg.renderConfig.numViews = renderConfigObj["numViews"] != null ? (int)renderConfigObj["numViews"] : Fallbacks.GetFallbackValue<int>("numViews");
				cfg.renderConfig.bandwidth = renderConfigObj["bandwidth"] != null ? (int)renderConfigObj["bandwidth"] : Fallbacks.GetFallbackValue<int>("bandwidth");
				cfg.renderConfig.framerate = renderConfigObj["framerate"] != null ? (int)renderConfigObj["framerate"] : Fallbacks.GetFallbackValue<int>("framerate");
				cfg.renderConfig.CheckValues();

				// Check for nulls and use fallbacks if necessary
				if (cfg.iceServers == null || cfg.iceServers.Count == 0) cfg.iceServers = Fallbacks.GetFallbackValue<List<Dictionary<string, string>>>("ice-servers");
				if (cfg.signalingConfig == null) cfg.signalingConfig = Fallbacks.GetFallbackValue<SignalingConfig>("signaling");
				if (cfg.dnssdName == null) cfg.dnssdName = Fallbacks.GetFallbackValue<string>("dnssd-name");
				if (cfg.diagnosticOptions == null) cfg.diagnosticOptions = Fallbacks.GetFallbackValue<string[]>("diagnostic-options");
				if (cfg.renderConfig == null) cfg.renderConfig = Fallbacks.GetFallbackValue<RenderConfig>("render-config");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Cannot read the config file, using fallback values instead.\nMessage:\n" + ex.Message + "\nStack Trace:\n" + ex.StackTrace);

				cfg.role = Fallbacks.Role;
				cfg.videoSource = Fallbacks.VideoSource;
				cfg.encoder = Fallbacks.Encoder;
				cfg.decoder = Fallbacks.Decoder;
				cfg.iceServers = Fallbacks.IceServers;
				cfg.signalingConfig = Fallbacks.SignalingConfig;
				cfg.dnssdName = Fallbacks.DnssdName;
				cfg.diagnosticOptions = Fallbacks.DiagnosticOptions;
				cfg.audioDevice = Fallbacks.AudioDevice;
				cfg.renderConfig = Fallbacks.RenderConfig;
			}

			return cfg;
		}

		public static string ToJSON(IsarRemotingConfig cfg)
		{
			return JsonConvert.SerializeObject(cfg, Formatting.Indented);
		}

		public void GetStructs(out RenderConfigStruct renderConfigStruct, out RemotingConfigStruct remotingConfigStruct, out IceServerSettings[] iceServerSettingsArray)
		{
			renderConfigStruct = renderConfig.AsStruct();

			remotingConfigStruct = new RemotingConfigStruct(role, videoSource, encoder, decoder, diagnosticOptions, audioDevice);

			iceServerSettingsArray = new IceServerSettings[iceServers.Count];
			for (int i = 0; i < iceServers.Count; i++)
			{
				iceServerSettingsArray[i] = new IceServerSettings(iceServers[i]);
			}
		}

		static T ParseEnum<T>(string key, string value) where T : Enum
		{
			Dictionary<string, Enum> enums = new Dictionary<string, Enum>()
			{
				{ "role_server", Role.Server },
				{ "role_client", Role.Client },
				{ "video-source_none", VideoSource.None },
				{ "video-source_d3d11", VideoSource.D3D11 },
				{ "video-source_webcam", VideoSource.Webcam },
				{ "encoder_builtin", Encoder.Builtin },
				{ "encoder_h264-uwp", Encoder.H264_UWP },
				{ "decoder_builtin", Decoder.Builtin },
				{ "decoder_h264-uwp", Decoder.H264_UWP },
				{ "audio-device_default", AudioDevice.Default },
				{ "audio-device_disabled", AudioDevice.Disabled }
			};

			if (!enums.TryGetValue(key+"_"+value, out Enum result)) return (T)Fallbacks.GetFallbackValue<T>(key);

			return (T)result;
		}
	}

	[System.Serializable]
	public class SignalingConfig
	{
		public string ip;
		public int port;

		public SignalingConfig()
		{
		}

		public void CheckValues()
		{
			System.Net.IPAddress ipAdress;

			if (!System.Net.IPAddress.TryParse(ip, out ipAdress))
			{
				Debug.LogWarning("Wrong IP Adress format, changing to " + Fallbacks.SignalingConfig.ip);
			}
			if (port < 1024 || port > 65535)
			{
				Debug.LogWarning("Port number is out of range, defaulting to " + Fallbacks.SignalingConfig.port);
				port = Fallbacks.SignalingConfig.port;
			}
		}
	}

	[System.Serializable]
	public class RenderConfig
	{
		public string name;
		public int width;
		public int height;
		public int numViews;
		public int bandwidth;
		public int framerate;

		public RenderConfig()
		{
		}

		public RenderConfig(RenderConfig other)
		{
			this.name = other.name;
			this.width = other.width;
			this.height = other.height;
			this.numViews = other.numViews;
			this.bandwidth = other.bandwidth;
			this.framerate = other.framerate;
		}

		public RenderConfigStruct AsStruct()
		{
			return new RenderConfigStruct((uint)width, (uint)height, (uint)numViews, (uint)bandwidth, (uint)framerate);
		}

		public static List<RenderConfig> CreateFromJSON(string jsonString)
		{
			List<RenderConfig> presets = new List<RenderConfig>();
			JObject presetsJSON = JObject.Parse(jsonString);
			presets = presetsJSON["presets"].Select(presetJSON => {
				RenderConfig newPreset = new RenderConfig();
				newPreset.name = (string)presetJSON["name"];
				newPreset.width = (int)presetJSON["width"];
				newPreset.height = (int)presetJSON["height"];
				newPreset.numViews = (int)presetJSON["numViews"];
				newPreset.bandwidth = (int)presetJSON["bandwidth"];
				newPreset.framerate = (int)presetJSON["framerate"];
				return newPreset;
			}).ToList();

			return presets;
		}

		public static string ToPresetsJSON(List<RenderConfig> presets)
		{
			JObject rootObj = new JObject(new JProperty("presets", JArray.FromObject(presets)));
			return rootObj.ToString();
		}

		public void CheckValues()
		{
			if (width % 2 != 0)
			{
				Debug.LogWarning("Odd number of width pixels, changing width to " + Fallbacks.RenderConfig.width);
				width = Fallbacks.RenderConfig.width;
			}
			if (height % 2 != 0)
			{
				Debug.LogWarning("Odd number of height pixels, changing height to " + Fallbacks.RenderConfig.height);
				height = Fallbacks.RenderConfig.height;
			}
			if (numViews < 1 || numViews > 2)
			{
				Debug.LogWarning("Number of view out of range, changing to " + Fallbacks.RenderConfig.numViews);
				numViews = Fallbacks.RenderConfig.numViews;
			}
			if (bandwidth < 0)
			{
				Debug.LogWarning("Negative bandwidth is not allowed, changing to " + Fallbacks.RenderConfig.bandwidth);
				bandwidth = Fallbacks.RenderConfig.bandwidth;
			}
			if (framerate < 0)
			{
				Debug.LogWarning("Negative frame rate is not allowed, changing to " + Fallbacks.RenderConfig.framerate);
				framerate = Fallbacks.RenderConfig.framerate;
			}
		}
	}
	
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Role
	{ 
		[EnumMember(Value = "server")]
		Server,
		[EnumMember(Value = "client")]
		Client
	};

	[JsonConverter(typeof(StringEnumConverter))]
	public enum VideoSource
	{
		[EnumMember(Value = "none")]
		None, 
		[EnumMember(Value = "d3d11")]
		D3D11, 
		[EnumMember(Value = "webcam")]
		Webcam
	};

	[JsonConverter(typeof(StringEnumConverter))]
	public enum Encoder
	{
		// libwebrtc built-in software encoders for H264, VP8, VP9 formats.
		[EnumMember(Value = "builtin")]
		Builtin,
		// Media Foundation (with Windows 10 features). H264 only.
		[EnumMember(Value = "h264-uwp")]
		H264_UWP
	};

	[JsonConverter(typeof(StringEnumConverter))]
	public enum Decoder
	{
		// libwebrtc built-in software decoders for H264, VP8, VP9 formats.
		[EnumMember(Value = "builtin")]
		Builtin,
		// Media Foundation (with Windows 10 features). H264 only.
		[EnumMember(Value = "h264-uwp")]
		H264_UWP
	};

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct IceServerSettings
	{
		public string url;
		public string username;
		public string password;

		public IceServerSettings(Dictionary<string, string> dict)
		{
			this.url = dict["url"];
			this.username = dict["username"];
			this.password = dict["credential"];
		}

		public IceServerSettings(string url, string username, string password)
		{
			this.url = url;
			this.username = username;
			this.password = password;
		}
	};

	public enum DiagnosticOptions
	{
		Disabled = 0,
		EnableTracing = (1 << 0),
		EnableEventLog = (1 << 1),
		EnableStatsCollector = (1 << 2)
	};

	[JsonConverter(typeof(StringEnumConverter))]
	public enum AudioDevice
	{
		[EnumMember(Value = "default")]
		Default,
		[EnumMember(Value = "disabled")]
		Disabled
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct RenderConfigStruct
	{
		public UInt32 width;
		public UInt32 height;
		public UInt32 numViews;
		public UInt32 encoderBitrateKbps;
		public UInt32 framerate;
		
		public RenderConfigStruct(RenderConfig cfg)
		{
			this.width = (uint)cfg.width;
			this.height = (uint)cfg.height;
			this.numViews =(uint) cfg.numViews;
			this.encoderBitrateKbps = (uint)cfg.bandwidth;
			this.framerate = (uint)cfg.framerate;
		}

		public RenderConfigStruct(uint width, uint height, uint numViews, uint bandwidth, uint framerate)
		{
			this.width = width;
			this.height = height;
			this.numViews = numViews;
			this.encoderBitrateKbps = bandwidth;
			this.framerate = framerate;

		}
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct RemotingConfigStruct
	{
		public Role role;
		public VideoSource videoSource;
		public Encoder encoder;
		public Decoder decoder;
		public DiagnosticOptions diagnosticOptions;
		public AudioDevice audioDevice;

		public RemotingConfigStruct(Role role, VideoSource videoSource, Encoder encoder, Decoder decoder, DiagnosticOptions diagnosticOptions, AudioDevice audioDevice)
		{
			this.role = role;
			this.videoSource = videoSource;
			this.encoder = encoder;
			this.decoder = decoder;
			this.diagnosticOptions = diagnosticOptions;
			this.audioDevice = audioDevice;
		}

		public RemotingConfigStruct(Role role, VideoSource videoSource, Encoder encoder, Decoder decoder, string[] diagnosticOptions, AudioDevice audioDevice)
		{
			this.role = role;
			this.videoSource = videoSource;
			this.encoder = encoder;
			this.decoder = decoder;
			this.audioDevice = audioDevice;
			this.diagnosticOptions = DiagnosticOptions.Disabled;

			foreach (var option in diagnosticOptions)
			{
				if (option == "tracing") this.diagnosticOptions |= DiagnosticOptions.EnableTracing;
				if (option == "event-log") this.diagnosticOptions |= DiagnosticOptions.EnableEventLog;
				if (option == "stats-collector") this.diagnosticOptions |= DiagnosticOptions.EnableStatsCollector;
			}
		}
	}
}
