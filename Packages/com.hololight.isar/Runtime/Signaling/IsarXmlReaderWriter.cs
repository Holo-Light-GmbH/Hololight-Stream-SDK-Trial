//#define ENABLE_LOGGING

/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

#pragma warning disable IDE0063 // Use simple 'using' statement // not supported in older C#/Unity versions

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Debug = HoloLight.Isar.Shared.Debug;

namespace HoloLight.Isar.Signaling
{
	internal static class Tokens
	{
		public const string SDP = "Sdp";
		public const string SDP_TYPE = "Type";
		public const string SDP_TYPE_OFFER = "Offer";
		public const string SDP_TYPE_ANSWER = "Answer";

		public const string VERSION = "Version";

		public const string ICE_CANDIDATE = "IceCandidate";
		public const string ICE_MID = "sdp-mid";
		public const string ICE_MLINE_INDEX = "mline-index";

		// feature support
		public const string DEPTH_ALPHA = "DepthAlphaEnabled";
	}

	internal class XmlElement
	{
		public string Name;
		public string Content;
		public List<XmlAttribute> Attributes = new List<XmlAttribute>();
	}

	internal class XmlAttribute
	{
		public string Name;
		public string Value;
	}

	internal static class IsarXmlReader /*: IDisposable*/
	{
#if false
		public void ReadVersion(out uint version)
		{
			//string bullshit = reader.GetAttribute(Tokens.VERSION_REMOTING_LIB); // TODO: can throw
			string versionStr = _reader.ReadElementContentAsString(); // TODO: can throw
			Debug.Assert(!string.IsNullOrEmpty(versionStr));
			Debug.WriteLine($"Read <{Tokens.VERSION}>{versionStr}</{Tokens.VERSION}>");
			version = uint.Parse(versionStr); // TODO: can throw
		}

		public void ReadSdp(out string type, out string sdp)
		{
			type = _reader.GetAttribute(Tokens.SDP_TYPE); // TODO: can throw
			sdp = _reader.ReadElementContentAsString(); // TODO: can throw
			//Debug.WriteLine($"Received SDP message {{type: {type}, content: {sdp}}}");
			Debug.WriteLine($"Read <{Tokens.SDP} {Tokens.SDP_TYPE}\"{type}\">{sdp}</{Tokens.SDP}>");
		}

		//public void ReadSdp(out Tokens.Sdp type, out string sdp)
		//{
		//	string typeStr = _reader.GetAttribute(Tokens.SDP_TYPE);
		//	type = (Tokens.Sdp)Enum.Parse(typeof(Tokens.Sdp), typeStr); // TODO: can throw
		//	sdp = _reader.ReadElementContentAsString(); // TODO: can throw
		//	//Debug.WriteLine($"Received SDP message {{type: {type}, content: {sdp}}}");
		//	Debug.WriteLine($"Read <{Tokens.SDP} {Tokens.SDP_TYPE}\"{type}\">{sdp}</{Tokens.SDP}>");
		//}

		public void ReadIceCandidate(out string mid, out int mLineIndex, out string candidate)
		{
			mid = _reader.GetAttribute(Tokens.ICE_MID); // TODO: can throw
			string mLineIndexStr = _reader.GetAttribute(Tokens.ICE_MLINE_INDEX); // TODO: can throw
			Debug.Assert(!string.IsNullOrEmpty(mLineIndexStr));
			mLineIndex = int.Parse(mLineIndexStr);
			candidate = _reader.ReadElementContentAsString(); // TODO: can throw
			Debug.WriteLine(
				$"Read <{Tokens.ICE_CANDIDATE} {Tokens.ICE_MID}=\"{mid}\" {Tokens.ICE_MLINE_INDEX}=\"{mLineIndex}\">" +
				$"{candidate}" +
				$"</{Tokens.ICE_CANDIDATE}>");
		}
#endif
		///////////////////// static variants

		public static void ReadVersion(XmlReader reader, out uint version, out bool supportDepthAlpha)
		{
			string supportDepthAlphaStr = reader.GetAttribute(Tokens.DEPTH_ALPHA); // TODO: can throw
			string versionStr = reader.ReadElementContentAsString(); // TODO: can throw
			Debug.Assert(!string.IsNullOrEmpty(versionStr));
#if ENABLE_LOGGING
			Debug.Log($"Parsed <{Tokens.VERSION} {Tokens.DEPTH_ALPHA}=\"{supportDepthAlphaStr}\">{versionStr}</{Tokens.VERSION}>");
#endif
			version = uint.Parse(versionStr); // TODO: can throw
			supportDepthAlpha = supportDepthAlphaStr == "1";
		}

