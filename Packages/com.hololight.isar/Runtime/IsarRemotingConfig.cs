using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;

namespace HoloLight.Isar
{
	/// <summary>
	/// Serialisable class for unpacking the current resolution of the camera
	/// </summary>
	[System.Serializable]
	public class IsarRemotingConfig
	{
		public string role;

		[JsonProperty("video-source")]
		public string videoSource;

		public string encoder;
		public string decoder;

		[JsonProperty("ice-servers")]
		public List<Dictionary<string, string>> iceServers;

		[JsonProperty("dnssd-name")]
		public string dnssdName;

		public RenderConfig renderConfig;

		public static IsarRemotingConfig CreateFromJSON(string jsonString)
		{
			JObject parsedObject = JObject.Parse(jsonString);
			IsarRemotingConfig cfg = new IsarRemotingConfig();

			//Parse string fields
			cfg.role = (string)parsedObject["role"];
			cfg.videoSource = (string)parsedObject["video-source"];
			cfg.encoder = (string)parsedObject["encoder"];
			cfg.decoder = (string)parsedObject["decoder"];
			cfg.dnssdName = (string)parsedObject["dnssd-name"];

			//Initialize iceservers list and populate
			JArray iceServersArr = (JArray)parsedObject["ice-servers"];
			cfg.iceServers = iceServersArr.Select(iceServerJSON => {
				Dictionary<string, string> iceServerEntry = new Dictionary<string, string>();
				iceServerEntry.Add("url", (string)iceServerJSON["url"]);
				return iceServerEntry;
			}).ToList();

			//Parse render config
			JObject renderConfigObj = (JObject)parsedObject["renderConfig"];
			cfg.renderConfig = new RenderConfig();
			cfg.renderConfig.name = (string)renderConfigObj["name"];
			cfg.renderConfig.width = (int)renderConfigObj["width"];
			cfg.renderConfig.height = (int)renderConfigObj["height"];
			cfg.renderConfig.numViews = (int)renderConfigObj["numViews"];
			cfg.renderConfig.bandwidth = (int)renderConfigObj["bandwidth"];

			return cfg;
		}

		public static string ToJSON(IsarRemotingConfig cfg)
		{
			return JsonConvert.SerializeObject(cfg, Formatting.Indented);
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
	}
}
