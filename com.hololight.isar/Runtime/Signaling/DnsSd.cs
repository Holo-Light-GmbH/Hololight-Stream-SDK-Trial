using System;
using System.IO;
using Makaretu.Dns;
using UnityEngine;

namespace HoloLight.Isar
{
	class DnsSd : IDisposable
	{
		private const string DNS_SD_SERVICE_NAME = "_hlrsignaling";
		private const string DNS_SD_PROTOCOL_NAME = "_tcp";
		private const string DNS_SD_KEY = "\"dnssd-name\"";

		private readonly MulticastService _service = new MulticastService();
		private ServiceDiscovery _serviceDiscovery;
		private readonly string _instanceName;
		private ServiceProfile _serviceProfile = null;

		public DnsSd(string configFilePath)
		{
			string name = ParseServiceNameFromConfig(configFilePath);

			// RFC6763 recommends a friendly name for instance names, so we use the currently logged in user here if
			// the developer didn't specify anything himself.
			_instanceName = string.IsNullOrWhiteSpace(name) ? Environment.UserName : name;
		}

		/// <summary>
		/// Parses a JSON string with the key "dnssd-name" from a file specified by
		/// <paramref name="configFilePath"/>.
		/// </summary>
		/// <param name="configFilePath">Path to remoting config file.</param>
		/// <returns>Returns empty string if there is no key of that name or the format is invalid.</returns>
		private string ParseServiceNameFromConfig(string configFilePath)
		{
			using (var stream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read))
			{
				using (var reader = new StreamReader(stream))
				{
					// this isn't pretty, but the config file isn't big so we just do it like that
					// because it's faster to implement than dealing with the build system and 3rd party libs.
					string json = reader.ReadToEnd();
					string key = DNS_SD_KEY;

					int keyIndex = json.IndexOf(key, StringComparison.Ordinal);
					if (keyIndex != -1)
					{
						// find next double quote after "dnssd-name"
						int indexAfterKey = keyIndex + key.Length;
						int openingQuoteIndex = json.IndexOf('"', indexAfterKey);

						if (openingQuoteIndex != -1)
						{
							// find closing quote index
							int closingQuoteIndex = json.IndexOf('"', openingQuoteIndex + 1);

							if (closingQuoteIndex != -1)
							{
								// get enclosed string without quotes
								int nameIndex = openingQuoteIndex + 1;
								int length = closingQuoteIndex - nameIndex;

								return json.Substring(nameIndex, length);
							}
						}
					}
				}
			}

			return "";
		}

		/// <summary>
		/// Start advertising a DNS-SD Service so that a client can
		/// connect without knowing our IP address.
		/// </summary>
		/// <param name="port">The port where the service is offered.</param>
		public void Advertise(int port)
		{
			// Workaround for IL2CPP being dumb and failing somewhere in SetSocketOption for
			// IPv6 without a proper error. Apparently it has been "fixed" in Unity 2019.3
			// (https://issuetracker.unity3d.com/issues/system-dot-net-cannot-set-ipv6-to-multicast-socket-argumentexception-is-thrown)
			// but it still doesn't advertise and there's no error message at all.
			// Well, whatever. Let's just stick to IPv4 like it's 1983.
			_service.UseIpv6 = false;

			// TODO: Known Issue: DNS-SD library tries to load MacOS dependency, even on Windows
			try
			{
				_service.Start();
			}
			catch (DllNotFoundException e)
			{
				Debug.LogException(e);
			}

			_serviceDiscovery = new ServiceDiscovery(_service);

			DomainName instanceName = _instanceName;
			DomainName serviceName = $"{DNS_SD_SERVICE_NAME}.{DNS_SD_PROTOCOL_NAME}";

			_serviceProfile = new ServiceProfile(instanceName, serviceName, (ushort)port);
			_serviceDiscovery.Advertise(_serviceProfile);
		}

		// TODO: implement DnsSd to be persistent & only Advertise/Unadvertise on Dis-/Connect
		public void Unadvertise()
		{
			if(_serviceProfile != null)
				_serviceDiscovery?.Unadvertise(_serviceProfile);
			_serviceProfile = null;
			_service.Stop();
		}

		public void Dispose()
		{
			Unadvertise();
			_serviceDiscovery?.Dispose();
			_service?.Dispose();
		}
	}
}