		public static void ReadSdp(XmlReader reader, out string type, out string sdp)
		{
			type = reader.GetAttribute(Tokens.SDP_TYPE); // TODO: can throw
			sdp = reader.ReadElementContentAsString(); // TODO: can throw
			//Debug.WriteLine($"Received SDP message {{type: {type}, content: {sdp}}}");
#if ENABLE_LOGGING
			Debug.Log($"Parsed <{Tokens.SDP} {Tokens.SDP_TYPE}=\"{type}\">{sdp}</{Tokens.SDP}>");
#endif
		}

		public static void ReadIceCandidate(XmlReader reader, out string mid, out int mLineIndex, out string candidate)
		{
			mid = reader.GetAttribute(Tokens.ICE_MID); // TODO: can throw
			string mLineIndexStr = reader.GetAttribute(Tokens.ICE_MLINE_INDEX); // TODO: can throw
			Debug.Assert(!string.IsNullOrEmpty(mLineIndexStr));
			mLineIndex = int.Parse(mLineIndexStr);
			candidate = reader.ReadElementContentAsString(); // TODO: can throw
#if ENABLE_LOGGING
			Debug.Log(
				$"Parsed <{Tokens.ICE_CANDIDATE} {Tokens.ICE_MID}=\"{mid}\" {Tokens.ICE_MLINE_INDEX}=\"{mLineIndex}\">" +
				$"{candidate}" +
				$"</{Tokens.ICE_CANDIDATE}>");
#endif
		}
	}

	internal static class IsarXmlWriter
	{
		public static void WriteVersion(uint version, bool supportDepthAlpha, Stream stream)
		{
			var element = new XmlElement
			{
				Name = Tokens.VERSION,
				Content = version.ToString(CultureInfo.InvariantCulture)
			};
			var supportDepthAlphaStr = supportDepthAlpha ? "1" : "0";
			element.Attributes.Add(new XmlAttribute { Name = "DepthAlphaEnabled", Value = supportDepthAlphaStr });

			WriteElement(element, stream);
		}

		public static void WriteSdp(string type, string sdp, Stream stream)
		{
			var element = new XmlElement {Name = Tokens.SDP, Content = sdp};
			element.Attributes.Add(new XmlAttribute { Name = Tokens.SDP_TYPE, Value = type});
			WriteElement(element, stream);
		}

		public static void WriteIceCandidate(string mid, int mlineIndex, string candidate, Stream stream)
		{
			var element = new XmlElement
			{
				Name = Tokens.ICE_CANDIDATE,
				Attributes = new List<XmlAttribute>
				{
					new XmlAttribute {Name = Tokens.ICE_MID, Value = mid},
					new XmlAttribute
					{
						Name = Tokens.ICE_MLINE_INDEX,
						Value = mlineIndex.ToString(CultureInfo.InvariantCulture)
					}
				},
				Content = candidate
			};
			WriteElement(element, stream);
		}

		private static void WriteElement(XmlElement element, Stream stream)
		{
			// TODO: what does this additional bufferSize do?
			using (var streamWriter = new StreamWriter(stream, new UTF8Encoding(false, true), 1024, true))
			{
				var settings = new XmlWriterSettings();
				// TODO: get rid of xml declaration
				//settings.OmitXmlDeclaration = true;
				settings.CheckCharacters = true;
				//settings.ConformanceLevel = ConformanceLevel.Fragment;
				using (var writer = XmlWriter.Create(streamWriter, settings))
				{
					writer.WriteStartDocument();
					{
						writer.WriteStartElement(element.Name);
						{
							foreach (var attribute in element.Attributes)
							{
								writer.WriteAttributeString(attribute.Name, attribute.Value);
							}
							writer.WriteString(element.Content);
						}
						writer.WriteEndElement();
					}
					writer.WriteEndDocument();
				}
			}
		}

		// with byte[] (currently only used for version)
		// TODO: possibly buggy due to threading or sth.
		public static int WriteVersionAsBytes(uint version, ref byte[] output, int offset)
		{
			var element = new XmlElement
			{
				Name = Tokens.VERSION,
				Content = version.ToString(CultureInfo.InvariantCulture)
			};

			return WriteElementAsBytes(element, ref output, offset);
		}

		public static int WriteIceCandidateAsBytes(string mid, int mlineIndex, string candidate, ref byte[] output, int offset)
		{
			var element = new XmlElement
			{
				Name = Tokens.ICE_CANDIDATE,
				Attributes = new List<XmlAttribute>
				{
					new XmlAttribute {Name = Tokens.ICE_MID, Value = mid},
					new XmlAttribute
					{
						Name = Tokens.ICE_MLINE_INDEX,
						Value = mlineIndex.ToString(CultureInfo.InvariantCulture)
					}
				},
				Content = candidate
			};
			return WriteElementAsBytes(element, ref output, offset);
		}

		private static int WriteElementAsBytes(XmlElement element, ref byte[] output, int offset)
		{
			using (var stream = new MemoryStream(output, offset, output.Length - offset))
			{
				WriteElement(element, stream);
				return (int)stream.Position;
			}
		}
	}
}

#pragma warning restore IDE0063 // Use simple 'using' statement
